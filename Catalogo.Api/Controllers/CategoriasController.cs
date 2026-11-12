using Catalogo.Api.Data;
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly CatalogoDbContext _context;

    public CategoriasController(CatalogoDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Categoria>>> GetAll()
    {
        return Ok(await _context.Categorias.AsNoTracking().ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Categoria>> GetById(int id)
    {
        var categoria = await _context.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.IdCategoria == id);
        if (categoria == null)
        {
            return NotFound(new { mensaje = $"La categoría con Id {id} no fue encontrada en CatalogoDB." });
        }
        return Ok(categoria);
    }

    [HttpPost]
    public async Task<ActionResult<Categoria>> Create([FromBody] Categoria categoria)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = categoria.IdCategoria }, categoria);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Categoria categoria)
    {
        if (id <= 0)
        {
            return BadRequest(new { mensaje = "El IdCategoria proporcionado no es válido." });
        }

        var existente = await _context.Categorias.FindAsync(id);
        if (existente == null)
        {
            return NotFound(new { mensaje = $"La categoría con Id {id} no existe." });
        }

        existente.Nombre = categoria.Nombre;
        existente.Descripcion = categoria.Descripcion;
        await _context.SaveChangesAsync();

        return Ok(existente);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existente = await _context.Categorias.FindAsync(id);
        if (existente == null)
        {
            return NotFound(new { mensaje = $"La categoría con Id {id} no existe." });
        }

        bool tieneProductos = await _context.Productos.AnyAsync(p => p.IdCategoria == id);
        if (tieneProductos)
        {
            return BadRequest(new { mensaje = "No se puede eliminar la categoría porque tiene productos asociados (Regla de FK física)." });
        }

        _context.Categorias.Remove(existente);
        await _context.SaveChangesAsync();
        return Ok(new { mensaje = $"Categoría {id} eliminada correctamente." });
    }
}
