using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Inventario.Api;

public class InventarioDbContext : DbContext
{
    public InventarioDbContext(DbContextOptions<InventarioDbContext> options) : base(options)
    {
    }

    public DbSet<Bodega> Bodegas => Set<Bodega>();
    public DbSet<Existencia> Existencias => Set<Existencia>();
    public DbSet<MovimientoInventario> Movimientos => Set<MovimientoInventario>();
    public DbSet<ReservaStock> Reservas => Set<ReservaStock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Existencia>()
            .HasOne<Bodega>().WithMany().HasForeignKey(e => e.IdBodega)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<MovimientoInventario>()
            .HasOne<Bodega>().WithMany().HasForeignKey(m => m.IdBodega)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ReservaStock>()
            .HasOne<Bodega>().WithMany().HasForeignKey(r => r.IdBodega)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
