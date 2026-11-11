using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/categorias")]
public class CategoriasController(CatalogoDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Categoria>>> Listar()
    {
        return await db.Categorias.ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Categoria>> Obtener(int id)
    {
        var categoria = await db.Categorias.FindAsync(id);
        if (categoria == null)
            return NotFound();

        return categoria;
    }

    [HttpPost]
    public async Task<ActionResult<Categoria>> Crear(Categoria categoria)
    {
        categoria.IdCategoria = 0;
        db.Categorias.Add(categoria);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(Obtener), new { id = categoria.IdCategoria }, categoria);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Categoria>> Actualizar(int id, Categoria datos)
    {
        var categoria = await db.Categorias.FindAsync(id);
        if (categoria == null)
            return NotFound();

        categoria.Nombre = datos.Nombre;
        categoria.Descripcion = datos.Descripcion;
        await db.SaveChangesAsync();

        return categoria;
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var categoria = await db.Categorias.FindAsync(id);
        if (categoria == null)
            return NotFound();

        db.Categorias.Remove(categoria);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 547 })
        {
            return BadRequest(new { mensaje = "Hay productos en esta categoría." });
        }

        return NoContent();
    }
}