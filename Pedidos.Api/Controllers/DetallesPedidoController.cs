using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Models;
using Pedidos.Api.Services;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DetallesPedidoController : ControllerBase
{
    private readonly PedidosDbContext _context;
    private readonly IntegracionMicroserviciosService _integracionService;

    public DetallesPedidoController(PedidosDbContext context, IntegracionMicroserviciosService integracionService)
    {
        _context = context;
        _integracionService = integracionService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DetallePedido>>> GetAll([FromQuery] int? idPedido = null)
    {
        var query = _context.DetallesPedido.AsNoTracking().AsQueryable();
        if (idPedido.HasValue)
        {
            query = query.Where(d => d.IdPedido == idPedido.Value);
        }

        return Ok(await query.ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DetallePedido>> GetById(int id)
    {
        var detalle = await _context.DetallesPedido.AsNoTracking().FirstOrDefaultAsync(d => d.IdDetalle == id);
        if (detalle == null)
        {
            return NotFound(new { mensaje = $"El detalle de pedido con Id {id} no fue encontrado en PedidosDB." });
        }

        return Ok(detalle);
    }

    [HttpPost]
    public async Task<ActionResult<DetallePedido>> Create([FromBody] DetallePedido detalle)
    {
        // 1. Validar FK FÍSICA: Pedido en PedidosDB
        var pedido = await _context.Pedidos
            .Include(p => p.Detalles)
            .FirstOrDefaultAsync(p => p.IdPedido == detalle.IdPedido);

        if (pedido == null)
        {
            return BadRequest(new { mensaje = $"Error de FK física: El pedido con Id {detalle.IdPedido} no existe en PedidosDB." });
        }

        // 2. Validar FK LÓGICA: IdProducto en Catalogo.Api y Stock en Inventario.Api
        var validacion = await _integracionService.ValidarProductoYStockAsync(detalle.IdProducto, detalle.Cantidad);
        if (!validacion.Valido)
        {
            return BadRequest(new { mensaje = validacion.Mensaje });
        }

        _context.DetallesPedido.Add(detalle);
        await _context.SaveChangesAsync();

        // Recalcular total del pedido
        pedido.Total = await _context.DetallesPedido
            .Where(d => d.IdPedido == pedido.IdPedido)
            .SumAsync(d => d.Cantidad * d.PrecioUnitario);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = detalle.IdDetalle }, detalle);
    }
}
