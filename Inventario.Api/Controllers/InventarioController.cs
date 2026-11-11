using System.Data;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api;

[ApiController]
[Route("api")]
public class InventarioController : ControllerBase
{
    private readonly InventarioDbContext db;
    private readonly IHttpClientFactory clientesHttp;

    public InventarioController(InventarioDbContext db, IHttpClientFactory clientesHttp)
    {
        this.db = db;
        this.clientesHttp = clientesHttp;
    }

    [HttpGet("bodegas")]
    public async Task<IActionResult> ListarBodegas()
    {
        return Ok(await db.Bodegas.ToListAsync());
    }

    [HttpGet("bodegas/{id:int}")]
    public async Task<IActionResult> ObtenerBodega(int id)
    {
        var bodega = await db.Bodegas.FindAsync(id);
        if (bodega == null)
            return NotFound("La bodega no existe.");

        return Ok(bodega);
    }

    [HttpPost("bodegas")]
    public async Task<IActionResult> CrearBodega(NuevaBodega datos)
    {
        var bodega = new Bodega
        {
            Nombre = datos.Nombre.Trim(),
            Ciudad = datos.Ciudad.Trim(),
            Direccion = datos.Direccion.Trim()
        };
        db.Bodegas.Add(bodega);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerBodega), new { id = bodega.IdBodega }, bodega);
    }

    [HttpGet("existencias")]
    public async Task<IActionResult> ListarExistencias()
    {
        return Ok(await ConsultarExistencias(db.Existencias).ToListAsync());
    }

    [HttpGet("existencias/{id:int}")]
    public async Task<IActionResult> ObtenerExistencia(int id)
    {
        var existencia = await ConsultarExistencias(db.Existencias.Where(e => e.IdExistencia == id))
            .FirstOrDefaultAsync();
        if (existencia == null)
            return NotFound("La existencia no está registrada.");

        return Ok(existencia);
    }

    [HttpGet("existencias/producto/{id:int}")]
    public async Task<IActionResult> ExistenciasPorProducto(int id)
    {
        return Ok(await ConsultarExistencias(db.Existencias.Where(e => e.IdProducto == id)).ToListAsync());
    }

    [HttpGet("movimientos")]
    public async Task<IActionResult> ListarMovimientos()
    {
        return Ok(await db.Movimientos.ToListAsync());
    }

    [HttpGet("movimientos/{id:int}")]
    public async Task<IActionResult> ObtenerMovimiento(int id)
    {
        var movimiento = await db.Movimientos.FindAsync(id);
        if (movimiento == null)
            return NotFound("El movimiento no existe.");

        return Ok(movimiento);
    }

    [HttpPost("movimientos")]
    public async Task<IActionResult> RegistrarMovimiento(NuevoMovimiento datos)
    {
        if (datos.Tipo != "Entrada" && datos.Tipo != "Salida")
            return BadRequest("El tipo debe ser Entrada o Salida.");

        var error = await ValidarProductoYBodega(datos.IdProducto, datos.IdBodega);
        if (error != null)
            return error;

        // La consulta y la actualización del stock se hacen en la misma transacción.
        await using var transaccion = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var existencia = await db.Existencias.SingleOrDefaultAsync(e =>
            e.IdProducto == datos.IdProducto && e.IdBodega == datos.IdBodega);

        if (datos.Tipo == "Entrada")
        {
            if (existencia == null)
            {
                existencia = new Existencia { IdProducto = datos.IdProducto, IdBodega = datos.IdBodega };
                db.Existencias.Add(existencia);
            }

            if (datos.Cantidad > int.MaxValue - existencia.Stock)
                return BadRequest("La cantidad supera el límite de stock.");

            existencia.Stock += datos.Cantidad;
        }
        else
        {
            var reservado = await CantidadReservada(datos.IdProducto, datos.IdBodega);
            if (existencia == null || existencia.Stock - reservado < datos.Cantidad)
                return BadRequest("El stock disponible no alcanza para la salida.");

            existencia.Stock -= datos.Cantidad;
        }

        var movimiento = new MovimientoInventario
        {
            IdProducto = datos.IdProducto,
            IdBodega = datos.IdBodega,
            Tipo = datos.Tipo,
            Cantidad = datos.Cantidad,
            Fecha = DateTime.UtcNow
        };
        db.Movimientos.Add(movimiento);
        await db.SaveChangesAsync();
        await transaccion.CommitAsync();

        return CreatedAtAction(nameof(ObtenerMovimiento), new { id = movimiento.IdMovimiento }, movimiento);
    }

    [HttpGet("reservas")]
    public async Task<IActionResult> ListarReservas()
    {
        return Ok(await db.Reservas.ToListAsync());
    }

    [HttpGet("reservas/{id:int}")]
    public async Task<IActionResult> ObtenerReserva(int id)
    {
        var reserva = await db.Reservas.FindAsync(id);
        if (reserva == null)
            return NotFound("La reserva no existe.");

        return Ok(reserva);
    }

    [HttpPost("reservas")]
    public async Task<IActionResult> CrearReserva(NuevaReserva datos)
    {
        var error = await ValidarProductoYBodega(datos.IdProducto, datos.IdBodega);
        if (error != null)
            return error;

        await using var transaccion = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var existencia = await db.Existencias.SingleOrDefaultAsync(e =>
            e.IdProducto == datos.IdProducto && e.IdBodega == datos.IdBodega);
        var reservado = await CantidadReservada(datos.IdProducto, datos.IdBodega);

        if (existencia == null || existencia.Stock - reservado < datos.Cantidad)
            return BadRequest("El stock disponible no alcanza para la reserva.");

        var reserva = new ReservaStock
        {
            IdProducto = datos.IdProducto,
            IdBodega = datos.IdBodega,
            Cantidad = datos.Cantidad,
            Estado = "Activa"
        };
        db.Reservas.Add(reserva);
        await db.SaveChangesAsync();
        await transaccion.CommitAsync();

        return CreatedAtAction(nameof(ObtenerReserva), new { id = reserva.IdReserva }, reserva);
    }

    [HttpPut("reservas/{id:int}/estado")]
    public async Task<IActionResult> CambiarEstadoReserva(int id, EstadoReserva datos)
    {
        if (datos.Estado != "Cancelada" && datos.Estado != "Confirmada")
            return BadRequest("El estado debe ser Cancelada o Confirmada.");

        await using var transaccion = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var reserva = await db.Reservas.FindAsync(id);
        if (reserva == null)
            return NotFound("La reserva no existe.");

        if (reserva.Estado == datos.Estado)
            return Ok(reserva);

        if (reserva.Estado != "Activa")
            return BadRequest("Solo se puede cambiar el estado de una reserva activa.");

        if (datos.Estado == "Confirmada")
        {
            var existencia = await db.Existencias.SingleOrDefaultAsync(e =>
                e.IdProducto == reserva.IdProducto && e.IdBodega == reserva.IdBodega);
            if (existencia == null || existencia.Stock < reserva.Cantidad)
                return BadRequest("El stock no alcanza para confirmar la reserva.");

            existencia.Stock -= reserva.Cantidad;
            db.Movimientos.Add(new MovimientoInventario
            {
                IdProducto = reserva.IdProducto,
                IdBodega = reserva.IdBodega,
                Tipo = "Salida",
                Cantidad = reserva.Cantidad,
                Fecha = DateTime.UtcNow
            });
        }

        reserva.Estado = datos.Estado;
        await db.SaveChangesAsync();
        await transaccion.CommitAsync();

        return Ok(reserva);
    }

    private IQueryable<ExistenciaDisponible> ConsultarExistencias(IQueryable<Existencia> existencias)
    {
        return existencias.Select(e => new ExistenciaDisponible(
            e.IdExistencia, e.IdProducto, e.IdBodega, e.Stock,
            e.Stock - (db.Reservas
                .Where(r => r.IdProducto == e.IdProducto && r.IdBodega == e.IdBodega && r.Estado == "Activa")
                .Sum(r => (int?)r.Cantidad) ?? 0)));
    }

    private async Task<int> CantidadReservada(int idProducto, int idBodega)
    {
        return await db.Reservas
            .Where(r => r.IdProducto == idProducto && r.IdBodega == idBodega && r.Estado == "Activa")
            .SumAsync(r => r.Cantidad);
    }

    private async Task<IActionResult?> ValidarProductoYBodega(int idProducto, int idBodega)
    {
        if (!await db.Bodegas.AnyAsync(b => b.IdBodega == idBodega))
            return NotFound("La bodega no existe.");

        var cliente = clientesHttp.CreateClient("Catalogo");
        try
        {
            using var respuesta = await cliente.GetAsync($"api/productos/{idProducto}", HttpContext.RequestAborted);
            if (respuesta.StatusCode == HttpStatusCode.NotFound)
                return NotFound("El producto no existe en el catálogo.");

            if (respuesta.IsSuccessStatusCode)
                return null;
        }
        catch (HttpRequestException)
        {
            return StatusCode(503, "No se pudo consultar el catálogo.");
        }
        catch (OperationCanceledException) when (!HttpContext.RequestAborted.IsCancellationRequested)
        {
            return StatusCode(503, "El catálogo tardó demasiado en responder.");
        }

        return StatusCode(503, "No se pudo consultar el catálogo.");
    }
}

