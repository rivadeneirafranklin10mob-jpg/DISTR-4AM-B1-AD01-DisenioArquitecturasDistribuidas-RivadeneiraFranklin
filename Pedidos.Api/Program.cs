using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("PedidosConnection");
builder.Services.AddDbContext<PedidosDbContext>(options =>
    options.UseSqlServer(connectionString));

var catalogoUrl = builder.Configuration["MicroserviciosUrls:CatalogoApi"] ?? "http://localhost:5101";
var inventarioUrl = builder.Configuration["MicroserviciosUrls:InventarioApi"] ?? "http://localhost:5102";

builder.Services.AddHttpClient("CatalogoApi", client =>
{
    client.BaseAddress = new Uri(catalogoUrl);
});

builder.Services.AddHttpClient("InventarioApi", client =>
{
    client.BaseAddress = new Uri(inventarioUrl);
});

builder.Services.AddScoped<IntegracionMicroserviciosService>();

var app = builder.Build();

// Inicializa automáticamente PedidosDB y sus datos semilla si no existe
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PedidosDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
