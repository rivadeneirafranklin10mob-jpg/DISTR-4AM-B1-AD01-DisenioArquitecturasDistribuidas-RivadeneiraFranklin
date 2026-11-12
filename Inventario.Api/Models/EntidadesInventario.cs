using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Inventario.Api.Models;

[Table("Bodega")]
public class Bodega
{
    [Key]
    public int IdBodega { get; set; }

    [Required(ErrorMessage = "El nombre de la bodega es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ciudad es obligatoria.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "La ciudad debe tener entre 2 y 100 caracteres.")]
    public string Ciudad { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "La dirección debe tener entre 5 y 200 caracteres.")]
    public string Direccion { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Existencia> Existencias { get; set; } = new List<Existencia>();

    [JsonIgnore]
    public ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();

    [JsonIgnore]
    public ICollection<ReservaStock> Reservas { get; set; } = new List<ReservaStock>();
}

[Table("Existencia")]
public class Existencia
{
    [Key]
    public int IdExistencia { get; set; }

    /// <summary>
    /// Llave Foránea LÓGICA hacia Catalogo.Api (Producto.IdProducto).
    /// Se valida mediante petición HTTP a Catalogo.Api.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "IdProducto (FK lógica) debe ser mayor a 0.")]
    public int IdProducto { get; set; }

    /// <summary>
    /// Llave Foránea FÍSICA hacia Bodega.IdBodega en InventarioDB.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "IdBodega (FK física) debe ser mayor a 0.")]
    public int IdBodega { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    public int Stock { get; set; }

    [ForeignKey(nameof(IdBodega))]
    public Bodega? Bodega { get; set; }
}

[Table("MovimientoInventario")]
public class MovimientoInventario
{
    [Key]
    public int IdMovimiento { get; set; }

    /// <summary>
    /// Llave Foránea LÓGICA hacia Catalogo.Api (Producto.IdProducto).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "IdProducto (FK lógica) debe ser mayor a 0.")]
    public int IdProducto { get; set; }

    /// <summary>
    /// Llave Foránea FÍSICA hacia Bodega.IdBodega en InventarioDB.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "IdBodega (FK física) debe ser mayor a 0.")]
    public int IdBodega { get; set; }

    [Required(ErrorMessage = "El tipo de movimiento es obligatorio (ENTRADA, SALIDA, AJUSTE).")]
    [RegularExpression("^(ENTRADA|SALIDA|AJUSTE)$", ErrorMessage = "El tipo debe ser 'ENTRADA', 'SALIDA' o 'AJUSTE'.")]
    [StringLength(30)]
    public string Tipo { get; set; } = "ENTRADA";

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad del movimiento debe ser mayor a 0.")]
    public int Cantidad { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(IdBodega))]
    public Bodega? Bodega { get; set; }
}

[Table("ReservaStock")]
public class ReservaStock
{
    [Key]
    public int IdReserva { get; set; }

    /// <summary>
    /// Llave Foránea LÓGICA hacia Catalogo.Api (Producto.IdProducto).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "IdProducto (FK lógica) debe ser mayor a 0.")]
    public int IdProducto { get; set; }

    /// <summary>
    /// Llave Foránea FÍSICA hacia Bodega.IdBodega en InventarioDB.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "IdBodega (FK física) debe ser mayor a 0.")]
    public int IdBodega { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad a reservar debe ser mayor a 0.")]
    public int Cantidad { get; set; }

    [Required]
    [RegularExpression("^(Reservado|Confirmado|Cancelado)$", ErrorMessage = "El estado debe ser 'Reservado', 'Confirmado' o 'Cancelado'.")]
    [StringLength(30)]
    public string Estado { get; set; } = "Reservado";

    [ForeignKey(nameof(IdBodega))]
    public Bodega? Bodega { get; set; }
}
