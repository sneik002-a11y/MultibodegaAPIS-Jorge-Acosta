using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
public class PedidosController(PedidosDb db, IHttpClientFactory servicios) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Pedido>>> Listar()
    {
        return await db.Pedidos.AsNoTracking().Include(p => p.Detalles)
            .OrderBy(p => p.IdPedido).ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Pedido>> Obtener(int id)
    {
        var pedido = await db.Pedidos.Include(p => p.Detalles)
            .FirstOrDefaultAsync(p => p.IdPedido == id);
        if (pedido == null)
            return NotFound("El pedido no existe.");

        return pedido;
    }

    [HttpPost]
    public async Task<ActionResult<Pedido>> Crear(NuevoPedido datos)
    {
        if (datos.Detalles.Any(d => d == null))
            return BadRequest("Cada detalle debe incluir un producto y una cantidad.");

        if (!await db.Clientes.AnyAsync(c => c.IdCliente == datos.IdCliente))
            return NotFound("El cliente no existe.");

        if (datos.Detalles.Select(d => d.IdProducto).Distinct().Count() != datos.Detalles.Count)
            return BadRequest("Cada producto debe aparecer una sola vez en el pedido.");

        var pedido = new Pedido { IdCliente = datos.IdCliente, Fecha = DateTime.UtcNow };
        var catalogo = servicios.CreateClient("Catalogo");
        var inventario = servicios.CreateClient("Inventario");

        try
        {
            foreach (var detalle in datos.Detalles)
            {
                using var respuesta = await catalogo.GetAsync($"/api/productos/{detalle.IdProducto}");
                if (respuesta.StatusCode == HttpStatusCode.NotFound)
                    return NotFound($"El producto {detalle.IdProducto} no existe.");

                respuesta.EnsureSuccessStatusCode();
                var producto = await respuesta.Content.ReadFromJsonAsync<ProductoCatalogo>();
                if (producto == null || !producto.Estado)
                    return BadRequest($"El producto {detalle.IdProducto} está inactivo.");

                var existencias = await inventario.GetFromJsonAsync<List<StockBodega>>(
                    $"/api/existencias/producto/{detalle.IdProducto}");
                var disponible = existencias?.Sum(e => (long)e.Disponible) ?? 0;
                if (disponible < detalle.Cantidad)
                    return BadRequest($"No hay stock suficiente del producto {detalle.IdProducto}.");

                pedido.Detalles.Add(new DetallePedido
                {
                    IdProducto = detalle.IdProducto,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = producto.Precio
                });
                pedido.Total += producto.Precio * detalle.Cantidad;
                if (pedido.Total > 9999999999999999.99m)
                    return BadRequest("El total del pedido supera el valor permitido.");
            }
        }
        catch (Exception error) when (error is HttpRequestException or TaskCanceledException)
        {
            return StatusCode(503, "No se pudo consultar el catálogo o el inventario.");
        }

        db.Pedidos.Add(pedido);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 547 })
        {
            return NotFound("El cliente no existe.");
        }

        return CreatedAtAction(nameof(Obtener), new { id = pedido.IdPedido }, pedido);
    }

    [HttpPut("{id:int}/estado")]
    public async Task<ActionResult<Pedido>> CambiarEstado(int id, EstadoPedido datos)
    {
        if (datos.Estado is not ("Confirmado" or "Cancelado"))
            return BadRequest("El estado debe ser Confirmado o Cancelado.");

        var pedido = await db.Pedidos.Include(p => p.Detalles)
            .FirstOrDefaultAsync(p => p.IdPedido == id);
        if (pedido == null)
            return NotFound("El pedido no existe.");

        if (pedido.Estado == datos.Estado)
            return Ok(pedido);

        if (pedido.Estado != "Pendiente")
            return BadRequest("Solo se puede cambiar el estado de un pedido pendiente.");

        var actualizados = await db.Pedidos
            .Where(p => p.IdPedido == id && p.Estado == "Pendiente")
            .ExecuteUpdateAsync(cambio => cambio.SetProperty(p => p.Estado, datos.Estado));
        if (actualizados == 0)
        {
            await db.Entry(pedido).ReloadAsync();
            if (pedido.Estado != datos.Estado)
                return BadRequest("El estado del pedido ya cambió.");
        }

        pedido.Estado = datos.Estado;
        return Ok(pedido);
    }
}

public record NuevoPedido(
    [Range(1, int.MaxValue)] int IdCliente,
    [Required, MinLength(1)] List<NuevoDetalle> Detalles);

public record NuevoDetalle(
    [Range(1, int.MaxValue)] int IdProducto,
    [Range(1, int.MaxValue)] int Cantidad);

public record EstadoPedido([Required] string Estado);
public record ProductoCatalogo(decimal Precio, bool Estado);
public record StockBodega(int Disponible);
