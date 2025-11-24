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

var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    })
    // Opcional: Para asegurar que el Model Binding use la cultura correcta
    .AddMvcOptions(options =>
    {
        options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
            _ => "El valor es requerido.");
    });
// FIN DE LA MODIFICACIÓN DE CULTURA

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
    options.User.RequireUniqueEmail = true; // Unicidad de Correo
    // (8 caracteres, Mayús, Minús, Número, Especial)**
    options.Password.RequireDigit = true; // Requiere Número
    options.Password.RequiredLength = 8;  // Requiere 8 caracteres (o más)
    options.Password.RequireNonAlphanumeric = true; // Requiere Carácter Especial
    options.Password.RequireUppercase = true; // Requiere Mayúscula
    options.Password.RequireLowercase = true; // Requiere Minúscula
    options.Password.RequiredUniqueChars = 1; // Un carácter especial es suficiente
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
        // Uso del operador de nulidad (!) ya que la clave se valida y se lanza 
        // una excepción en TokenService.cs, garantizando que esté presente en runtime.
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"]!)
        )
    };
});


// INYECCION DE DEPENDENCIAS
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IServicioRepository, ServicioRepository>();
builder.Services.AddScoped<IEstilistaRepository, EstilistaRepository>();
builder.Services.AddScoped<IEstilistaAgendaRepository, EstilistaAgendaRepository>();

builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IEstilistaService, EstilistaService>();

builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMQMessagePublisher>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IEstilistaAgendaService, EstilistaAgendaService>();

var app = builder.Build();

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(config =>
    {
        // Esta línea indica a Swagger UI que guarde las claves de autenticación
        config.ConfigObject.PersistAuthorization = true;
    });
}

app.UseCors(corsBuilder => corsBuilder
    .WithOrigins("http://localhost:4200", "https://localhost:4200")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();