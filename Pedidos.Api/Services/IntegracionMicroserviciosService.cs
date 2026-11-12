using System.Net;
using System.Net.Http.Json;

namespace Pedidos.Api.Services;

public class ProductoRemotoDto
{
    public int IdProducto { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class StockTotalRemotoDto
{
    public int IdProducto { get; set; }
    public int StockTotal { get; set; }
    public int BodegasConStock { get; set; }
}

public class IntegracionMicroserviciosService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IntegracionMicroserviciosService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Valida la llave foránea LÓGICA de IdProducto en Catalogo.Api y consulta el stock disponible en Inventario.Api.
    /// </summary>
    public async Task<(bool Valido, string Mensaje, ProductoRemotoDto? Producto)> ValidarProductoYStockAsync(int idProducto, int cantidadSolicitada)
    {
        try
        {
            var catalogoClient = _httpClientFactory.CreateClient("CatalogoApi");
            var prodResponse = await catalogoClient.GetAsync($"/api/productos/{idProducto}");
            if (prodResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return (false, $"Error de FK lógica: El producto con Id {idProducto} no existe en Catalogo.Api (CatalogoDB).", null);
            }

            prodResponse.EnsureSuccessStatusCode();
            var producto = await prodResponse.Content.ReadFromJsonAsync<ProductoRemotoDto>();
            if (producto == null || !string.Equals(producto.Estado, "Activo", StringComparison.OrdinalIgnoreCase))
            {
                return (false, $"El producto con Id {idProducto} no está activo en el catálogo.", producto);
            }

            var inventarioClient = _httpClientFactory.CreateClient("InventarioApi");
            var stockResponse = await inventarioClient.GetAsync($"/api/existencias/producto/{idProducto}/stock-total");
            if (stockResponse.IsSuccessStatusCode)
            {
                var stockInfo = await stockResponse.Content.ReadFromJsonAsync<StockTotalRemotoDto>();
                if (stockInfo != null && stockInfo.StockTotal < cantidadSolicitada)
                {
                    return (false, $"Stock insuficiente en Inventario.Api para el producto '{producto.Nombre}' (Id {idProducto}). Disponible total: {stockInfo.StockTotal}, solicitado: {cantidadSolicitada}.", producto);
                }
            }

            return (true, "Validación de FK lógica y stock exitosa.", producto);
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Error de comunicación al validar FK lógica con Catalogo.Api/Inventario.Api: {ex.Message}", null);
        }
    }
}
