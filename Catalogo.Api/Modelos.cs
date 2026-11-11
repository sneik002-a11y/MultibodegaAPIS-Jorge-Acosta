using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Catalogo.Api;

[Table("Categoria")]
public class Categoria
{
    [Key]
    public int IdCategoria { get; set; }

    [Required, StringLength(100)]
    public string Nombre { get; set; } = "";

    [StringLength(250)]
    public string? Descripcion { get; set; }
}

[Table("Marca")]
public class Marca
{
    [Key]
    public int IdMarca { get; set; }

    [Required, StringLength(100)]
    public string Nombre { get; set; } = "";

    [Required, StringLength(100)]
    public string PaisOrigen { get; set; } = "";
}

[Table("Producto")]
public class Producto
{
    [Key]
    public int IdProducto { get; set; }

    [Range(1, int.MaxValue)]
    public int IdCategoria { get; set; }

    [Range(1, int.MaxValue)]
    public int IdMarca { get; set; }

    [Required, StringLength(150)]
    public string Nombre { get; set; } = "";

    [Required, StringLength(50)]
    public string SKU { get; set; } = "";

    [Range(typeof(decimal), "0.01", "99999999.99", ParseLimitsInInvariantCulture = true)]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Precio { get; set; }

    public bool Estado { get; set; } = true;
}
