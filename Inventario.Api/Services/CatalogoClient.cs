using System.Net;
using System.Net.Http.Json;

namespace Inventario.Api.Services;

public class ProductoCatalogoDto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class CatalogoClient
{
    private readonly HttpClient _httpClient;

    public CatalogoClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Valida la referencia de llave foránea LÓGICA de IdProducto contra Catalogo.Api.
    /// </summary>
    public async Task<(bool Existe, string Mensaje, ProductoCatalogoDto? Producto)> ValidarProductoAsync(int idProducto)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/productos/{idProducto}");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return (false, $"Error de FK lógica: El producto con Id {idProducto} no existe en Catalogo.Api (CatalogoDB).", null);
            }

            response.EnsureSuccessStatusCode();
            var producto = await response.Content.ReadFromJsonAsync<ProductoCatalogoDto>();
            if (producto != null && !string.Equals(producto.Estado, "Activo", StringComparison.OrdinalIgnoreCase))
            {
                return (false, $"El producto con Id {idProducto} ('{producto.Nombre}') está en estado '{producto.Estado}'.", producto);
            }

            return (true, "Producto válido.", producto);
        }
        catch (HttpRequestException ex)
        {
            return (false, $"No se pudo verificar la FK lógica en Catalogo.Api: {ex.Message}", null);
        }
    }
}
