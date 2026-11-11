using Catalogo.Api;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<CatalogoDbContext>(opciones =>
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("CatalogoDB")));
builder.Services.AddHttpClient("Inventario", cliente =>
{
    cliente.BaseAddress = new Uri(builder.Configuration["Servicios:Inventario"]!);
    cliente.Timeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(opciones =>
    {
        opciones.SwaggerEndpoint("/openapi/v1.json", "Catalogo API");
        opciones.RoutePrefix = "";
        opciones.DocumentTitle = "Catalogo API";
    });
}

app.MapControllers(); 
app.Run();

