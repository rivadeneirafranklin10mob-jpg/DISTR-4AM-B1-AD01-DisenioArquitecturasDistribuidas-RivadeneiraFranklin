using Inventario.Api.Data;
using Inventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BodegasController : ControllerBase
{
    private readonly InventarioDbContext _context;

    public BodegasController(InventarioDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Bodega>>> GetAll()
    {
        return Ok(await _context.Bodegas.AsNoTracking().ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Bodega>> GetById(int id)
    {
        var bodega = await _context.Bodegas.AsNoTracking().FirstOrDefaultAsync(b => b.IdBodega == id);
        if (bodega == null)
        {
            return NotFound(new { mensaje = $"La bodega con Id {id} no fue encontrada en InventarioDB." });
        }
        return Ok(bodega);
    }

    [HttpPost]
    public async Task<ActionResult<Bodega>> Create([FromBody] Bodega bodega)
    {
        _context.Bodegas.Add(bodega);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = bodega.IdBodega }, bodega);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Bodega bodega)
    {
        var existente = await _context.Bodegas.FindAsync(id);
        if (existente == null)
        {
            return NotFound(new { mensaje = $"La bodega con Id {id} no existe." });
        }

        existente.Nombre = bodega.Nombre;
        existente.Ciudad = bodega.Ciudad;
        existente.Direccion = bodega.Direccion;

        await _context.SaveChangesAsync();
        return Ok(existente);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existente = await _context.Bodegas.FindAsync(id);
        if (existente == null)
        {
            return NotFound(new { mensaje = $"La bodega con Id {id} no existe." });
        }

        bool tieneReferencias = await _context.Existencias.AnyAsync(e => e.IdBodega == id)
            || await _context.MovimientosInventario.AnyAsync(m => m.IdBodega == id)
            || await _context.ReservasStock.AnyAsync(r => r.IdBodega == id);

        if (tieneReferencias)
        {
            return BadRequest(new { mensaje = "No se puede eliminar la bodega porque tiene existencias, movimientos o reservas asociadas (Regla de FK física)." });
        }

        _context.Bodegas.Remove(existente);
        await _context.SaveChangesAsync();
        return Ok(new { mensaje = $"Bodega {id} eliminada correctamente." });
    }
}
