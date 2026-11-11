using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Emit;

namespace Pedidos.Api;

public class PedidosDb(DbContextOptions<PedidosDb> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<DetallePedido> Detalles => Set<DetallePedido>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Cliente>().HasIndex(c => c.Cedula).IsUnique();
        model.Entity<Pedido>().HasOne<Cliente>().WithMany()
            .HasForeignKey(p => p.IdCliente).OnDelete(DeleteBehavior.NoAction);
        model.Entity<DetallePedido>().HasIndex(d => new { d.IdPedido, d.IdProducto }).IsUnique();
        model.Entity<Pedido>().HasMany(p => p.Detalles).WithOne().HasForeignKey(d => d.IdPedido);
    }
}

[Table("Cliente")]
public class Cliente
{
    [Key]
    public int IdCliente { get; set; }

    [Required, StringLength(120)]
    public string Nombres { get; set; } = "";

    [Required, StringLength(20)]
    public string Cedula { get; set; } = "";

    [Required, EmailAddress, StringLength(150)]
    public string Correo { get; set; } = "";
}

[Table("Pedido")]
public class Pedido
{
    [Key]
    public int IdPedido { get; set; }
    public int IdCliente { get; set; }
    public DateTime Fecha { get; set; }

    [Precision(18, 2)]
    public decimal Total { get; set; }

    [StringLength(20)]
    public string Estado { get; set; } = "Pendiente";

    public List<DetallePedido> Detalles { get; set; } = [];
}

[Table("DetallePedido")]
public class DetallePedido
{
    [Key]
    public int IdDetalle { get; set; }
    public int IdPedido { get; set; }
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }

    [Precision(18, 2)]
    public decimal PrecioUnitario { get; set; }
}
