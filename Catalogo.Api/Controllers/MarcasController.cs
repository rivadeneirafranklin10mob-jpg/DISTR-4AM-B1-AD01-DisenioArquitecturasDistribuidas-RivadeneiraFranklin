using Catalogo.Api.Data;
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarcasController : ControllerBase
{
    private readonly CatalogoDbContext _context;

    public MarcasController(CatalogoDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Marca>>> GetAll()
    {
        return Ok(await _context.Marcas.AsNoTracking().ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Marca>> GetById(int id)
    {
        var marca = await _context.Marcas.AsNoTracking().FirstOrDefaultAsync(m => m.IdMarca == id);
        if (marca == null)
        {
            return NotFound(new { mensaje = $"La marca con Id {id} no fue encontrada en CatalogoDB." });
        }
        return Ok(marca);
    }

    [HttpPost]
    public async Task<ActionResult<Marca>> Create([FromBody] Marca marca)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _context.Marcas.Add(marca);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = marca.IdMarca }, marca);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Marca marca)
    {
        var existente = await _context.Marcas.FindAsync(id);
        if (existente == null)
        {
            return NotFound(new { mensaje = $"La marca con Id {id} no existe." });
        }

        existente.Nombre = marca.Nombre;
        existente.PaisOrigen = marca.PaisOrigen;
        await _context.SaveChangesAsync();

        return Ok(existente);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existente = await _context.Marcas.FindAsync(id);
        if (existente == null)
        {
            return NotFound(new { mensaje = $"La marca con Id {id} no existe." });
        }

        bool tieneProductos = await _context.Productos.AnyAsync(p => p.IdMarca == id);
        if (tieneProductos)
        {
            return BadRequest(new { mensaje = "No se puede eliminar la marca porque tiene productos asociados (Regla de FK física)." });
        }

        _context.Marcas.Remove(existente);
        await _context.SaveChangesAsync();
        return Ok(new { mensaje = $"Marca {id} eliminada correctamente." });
    }
}
