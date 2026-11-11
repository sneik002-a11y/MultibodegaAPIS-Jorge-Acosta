using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController(PedidosDb db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Cliente>>> Listar()
    {
        return await db.Clientes.AsNoTracking().OrderBy(c => c.IdCliente).ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Cliente>> Obtener(int id)
    {
        var cliente = await db.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound("El cliente no existe.");

        return cliente;
    }

    [HttpPost]
    public async Task<ActionResult<Cliente>> Crear(Cliente datos)
    {
        datos.IdCliente = 0;
        datos.Nombres = datos.Nombres.Trim();
        datos.Cedula = datos.Cedula.Trim();
        datos.Correo = datos.Correo.Trim();
        db.Clientes.Add(datos);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return BadRequest("La cédula ya está registrada.");
        }

        return CreatedAtAction(nameof(Obtener), new { id = datos.IdCliente }, datos);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, Cliente datos)
    {
        var cliente = await db.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound("El cliente no existe.");

        cliente.Nombres = datos.Nombres.Trim();
        cliente.Cedula = datos.Cedula.Trim();
        cliente.Correo = datos.Correo.Trim();

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return BadRequest("La cédula ya está registrada.");
        }

        return Ok(cliente);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var cliente = await db.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound("El cliente no existe.");

        db.Clientes.Remove(cliente);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 547 })
        {
            return BadRequest("El cliente tiene pedidos registrados.");
        }

        return NoContent();
    }
}
