using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Pedidos.Api.Models;

[Table("Cliente")]
public class Cliente
{
    [Key]
    public int IdCliente { get; set; }

    [Required(ErrorMessage = "Los nombres del cliente son obligatorios.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "Los nombres deben tener entre 3 y 150 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "La cédula es obligatoria.")]
    [RegularExpression(@"^\d{10,13}$", ErrorMessage = "La cédula o RUC debe contener entre 10 y 13 dígitos numéricos.")]
    [StringLength(20)]
    public string Cedula { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [StringLength(150)]
    public string Correo { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}

[Table("Pedido")]
public class Pedido
{
    [Key]
    public int IdPedido { get; set; }

    /// <summary>
    /// Llave Foránea FÍSICA hacia Cliente.IdCliente en PedidosDB.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "IdCliente (FK física) debe ser mayor a 0.")]
    public int IdCliente { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    [Range(0, 9999999.99, ErrorMessage = "El total no puede ser negativo.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    [Required]
    [RegularExpression("^(Pendiente|Confirmado|Cancelado|Entregado)$", ErrorMessage = "El estado debe ser 'Pendiente', 'Confirmado', 'Cancelado' o 'Entregado'.")]
    [StringLength(30)]
    public string Estado { get; set; } = "Confirmado";

    [ForeignKey(nameof(IdCliente))]
    public Cliente? Cliente { get; set; }

    public List<DetallePedido> Detalles { get; set; } = new();
}

[Table("DetallePedido")]
public class DetallePedido
{
    [Key]
    public int IdDetalle { get; set; }

    /// <summary>
    /// Llave Foránea FÍSICA hacia Pedido.IdPedido en PedidosDB.
    /// </summary>
    public int IdPedido { get; set; }

    /// <summary>
    /// Llave Foránea LÓGICA hacia Catalogo.Api (Producto.IdProducto).
    /// Se valida vía HTTP contra Catalogo.Api e Inventario.Api.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "IdProducto (FK lógica) debe ser mayor a 0.")]
    public int IdProducto { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; set; }

    [Range(0.01, 999999.99, ErrorMessage = "El precio unitario debe ser mayor a 0.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }

    [JsonIgnore]
    [ForeignKey(nameof(IdPedido))]
    public Pedido? Pedido { get; set; }
}
