using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Models;

namespace Pedidos.Api.Data;

public class PedidosDbContext : DbContext
{
    public PedidosDbContext(DbContextOptions<PedidosDbContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<DetallePedido> DetallesPedido => Set<DetallePedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasIndex(c => c.Cedula).IsUnique();
        });

        // Regla de FK FÍSICA: Pedido -> Cliente
        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasOne(p => p.Cliente)
                .WithMany(c => c.Pedidos)
                .HasForeignKey(p => p.IdCliente)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Pedido_Cliente");
        });

        // Regla de FK FÍSICA: DetallePedido -> Pedido
        // DetallePedido.IdProducto es FK LÓGICA hacia Catalogo.Api
        modelBuilder.Entity<DetallePedido>(entity =>
        {
            entity.HasOne(d => d.Pedido)
                .WithMany(p => p.Detalles)
                .HasForeignKey(d => d.IdPedido)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_DetallePedido_Pedido");
        });

        // Datos semilla
        modelBuilder.Entity<Cliente>().HasData(
            new Cliente { IdCliente = 1, Nombres = "Franklin Rivadeneira", Cedula = "1723456789", Correo = "franklin.rivadeneira@cordillera.edu.ec" },
            new Cliente { IdCliente = 2, Nombres = "María Fernanda Gómez", Cedula = "0912345678", Correo = "maria.gomez@empresa.com.ec" }
        );

        modelBuilder.Entity<Pedido>().HasData(
            new Pedido { IdPedido = 1, IdCliente = 1, Fecha = new DateTime(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc), Total = 2500.00m, Estado = "Confirmado" }
        );

        modelBuilder.Entity<DetallePedido>().HasData(
            new DetallePedido { IdDetalle = 1, IdPedido = 1, IdProducto = 1, Cantidad = 2, PrecioUnitario = 1250.00m }
        );
    }
}
