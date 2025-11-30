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

var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    })
    .AddMvcOptions(options =>
    {
        options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
            _ => "El valor es requerido.");
    });

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

// --- DATABASE AND IDENTITY CONFIGURATION ---
builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

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

// --- AUTHENTICATION CONFIGURATION ---
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


// --- INYECCION DE DEPENDENCIAS ---

// Repositorios
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IServicioRepository, ServicioRepository>();
builder.Services.AddScoped<IEstilistaRepository, EstilistaRepository>();
builder.Services.AddScoped<IEstilistaAgendaRepository, EstilistaAgendaRepository>();

// Servicios de Infraestructura
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMQMessagePublisher>();

// Servicios de Aplicación
builder.Services.AddScoped<IEstilistaService, EstilistaService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IEstilistaAgendaService, EstilistaAgendaService>();

// Esto permite inyectar IReservacionClient en los servicios
builder.Services.AddHttpClient<IReservacionClient, ReservacionClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5288/");

    // Timeout de seguridad para que el monolito no se quede colgado si el microservicio no responde
    client.Timeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(config =>
    {
        config.ConfigObject.PersistAuthorization = true;
    });
}

app.UseMiddleware<Peluqueria.API.Middleware.ExceptionMiddleware>();

app.UseCors(corsBuilder => corsBuilder
    .WithOrigins("http://localhost:4200", "https://localhost:4200")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();