using Inventario.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Data;

public class InventarioDbContext : DbContext
{
    public InventarioDbContext(DbContextOptions<InventarioDbContext> options) : base(options)
    {
    }

    public DbSet<Bodega> Bodegas => Set<Bodega>();
    public DbSet<Existencia> Existencias => Set<Existencia>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();
    public DbSet<ReservaStock> ReservasStock => Set<ReservaStock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Reglas de FKs FÍSICAS hacia Bodega (dentro de InventarioDB)
        // IdProducto es FK LÓGICA hacia Catalogo.Api y no tiene restricción física entre BDs
        modelBuilder.Entity<Existencia>(entity =>
        {
            entity.HasIndex(e => new { e.IdProducto, e.IdBodega }).IsUnique();

            entity.HasOne(e => e.Bodega)
                .WithMany(b => b.Existencias)
                .HasForeignKey(e => e.IdBodega)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Existencia_Bodega");
        });

        modelBuilder.Entity<MovimientoInventario>(entity =>
        {
            entity.HasOne(m => m.Bodega)
                .WithMany(b => b.Movimientos)
                .HasForeignKey(m => m.IdBodega)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_MovimientoInventario_Bodega");
        });

        modelBuilder.Entity<ReservaStock>(entity =>
        {
            entity.HasOne(r => r.Bodega)
                .WithMany(b => b.Reservas)
                .HasForeignKey(r => r.IdBodega)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_ReservaStock_Bodega");
        });

        // Datos semilla
        modelBuilder.Entity<Bodega>().HasData(
            new Bodega { IdBodega = 1, Nombre = "Bodega Central Norte", Ciudad = "Quito", Direccion = "Av. Galo Plaza Lasso N52-100 y Capitán Ramón Borja" },
            new Bodega { IdBodega = 2, Nombre = "Bodega Distribución Sur", Ciudad = "Guayaquil", Direccion = "Km 14.5 Vía Daule, Parque Industrial Pascuales" }
        );

        modelBuilder.Entity<Existencia>().HasData(
            new Existencia { IdExistencia = 1, IdProducto = 1, IdBodega = 1, Stock = 25 },
            new Existencia { IdExistencia = 2, IdProducto = 2, IdBodega = 1, Stock = 40 },
            new Existencia { IdExistencia = 3, IdProducto = 3, IdBodega = 1, Stock = 100 },
            new Existencia { IdExistencia = 4, IdProducto = 1, IdBodega = 2, Stock = 15 },
            new Existencia { IdExistencia = 5, IdProducto = 2, IdBodega = 2, Stock = 30 }
        );

        modelBuilder.Entity<MovimientoInventario>().HasData(
            new MovimientoInventario { IdMovimiento = 1, IdProducto = 1, IdBodega = 1, Tipo = "ENTRADA", Cantidad = 25, Fecha = new DateTime(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc) },
            new MovimientoInventario { IdMovimiento = 2, IdProducto = 2, IdBodega = 1, Tipo = "ENTRADA", Cantidad = 40, Fecha = new DateTime(2026, 11, 1, 10, 5, 0, DateTimeKind.Utc) },
            new MovimientoInventario { IdMovimiento = 3, IdProducto = 3, IdBodega = 1, Tipo = "ENTRADA", Cantidad = 100, Fecha = new DateTime(2026, 11, 1, 10, 10, 0, DateTimeKind.Utc) }
        );

        modelBuilder.Entity<ReservaStock>().HasData(
            new ReservaStock { IdReserva = 1, IdProducto = 1, IdBodega = 1, Cantidad = 2, Estado = "Confirmado" }
        );
    }
}
