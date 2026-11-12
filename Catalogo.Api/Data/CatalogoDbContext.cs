using Catalogo.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Data;

public class CatalogoDbContext : DbContext
{
    public CatalogoDbContext(DbContextOptions<CatalogoDbContext> options) : base(options)
    {
    }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<Producto> Productos => Set<Producto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Reglas de llaves FK FÍSICAS dentro de CatalogoDB
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasIndex(p => p.SKU).IsUnique();

            entity.HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Producto_Categoria");

            entity.HasOne(p => p.Marca)
                .WithMany(m => m.Productos)
                .HasForeignKey(p => p.IdMarca)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Producto_Marca");
        });

        // Datos semilla
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { IdCategoria = 1, Nombre = "Laptops y Portátiles", Descripcion = "Equipos portátiles empresariales y gaming" },
            new Categoria { IdCategoria = 2, Nombre = "Monitores y Pantallas", Descripcion = "Monitores LED, IPS y curvos de alta resolución" },
            new Categoria { IdCategoria = 3, Nombre = "Periféricos y Accesorios", Descripcion = "Teclados mecánicos, mouses e interfaces USB" }
        );

        modelBuilder.Entity<Marca>().HasData(
            new Marca { IdMarca = 1, Nombre = "Dell Technologies", PaisOrigen = "Estados Unidos" },
            new Marca { IdMarca = 2, Nombre = "Samsung Electronics", PaisOrigen = "Corea del Sur" },
            new Marca { IdMarca = 3, Nombre = "Logitech", PaisOrigen = "Suiza" }
        );

        modelBuilder.Entity<Producto>().HasData(
            new Producto { IdProducto = 1, IdCategoria = 1, IdMarca = 1, Nombre = "Laptop Dell Latitude 5540 Core i7 16GB 512GB SSD", SKU = "DELL-LAT-5540", Precio = 1250.00m, Estado = "Activo" },
            new Producto { IdProducto = 2, IdCategoria = 2, IdMarca = 2, Nombre = "Monitor Samsung Odyssey G5 27 Pulgadas QHD", SKU = "SAM-ODY-G527", Precio = 340.50m, Estado = "Activo" },
            new Producto { IdProducto = 3, IdCategoria = 3, IdMarca = 3, Nombre = "Mouse Inalámbrico Logitech MX Master 3S", SKU = "LOG-MXM-3S", Precio = 99.99m, Estado = "Activo" }
        );
    }
}
