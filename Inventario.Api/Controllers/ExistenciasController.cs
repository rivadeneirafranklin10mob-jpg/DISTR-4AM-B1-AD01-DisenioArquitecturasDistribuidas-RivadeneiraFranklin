using Inventario.Api.Data;
using Inventario.Api.Models;
using Inventario.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExistenciasController : ControllerBase
{
    private readonly InventarioDbContext _context;
    private readonly CatalogoClient _catalogoClient;

    public ExistenciasController(InventarioDbContext context, CatalogoClient catalogoClient)
    {
        _context = context;
        _catalogoClient = catalogoClient;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Existencia>>> GetAll([FromQuery] int? idProducto = null, [FromQuery] int? idBodega = null)
    {
        var query = _context.Existencias.Include(e => e.Bodega).AsNoTracking().AsQueryable();

        if (idProducto.HasValue)
        {
            query = query.Where(e => e.IdProducto == idProducto.Value);
        }

        if (idBodega.HasValue)
        {
            query = query.Where(e => e.IdBodega == idBodega.Value);
        }

        return Ok(await query.ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Existencia>> GetById(int id)
    {
        var existencia = await _context.Existencias
            .Include(e => e.Bodega)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.IdExistencia == id);

        if (existencia == null)
        {
            return NotFound(new { mensaje = $"La existencia con Id {id} no fue encontrada en InventarioDB." });
        }

        return Ok(existencia);
    }

    [HttpGet("producto/{idProducto:int}/stock-total")]
    public async Task<IActionResult> GetStockTotalPorProducto(int idProducto)
    {
        var existencias = await _context.Existencias
            .Where(e => e.IdProducto == idProducto)
            .ToListAsync();

        int stockTotal = existencias.Sum(e => e.Stock);
        return Ok(new
        {
            idProducto,
            stockTotal,
            bodegasConStock = existencias.Count(e => e.Stock > 0)
        });
    }

    [HttpPost]
    public async Task<ActionResult<Existencia>> Create([FromBody] Existencia existencia)
    {
        // 1. Validar FK FÍSICA: Bodega en InventarioDB
        bool existeBodega = await _context.Bodegas.AnyAsync(b => b.IdBodega == existencia.IdBodega);
        if (!existeBodega)
        {
            return BadRequest(new { mensaje = $"Error de FK física: La bodega con Id {existencia.IdBodega} no existe en InventarioDB." });
        }

        // 2. Validar FK LÓGICA: Producto en Catalogo.Api
        var validacionProducto = await _catalogoClient.ValidarProductoAsync(existencia.IdProducto);
        if (!validacionProducto.Existe)
        {
            return BadRequest(new { mensaje = validacionProducto.Mensaje });
        }

        // 3. Validar duplicidad de Existencia para (IdProducto, IdBodega)
        bool yaExiste = await _context.Existencias.AnyAsync(e =>
            e.IdProducto == existencia.IdProducto && e.IdBodega == existencia.IdBodega);
        if (yaExiste)
        {
            return BadRequest(new { mensaje = $"Ya existe un registro de existencia para el producto {existencia.IdProducto} en la bodega {existencia.IdBodega}." });
        }

        existencia.Bodega = null;
        _context.Existencias.Add(existencia);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = existencia.IdExistencia }, existencia);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Existencia existencia)
    {
        var existente = await _context.Existencias.FindAsync(id);
        if (existente == null)
        {
            return NotFound(new { mensaje = $"La existencia con Id {id} no fue encontrada." });
        }

        existente.Stock = existencia.Stock;
        await _context.SaveChangesAsync();

        return Ok(existente);
    }
}
