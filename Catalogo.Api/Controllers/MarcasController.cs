using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/marcas")]
public class MarcasController(CatalogoDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Marca>>> Listar()
    {
        return await db.Marcas.ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Marca>> Obtener(int id)
    {
        var marca = await db.Marcas.FindAsync(id);
        if (marca == null)
            return NotFound();

        return marca;
    }

    [HttpPost]
    public async Task<ActionResult<Marca>> Crear(Marca marca)
    {
        marca.IdMarca = 0;
        db.Marcas.Add(marca);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(Obtener), new { id = marca.IdMarca }, marca);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Marca>> Actualizar(int id, Marca datos)
    {
        var marca = await db.Marcas.FindAsync(id);
        if (marca == null)
            return NotFound();

        marca.Nombre = datos.Nombre;
        marca.PaisOrigen = datos.PaisOrigen;
        await db.SaveChangesAsync();

        return marca;
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var marca = await db.Marcas.FindAsync(id);
        if (marca == null)
            return NotFound();

        db.Marcas.Remove(marca);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 547 })
        {
            return BadRequest(new { mensaje = "Hay productos de esta marca." });
        }

        return NoContent();
    }
}
