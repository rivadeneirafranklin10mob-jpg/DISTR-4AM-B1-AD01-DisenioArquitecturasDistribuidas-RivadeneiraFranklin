using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Models;
using Pedidos.Api.Services;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly PedidosDbContext _context;
    private readonly IntegracionMicroserviciosService _integracionService;

    public PedidosController(PedidosDbContext context, IntegracionMicroserviciosService integracionService)
    {
        _context = context;
        _integracionService = integracionService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetAll()
    {
        var pedidos = await _context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Detalles)
            .OrderByDescending(p => p.Fecha)
            .AsNoTracking()
            .ToListAsync();

        return Ok(pedidos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Pedido>> GetById(int id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Detalles)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPedido == id);

        if (pedido == null)
        {
            return NotFound(new { mensaje = $"El pedido con Id {id} no fue encontrado en PedidosDB." });
        }

        return Ok(pedido);
    }

    [HttpPost]
    public async Task<ActionResult<Pedido>> Create([FromBody] Pedido pedido)
    {
        // 1. Validar FK FÍSICA: Cliente en PedidosDB
        bool existeCliente = await _context.Clientes.AnyAsync(c => c.IdCliente == pedido.IdCliente);
        if (!existeCliente)
        {
            return BadRequest(new { mensaje = $"Error de FK física: El cliente con Id {pedido.IdCliente} no existe en PedidosDB." });
        }

        // 2. Validar FK LÓGICA (IdProducto) y Stock para cada detalle si se envían detalles
        decimal totalCalculado = 0m;
        if (pedido.Detalles != null && pedido.Detalles.Count > 0)
        {
            foreach (var detalle in pedido.Detalles)
            {
                var validacion = await _integracionService.ValidarProductoYStockAsync(detalle.IdProducto, detalle.Cantidad);
                if (!validacion.Valido)
                {
                    return BadRequest(new { mensaje = validacion.Mensaje });
                }

                if (detalle.PrecioUnitario <= 0 && validacion.Producto != null)
                {
                    detalle.PrecioUnitario = validacion.Producto.Precio;
                }

                totalCalculado += detalle.Cantidad * detalle.PrecioUnitario;
            }
            pedido.Total = totalCalculado;
        }

        pedido.Fecha = DateTime.UtcNow;
        pedido.Cliente = null;

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        var creado = await _context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Detalles)
            .FirstAsync(p => p.IdPedido == pedido.IdPedido);

        return CreatedAtAction(nameof(GetById), new { id = creado.IdPedido }, creado);
    }

    [HttpPut("{id:int}/estado")]
    public async Task<IActionResult> UpdateEstado(int id, [FromBody] string nuevoEstado)
    {
        if (nuevoEstado != "Pendiente" && nuevoEstado != "Confirmado" && nuevoEstado != "Cancelado" && nuevoEstado != "Entregado")
        {
            return BadRequest(new { mensaje = "El estado debe ser 'Pendiente', 'Confirmado', 'Cancelado' o 'Entregado'." });
        }

        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido == null)
        {
            return NotFound(new { mensaje = $"El pedido con Id {id} no fue encontrado." });
        }

        pedido.Estado = nuevoEstado;
        await _context.SaveChangesAsync();
        return Ok(pedido);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido == null)
        {
            return NotFound(new { mensaje = $"El pedido con Id {id} no fue encontrado." });
        }

        _context.Pedidos.Remove(pedido);
        await _context.SaveChangesAsync();
        return Ok(new { mensaje = $"Pedido {id} y sus detalles fueron eliminados correctamente." });
    }
}
