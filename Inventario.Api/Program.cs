using Inventario.Api;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<InventarioDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("InventarioDB")));
builder.Services.AddHttpClient("Catalogo", cliente =>
{
    cliente.BaseAddress = new Uri(builder.Configuration["Servicios:Catalogo"]!);
    cliente.Timeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(opciones =>
    {
        opciones.SwaggerEndpoint("/openapi/v1.json", "Inventario API");
        opciones.RoutePrefix = "";
        opciones.DocumentTitle = "Inventario API";
    });
}

app.MapControllers();
app.Run();
 