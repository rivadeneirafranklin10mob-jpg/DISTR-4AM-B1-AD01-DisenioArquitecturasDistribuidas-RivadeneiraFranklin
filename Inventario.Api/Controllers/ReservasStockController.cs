using Inventario.Api.Data;
using Inventario.Api.Models;
using Inventario.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservasStockController : ControllerBase
{
    private readonly InventarioDbContext _context;
    private readonly CatalogoClient _catalogoClient;

    public ReservasStockController(InventarioDbContext context, CatalogoClient catalogoClient)
    {
        _context = context;
        _catalogoClient = catalogoClient;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservaStock>>> GetAll()
    {
        return Ok(await _context.ReservasStock
            .Include(r => r.Bodega)
            .AsNoTracking()
            .ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservaStock>> GetById(int id)
    {
        var reserva = await _context.ReservasStock
            .Include(r => r.Bodega)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.IdReserva == id);

        if (reserva == null)
        {
            return NotFound(new { mensaje = $"La reserva con Id {id} no fue encontrada en InventarioDB." });
        }

        return Ok(reserva);
    }

    [HttpPost]
    public async Task<ActionResult<ReservaStock>> Create([FromBody] ReservaStock reserva)
    {
        // 1. Validar FK FÍSICA: Bodega
        bool existeBodega = await _context.Bodegas.AnyAsync(b => b.IdBodega == reserva.IdBodega);
        if (!existeBodega)
        {
            return BadRequest(new { mensaje = $"Error de FK física: La bodega con Id {reserva.IdBodega} no existe en InventarioDB." });
        }

        // 2. Validar FK LÓGICA: Producto en Catalogo.Api
        var validacionProducto = await _catalogoClient.ValidarProductoAsync(reserva.IdProducto);
        if (!validacionProducto.Existe)
        {
            return BadRequest(new { mensaje = validacionProducto.Mensaje });
        }

        // 3. Verificar disponibilidad y descontar stock en la bodega
        var existencia = await _context.Existencias.FirstOrDefaultAsync(e =>
            e.IdProducto == reserva.IdProducto && e.IdBodega == reserva.IdBodega);

        if (existencia == null || existencia.Stock < reserva.Cantidad)
        {
            int disponible = existencia?.Stock ?? 0;
            return BadRequest(new
            {
                mensaje = $"Stock insuficiente para reservar el producto {reserva.IdProducto} en la bodega {reserva.IdBodega}. Disponible: {disponible}, solicitado: {reserva.Cantidad}."
            });
        }

        existencia.Stock -= reserva.Cantidad;
        reserva.Bodega = null;

        _context.ReservasStock.Add(reserva);
        _context.MovimientosInventario.Add(new MovimientoInventario
        {
            IdProducto = reserva.IdProducto,
            IdBodega = reserva.IdBodega,
            Tipo = "SALIDA",
            Cantidad = reserva.Cantidad,
            Fecha = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = reserva.IdReserva }, reserva);
    }

    [HttpPut("{id:int}/estado")]
    public async Task<IActionResult> UpdateEstado(int id, [FromBody] string nuevoEstado)
    {
        if (nuevoEstado != "Reservado" && nuevoEstado != "Confirmado" && nuevoEstado != "Cancelado")
        {
            return BadRequest(new { mensaje = "El estado debe ser 'Reservado', 'Confirmado' o 'Cancelado'." });
        }

        var reserva = await _context.ReservasStock.FindAsync(id);
        if (reserva == null)
        {
            return NotFound(new { mensaje = $"La reserva con Id {id} no fue encontrada." });
        }

        reserva.Estado = nuevoEstado;
        await _context.SaveChangesAsync();

        return Ok(reserva);
    }
}
