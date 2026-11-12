using Catalogo.Api.Data;
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly CatalogoDbContext _context;

    public ProductosController(CatalogoDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Producto>>> GetAll()
    {
        var productos = await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .AsNoTracking()
            .ToListAsync();
        return Ok(productos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Producto>> GetById(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdProducto == id);

        if (producto == null)
        {
            return NotFound(new { mensaje = $"El producto con Id {id} no fue encontrado en CatalogoDB." });
        }

        return Ok(producto);
    }

    [HttpPost]
    public async Task<ActionResult<Producto>> Create([FromBody] Producto producto)
    {
        // Validación de FK física: Categoria
        bool existeCategoria = await _context.Categorias.AnyAsync(c => c.IdCategoria == producto.IdCategoria);
        if (!existeCategoria)
        {
            return BadRequest(new { mensaje = $"Error de FK física: La categoría con Id {producto.IdCategoria} no existe en CatalogoDB." });
        }

        // Validación de FK física: Marca
        bool existeMarca = await _context.Marcas.AnyAsync(m => m.IdMarca == producto.IdMarca);
        if (!existeMarca)
        {
            return BadRequest(new { mensaje = $"Error de FK física: La marca con Id {producto.IdMarca} no existe en CatalogoDB." });
        }

        // Validación de unicidad de SKU
        bool skuDuplicado = await _context.Productos.AnyAsync(p => p.SKU == producto.SKU);
        if (skuDuplicado)
        {
            return BadRequest(new { mensaje = $"Ya existe un producto con el código SKU '{producto.SKU}'." });
        }

        producto.Categoria = null;
        producto.Marca = null;

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        var creado = await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .FirstAsync(p => p.IdProducto == producto.IdProducto);

        return CreatedAtAction(nameof(GetById), new { id = creado.IdProducto }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Producto producto)
    {
        var existente = await _context.Productos.FindAsync(id);
        if (existente == null)
        {
            return NotFound(new { mensaje = $"El producto con Id {id} no fue encontrado en CatalogoDB." });
        }

        if (!await _context.Categorias.AnyAsync(c => c.IdCategoria == producto.IdCategoria))
        {
            return BadRequest(new { mensaje = $"Error de FK física: La categoría con Id {producto.IdCategoria} no existe." });
        }

        if (!await _context.Marcas.AnyAsync(m => m.IdMarca == producto.IdMarca))
        {
            return BadRequest(new { mensaje = $"Error de FK física: La marca con Id {producto.IdMarca} no existe." });
        }

        if (await _context.Productos.AnyAsync(p => p.SKU == producto.SKU && p.IdProducto != id))
        {
            return BadRequest(new { mensaje = $"El SKU '{producto.SKU}' ya pertenece a otro producto." });
        }

        existente.IdCategoria = producto.IdCategoria;
        existente.IdMarca = producto.IdMarca;
        existente.Nombre = producto.Nombre;
        existente.SKU = producto.SKU;
        existente.Precio = producto.Precio;
        existente.Estado = producto.Estado;

        await _context.SaveChangesAsync();
        return Ok(existente);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existente = await _context.Productos.FindAsync(id);
        if (existente == null)
        {
            return NotFound(new { mensaje = $"El producto con Id {id} no fue encontrado." });
        }

        _context.Productos.Remove(existente);
        await _context.SaveChangesAsync();
        return Ok(new { mensaje = $"Producto {id} eliminado correctamente." });
    }
}
