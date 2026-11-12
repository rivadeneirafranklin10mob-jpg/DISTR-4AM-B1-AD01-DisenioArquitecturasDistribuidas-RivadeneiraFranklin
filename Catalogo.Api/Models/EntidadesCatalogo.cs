using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Catalogo.Api.Models;

[Table("Categoria")]
public class Categoria
{
    [Key]
    public int IdCategoria { get; set; }

    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La descripción no puede superar los 250 caracteres.")]
    public string? Descripcion { get; set; }

    [JsonIgnore]
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}

[Table("Marca")]
public class Marca
{
    [Key]
    public int IdMarca { get; set; }

    [Required(ErrorMessage = "El nombre de la marca es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El país de origen es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El país de origen debe tener entre 2 y 100 caracteres.")]
    public string PaisOrigen { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}

[Table("Producto")]
public class Producto
{
    [Key]
    public int IdProducto { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "IdCategoria debe ser un identificador válido mayor a 0.")]
    public int IdCategoria { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "IdMarca debe ser un identificador válido mayor a 0.")]
    public int IdMarca { get; set; }

    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "El nombre del producto debe tener entre 2 y 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código SKU es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El SKU debe tener entre 3 y 50 caracteres.")]
    public string SKU { get; set; } = string.Empty;

    [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor a 0.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "El estado del producto es obligatorio (Activo / Inactivo).")]
    [RegularExpression("^(Activo|Inactivo)$", ErrorMessage = "El estado debe ser 'Activo' o 'Inactivo'.")]
    [StringLength(30)]
    public string Estado { get; set; } = "Activo";

    [ForeignKey(nameof(IdCategoria))]
    public Categoria? Categoria { get; set; }

    [ForeignKey(nameof(IdMarca))]
    public Marca? Marca { get; set; }
}
