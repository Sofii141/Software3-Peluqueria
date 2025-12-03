using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Peluqueria.Application.Interfaces;
using Peluqueria.Application.Services;
using Peluqueria.Infrastructure.Identity;
using Peluqueria.Infrastructure.Data;
using Peluqueria.Infrastructure.Repositories;
using Peluqueria.Infrastructure.Service;
using System.Text;
using System.Globalization;
using Peluqueria.Infrastructure.Repository;
using Peluqueria.API.Middleware;
using Microsoft.AspNetCore.Mvc;
using Peluqueria.API.Errors;
using Peluqueria.Application.Exceptions;
using Peluqueria.Infrastructure.HttpClients;

// Configuración de la cultura por defecto para asegurar consistencia en formatos de fecha y moneda (en-US).
var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var builder = WebApplication.CreateBuilder(args);

// Configuración de controladores y serialización JSON.
// Se utiliza NewtonsoftJson para manejar referencias circulares que pueden ocurrir con Entity Framework.
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    })
    .AddMvcOptions(options =>
    {
        // Personalización de mensajes de error de validación de modelos.
        options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
            _ => "El valor es requerido.");
    });

// Personalización de la respuesta para errores de validación (HTTP 400).
// Intercepta el comportamiento por defecto para devolver un formato estandarizado (ErrorResponse).
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errores = context.ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .SelectMany(x => x.Value.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();

            var mensajeUnido = string.Join("; ", errores);

            var errorResponse = ErrorUtils.CrearError(
                CodigoError.CAMPOS_OBLIGATORIOS.Codigo,
                mensajeUnido,
                400,
                context.HttpContext.Request.Path,
                context.HttpContext.Request.Method
            );

            return new BadRequestObjectResult(errorResponse);
        };
    });

builder.Services.AddEndpointsApiExplorer();

// Configuración de Swagger/OpenAPI con soporte para autenticación JWT (Bearer Token).
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});

// --- CAPA DE INFRAESTRUCTURA: PERSISTENCIA E IDENTIDAD ---

// Configuración del contexto de base de datos (SQL Server).
builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Configuración de ASP.NET Core Identity (Usuarios y Roles).
// Se definen las políticas de complejidad de contraseñas y unicidad de emails.
builder.Services.AddIdentity<AppUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredUniqueChars = 1;
})
.AddEntityFrameworkStores<ApplicationDBContext>()
.AddDefaultTokenProviders();

// --- CAPA DE INFRAESTRUCTURA: AUTENTICACIÓN Y JWT ---

// Configuración del esquema de autenticación JWT Bearer.
// Valida el emisor, la audiencia y la firma del token usando la clave secreta configurada.
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"]!)
        )
    };
});


// --- INYECCIÓN DE DEPENDENCIAS (IoC) ---

// 1. Repositorios (Acceso a Datos)
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IServicioRepository, ServicioRepository>();
builder.Services.AddScoped<IEstilistaRepository, EstilistaRepository>();
builder.Services.AddScoped<IEstilistaAgendaRepository, EstilistaAgendaRepository>();

// 2. Servicios de Infraestructura (Externos/Cross-Cutting)
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMQMessagePublisher>(); // Singleton para conexión persistente a RabbitMQ

// 3. Servicios de Aplicación (Lógica de Negocio)
builder.Services.AddScoped<IEstilistaService, EstilistaService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IEstilistaAgendaService, EstilistaAgendaService>();
builder.Services.AddScoped<IDataSyncService, DataSyncService>();

// 4. Clientes HTTP (Comunicación entre microservicios)
// Configuración del cliente tipado para consultar el Microservicio de Reservaciones.
builder.Services.AddHttpClient<IReservacionClient, ReservacionClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5288/");
    // Establece un tiempo de espera para evitar bloqueos en la comunicación síncrona.
    client.Timeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();

// =================================================================
// INICIALIZACIÓN DE DATOS Y SINCRONIZACIÓN
// =================================================================
// Crea un alcance (scope) temporal para resolver servicios 'Scoped' durante el inicio de la aplicación.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // 1. Migraciones de Base de Datos
        // Aplica las migraciones pendientes o crea la base de datos si no existe.
        // También ejecuta el Seeding de datos definido en OnModelCreating.
        var context = services.GetRequiredService<ApplicationDBContext>();
        context.Database.Migrate();

        logger.LogInformation("Base de datos del Monolito actualizada/creada correctamente.");

        // 2. Sincronización de Datos (Event Bus)
        // Publica el estado actual de las entidades maestras a RabbitMQ para asegurar
        // que el microservicio consumidor tenga la data actualizada al arrancar.
        var syncService = services.GetRequiredService<IDataSyncService>();

        logger.LogInformation("Iniciando sincronización de datos con el Microservicio...");

        await syncService.SincronizarTodoAsync();

        logger.LogInformation("¡Sincronización completada! Mensajes enviados a RabbitMQ.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Ocurrió un error durante la migración o sincronización de datos.");
    }
}

// Configuración del Pipeline de solicitudes HTTP.

app.UseStaticFiles(); // Habilita el servicio de archivos estáticos (imágenes).

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(config =>
    {
        config.ConfigObject.PersistAuthorization = true;
    });
}

// Middleware global de manejo de excepciones.
app.UseMiddleware<Peluqueria.API.Middleware.ExceptionMiddleware>();

// Configuración de CORS para permitir peticiones desde el cliente Angular.
app.UseCors(corsBuilder => corsBuilder
    .WithOrigins("http://localhost:4200", "https://localhost:4200")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

app.UseAuthentication(); // Habilita la autenticación.
app.UseAuthorization();  // Habilita la autorización.

app.MapControllers(); // Mapea los controladores de la API.

app.Run();