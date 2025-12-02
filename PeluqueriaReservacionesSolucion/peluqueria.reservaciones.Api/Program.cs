// Usings necesarios para la configuración de EF Core
using Microsoft.EntityFrameworkCore;

// Usings para las Interfaces (Puertos de Salida)
using peluqueria.reservaciones.Core.Puertos.Salida;
using peluqueria.reservaciones.Aplicacion.Puertos.Entrada;

// Repositorios e Infraestructura
using peluqueria.reservaciones.Infraestructura.Persistencia;
using peluqueria.reservaciones.Infraestructura.Repositorios;
using peluqueria.reservaciones.Core.Dominio;

// Consumidores de Mensajes
using peluqueria.reservaciones.Infraestructura.Mensajes;
using peluqueria.reservaciones.Infraestructura.Eventos;

// Servicios de Aplicación
using peluqueria.reservaciones.Aplicacion.Plantilla;
using peluqueria.reservaciones.Api.Controladores;
using peluqueria.reservaciones.Aplicacion.Fachada;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMQSettings"));

builder.Services.AddHostedService<CategoriaConsumer>();
builder.Services.AddHostedService<ServicioConsumer>();
builder.Services.AddHostedService<EstilistaConsumer>();
builder.Services.AddHostedService<AgendaConsumer>();
builder.Services.AddHostedService<ClienteConsumer>();


// Registro del DbContext para SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ReservacionesDbContext>(options =>
    options.UseSqlServer(connectionString));


// Repositorios de Datos Maestros
builder.Services.AddScoped<ICategoriaRepositorio, CategoriaRepositorio>();
builder.Services.AddScoped<IServicioRepositorio, ServicioRepositorio>();
builder.Services.AddScoped<IEstilistaRepositorio, EstilistaRepositorio>();
builder.Services.AddScoped<IClienteRepositorio, ClienteRepositorio>();
builder.Services.AddScoped<IHorarioRepositorio, HorarioRepositorio>();
builder.Services.AddScoped<IReservacionRepositorio, ReservacionRepositorio>();

builder.Services.AddScoped<ReservacionEstandar>();

builder.Services.AddScoped<ReservacionReprogramar>();

builder.Services.AddScoped<IReservacionManejador, ReservacionManejador>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });



var app = builder.Build();

app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");


app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
