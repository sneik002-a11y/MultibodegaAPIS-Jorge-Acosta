using Microsoft.EntityFrameworkCore;
using Pedidos.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<PedidosDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("PedidosDB")));
builder.Services.AddHttpClient("Catalogo", cliente =>
{
    cliente.BaseAddress = new Uri(builder.Configuration["Servicios:Catalogo"]!);
    cliente.Timeout = TimeSpan.FromSeconds(5);
});
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
        opciones.SwaggerEndpoint("/openapi/v1.json", "Pedidos API");
        opciones.RoutePrefix = "";
        opciones.DocumentTitle = "Pedidos API";
    });
}

app.MapControllers();
app.Run();
 