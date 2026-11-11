using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventario.Api;

[Table("Bodega")]
public class Bodega
{
    [Key]
    public int IdBodega { get; set; }
    public string Nombre { get; set; } = "";
    public string Ciudad { get; set; } = "";
    public string Direccion { get; set; } = "";
}

[Table("Existencia")]
public class Existencia
{
    [Key]
    public int IdExistencia { get; set; }
    public int IdProducto { get; set; }
    public int IdBodega { get; set; }
    public int Stock { get; set; }
}

[Table("MovimientoInventario")]
public class MovimientoInventario
{
    [Key]
    public int IdMovimiento { get; set; }
    public int IdProducto { get; set; }
    public int IdBodega { get; set; }
    public string Tipo { get; set; } = "";
    public int Cantidad { get; set; }
    public DateTime Fecha { get; set; }
}

[Table("ReservaStock")]
public class ReservaStock
{
    [Key]
    public int IdReserva { get; set; }
    public int IdProducto { get; set; }
    public int IdBodega { get; set; }
    public int Cantidad { get; set; }
    public string Estado { get; set; } = "";
}

public record ExistenciaDisponible(int IdExistencia, int IdProducto, int IdBodega, int Stock, int Disponible);

public class NuevaBodega
{
    [Required, MaxLength(100)]
    public string Nombre { get; set; } = "";

    [Required, MaxLength(80)]
    public string Ciudad { get; set; } = "";

    [Required, MaxLength(150)]
    public string Direccion { get; set; } = "";
}

public class NuevoMovimiento
{
    [Range(1, int.MaxValue)]
    public int IdProducto { get; set; }

    [Range(1, int.MaxValue)]
    public int IdBodega { get; set; }

    [Required]
    public string Tipo { get; set; } = "";

    [Range(1, int.MaxValue)]
    public int Cantidad { get; set; }
}

public class NuevaReserva
{
    [Range(1, int.MaxValue)]
    public int IdProducto { get; set; }

    [Range(1, int.MaxValue)]
    public int IdBodega { get; set; }

    [Range(1, int.MaxValue)]
    public int Cantidad { get; set; }
}

public class EstadoReserva
{
    [Required]
    public string Estado { get; set; } = "";
}
