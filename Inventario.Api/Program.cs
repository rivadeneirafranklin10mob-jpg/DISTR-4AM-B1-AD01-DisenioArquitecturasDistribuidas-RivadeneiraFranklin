using Inventario.Api.Data;
using Inventario.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("InventarioConnection");
builder.Services.AddDbContext<InventarioDbContext>(options =>
    options.UseSqlServer(connectionString));

var catalogoUrl = builder.Configuration["MicroserviciosUrls:CatalogoApi"] ?? "http://localhost:5101";
builder.Services.AddHttpClient<CatalogoClient>(client =>
{
    client.BaseAddress = new Uri(catalogoUrl);
});

var app = builder.Build();

// Inicializa automáticamente InventarioDB y sus datos semilla si no existe
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InventarioDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
