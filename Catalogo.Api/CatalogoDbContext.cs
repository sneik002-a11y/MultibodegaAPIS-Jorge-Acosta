using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Catalogo.Api;

public class CatalogoDbContext(DbContextOptions<CatalogoDbContext> options) : DbContext(options)
{
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<Producto> Productos => Set<Producto>();

    protected override void OnModelCreating(ModelBuilder modelo)
    {
        modelo.Entity<Producto>().HasOne<Categoria>().WithMany()
            .HasForeignKey(producto => producto.IdCategoria).OnDelete(DeleteBehavior.Restrict);
        modelo.Entity<Producto>().HasOne<Marca>().WithMany()
            .HasForeignKey(producto => producto.IdMarca).OnDelete(DeleteBehavior.Restrict);
        modelo.Entity<Producto>().HasIndex(producto => producto.SKU).IsUnique();
    }
}
