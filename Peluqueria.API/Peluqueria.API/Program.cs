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
using System.Globalization; // <-- Necesario

var builder = WebApplication.CreateBuilder(args);

// --- CULTURA INVARIANTE PARA MODEL BINDING ---
// Establece la cultura para la aplicación/hilo. Usar la cultura Invariante o en-US 
// asegura que el separador decimal sea el punto (.), estándar para APIs.
var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Si usa .NET 6/7/8, esta configuración es la que aplica a la app y threads por defecto
// Puede que solo necesite configurar el Model Binding.
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
    // ... (resto del código de SwaggerGen)
    // ... (el resto de tu código sigue aquí sin cambios)
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

// El .AddControllers().AddNewtonsoftJson... fue modificado arriba, lo borramos de aquí:
// builder.Services.AddControllers().AddNewtonsoftJson(options =>
// {
//     options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
// });


// --- DATABASE AND IDENTITY CONFIGURATION ---
builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentity<AppUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDBContext>();

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
            Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])
        )
    };
});


// INYECCION DE DEPENDENCIAS
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IServicioRepository, ServicioRepository>();

builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();

builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IServicioService, ServicioService>();
builder.Services.AddScoped<IAccountService, AccountService>();
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
