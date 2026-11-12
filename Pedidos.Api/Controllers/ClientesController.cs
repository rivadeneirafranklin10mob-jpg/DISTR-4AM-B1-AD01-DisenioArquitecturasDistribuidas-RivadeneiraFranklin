using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Models;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly PedidosDbContext _context;

    public ClientesController(PedidosDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cliente>>> GetAll()
    {
        return Ok(await _context.Clientes.AsNoTracking().ToListAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Cliente>> GetById(int id)
    {
        var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.IdCliente == id);
        if (cliente == null)
        {
            return NotFound(new { mensaje = $"El cliente con Id {id} no fue encontrado en PedidosDB." });
        }
        return Ok(cliente);
    }

    [HttpPost]
    public async Task<ActionResult<Cliente>> Create([FromBody] Cliente cliente)
    {
        bool cedulaDuplicada = await _context.Clientes.AnyAsync(c => c.Cedula == cliente.Cedula);
        if (cedulaDuplicada)
        {
            return BadRequest(new { mensaje = $"Ya existe un cliente registrado con la cédula '{cliente.Cedula}'." });
        }

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = cliente.IdCliente }, cliente);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Cliente cliente)
    {
        var existente = await _context.Clientes.FindAsync(id);
        if (existente == null)
        {
            return NotFound(new { mensaje = $"El cliente con Id {id} no existe." });
        }

        if (await _context.Clientes.AnyAsync(c => c.Cedula == cliente.Cedula && c.IdCliente != id))
        {
            return BadRequest(new { mensaje = $"La cédula '{cliente.Cedula}' ya pertenece a otro cliente." });
        }

        existente.Nombres = cliente.Nombres;
        existente.Cedula = cliente.Cedula;
        existente.Correo = cliente.Correo;

        await _context.SaveChangesAsync();
        return Ok(existente);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existente = await _context.Clientes.FindAsync(id);
        if (existente == null)
        {
            return NotFound(new { mensaje = $"El cliente con Id {id} no existe." });
        }

        bool tienePedidos = await _context.Pedidos.AnyAsync(p => p.IdCliente == id);
        if (tienePedidos)
        {
            return BadRequest(new { mensaje = "No se puede eliminar el cliente porque tiene pedidos registrados (Regla de FK física)." });
        }

        _context.Clientes.Remove(existente);
        await _context.SaveChangesAsync();
        return Ok(new { mensaje = $"Cliente {id} eliminado correctamente." });
    }
}
