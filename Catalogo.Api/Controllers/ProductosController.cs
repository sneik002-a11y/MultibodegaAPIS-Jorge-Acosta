using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/productos")]
public class ProductosController(CatalogoDbContext db, IHttpClientFactory clientes) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Producto>>> Listar()
    {
        return await db.Productos.ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Producto>> Obtener(int id)
    {
        var producto = await db.Productos.FindAsync(id);
        if (producto == null)
            return NotFound();

        return producto;
    }

    [HttpGet("{id:int}/existencias")]
    public async Task<IActionResult> ObtenerExistencias(int id)
    {
        if (!await db.Productos.AnyAsync(producto => producto.IdProducto == id))
            return NotFound();

        try
        {
            using var cliente = clientes.CreateClient("Inventario");
            using var respuesta = await cliente.GetAsync($"/api/existencias/producto/{id}");
            if (!respuesta.IsSuccessStatusCode)
                return StatusCode(502, new { mensaje = "No se pudieron consultar las existencias." });

            var json = await respuesta.Content.ReadAsStringAsync();
            return Content(json, "application/json");
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, new { mensaje = "Inventario no está disponible." });
        }
        catch (TaskCanceledException)
        {
            return StatusCode(503, new { mensaje = "Inventario tardó demasiado en responder." });
        }
    }

    [HttpPost]
    public async Task<ActionResult<Producto>> Crear(Producto producto)
    {
        if (decimal.Round(producto.Precio, 2) != producto.Precio)
            return BadRequest(new { mensaje = "El precio admite hasta dos decimales." });

        producto.IdProducto = 0;
        producto.SKU = producto.SKU.Trim();
        db.Productos.Add(producto);
        var error = await GuardarProducto();
        if (error != null)
            return error;

        return CreatedAtAction(nameof(Obtener), new { id = producto.IdProducto }, producto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Producto>> Actualizar(int id, Producto datos)
    {
        var producto = await db.Productos.FindAsync(id);
        if (producto == null)
            return NotFound();
        if (decimal.Round(datos.Precio, 2) != datos.Precio)
            return BadRequest(new { mensaje = "El precio admite hasta dos decimales." });

        producto.IdCategoria = datos.IdCategoria;
        producto.IdMarca = datos.IdMarca;
        producto.Nombre = datos.Nombre;
        producto.SKU = datos.SKU.Trim();
        producto.Precio = datos.Precio;
        producto.Estado = datos.Estado;

        var error = await GuardarProducto();
        if (error != null)
            return error;

        return producto;
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Desactivar(int id)
    {
        var producto = await db.Productos.FindAsync(id);
        if (producto == null)
            return NotFound();

        // Inventario y Pedidos guardan este ID como referencia.
        producto.Estado = false;
        await db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<ActionResult?> GuardarProducto()
    {
        try
        {
            await db.SaveChangesAsync();
            return null;
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 547 })
        {
            return BadRequest(new { mensaje = "La categoría y la marca deben existir." });
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return BadRequest(new { mensaje = "Ya existe un producto con ese SKU." });
        }
    }
}
