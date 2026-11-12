using Inventario.Api.Data;
using Inventario.Api.Models;
using Inventario.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovimientosInventarioController : ControllerBase
{
    private readonly InventarioDbContext _context;
    private readonly CatalogoClient _catalogoClient;

    public MovimientosInventarioController(InventarioDbContext context, CatalogoClient catalogoClient)
    {
        _context = context;
        _catalogoClient = catalogoClient;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MovimientoInventario>>> GetAll()
    {
        return Ok(await _context.MovimientosInventario
            .Include(m => m.Bodega)
            .OrderByDescending(m => m.Fecha)
            .AsNoTracking()
            .ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovimientoInventario>> GetById(int id)
    {
        var movimiento = await _context.MovimientosInventario
            .Include(m => m.Bodega)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.IdMovimiento == id);

        if (movimiento == null)
        {
            return NotFound(new { mensaje = $"El movimiento con Id {id} no fue encontrado en InventarioDB." });
        }

        return Ok(movimiento);
    }

    [HttpPost]
    public async Task<ActionResult<MovimientoInventario>> Create([FromBody] MovimientoInventario movimiento)
    {
        // 1. Validar FK FÍSICA: Bodega en InventarioDB
        bool existeBodega = await _context.Bodegas.AnyAsync(b => b.IdBodega == movimiento.IdBodega);
        if (!existeBodega)
        {
            return BadRequest(new { mensaje = $"Error de FK física: La bodega con Id {movimiento.IdBodega} no existe en InventarioDB." });
        }

        // 2. Validar FK LÓGICA: Producto en Catalogo.Api
        var validacionProducto = await _catalogoClient.ValidarProductoAsync(movimiento.IdProducto);
        if (!validacionProducto.Existe)
        {
            return BadRequest(new { mensaje = validacionProducto.Mensaje });
        }

        // 3. Actualizar Existencia según el Tipo de movimiento
        var existencia = await _context.Existencias.FirstOrDefaultAsync(e =>
            e.IdProducto == movimiento.IdProducto && e.IdBodega == movimiento.IdBodega);

        if (movimiento.Tipo == "SALIDA")
        {
            if (existencia == null || existencia.Stock < movimiento.Cantidad)
            {
                int disponible = existencia?.Stock ?? 0;
                return BadRequest(new
                {
                    mensaje = $"Stock insuficiente en la bodega {movimiento.IdBodega} para el producto {movimiento.IdProducto}. Disponible: {disponible}, solicitado: {movimiento.Cantidad}."
                });
            }
            existencia.Stock -= movimiento.Cantidad;
        }
        else if (movimiento.Tipo == "ENTRADA")
        {
            if (existencia == null)
            {
                existencia = new Existencia
                {
                    IdProducto = movimiento.IdProducto,
                    IdBodega = movimiento.IdBodega,
                    Stock = movimiento.Cantidad
                };
                _context.Existencias.Add(existencia);
            }
            else
            {
                existencia.Stock += movimiento.Cantidad;
            }
        }
        else if (movimiento.Tipo == "AJUSTE")
        {
            if (existencia == null)
            {
                existencia = new Existencia
                {
                    IdProducto = movimiento.IdProducto,
                    IdBodega = movimiento.IdBodega,
                    Stock = movimiento.Cantidad
                };
                _context.Existencias.Add(existencia);
            }
            else
            {
                existencia.Stock = movimiento.Cantidad;
            }
        }

        movimiento.Fecha = DateTime.UtcNow;
        movimiento.Bodega = null;
        _context.MovimientosInventario.Add(movimiento);

        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = movimiento.IdMovimiento }, movimiento);
    }
}
