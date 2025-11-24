using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Peluqueria.Domain.Entities;
using Peluqueria.Infrastructure.Identity;

namespace Peluqueria.Infrastructure.Data
{
    public class ApplicationDBContext : IdentityDbContext<AppUser>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }

        // DbSets Existentes
        public DbSet<Categoria> Categorias { get; set; } = null!;
        public DbSet<Servicio> Servicios { get; set; } = null!;

        // Nuevos DbSets para la Arquitectura y Microservicio
        public DbSet<Estilista> Estilistas { get; set; } = null!;
        public DbSet<EstilistaServicio> EstilistaServicios { get; set; } = null!;
        public DbSet<ParametroSistema> ParametrosSistema { get; set; } = null!;

        public DbSet<HorarioSemanalBase> HorariosSemanalBase { get; set; } = null!;
        public DbSet<BloqueoRangoDiasLibres> BloqueosRangoDiasLibres { get; set; } = null!;
        public DbSet<BloqueoDescansoFijoDiario> BloqueosDescansoFijoDiario { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuración de la relación M:N Estilista-Servicio
            builder.Entity<EstilistaServicio>()
                .HasKey(es => new { es.EstilistaId, es.ServicioId });

            // Seed de Roles
            List<IdentityRole> roles = new List<IdentityRole>
            {
                new IdentityRole { Id = AdminUserSeed.ADMIN_ROLE_ID, Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Name = "Cliente", NormalizedName = "CLIENTE" },
                new IdentityRole { Name = "Estilista", NormalizedName = "ESTILISTA" },
            };
            builder.Entity<IdentityRole>().HasData(roles);

            // Seed de Parametros del Sistema (PEL-HU-25)
            builder.Entity<ParametroSistema>().HasData(
                new ParametroSistema
                {
                    Id = 1,
                    BufferMinutos = 5,
                    ToleranciaLlegadaMinutos = 10,
                    DuracionMinimaServicioMinutos = 45
                }
            );

            // Seed de Categorías (con baja lógica por defecto)
            builder.Entity<Categoria>().HasData(
                new Categoria { Id = 1, Nombre = "Cortes de Cabello", EstaActiva = true },
                new Categoria { Id = 2, Nombre = "Tratamientos Capilares", EstaActiva = true },
                new Categoria { Id = 3, Nombre = "Manicura y Pedicura", EstaActiva = true },
                new Categoria { Id = 4, Nombre = "Otra", EstaActiva = true }
            );

            // Seed de Servicios (con DuracionMinutos añadido)
            builder.Entity<Servicio>().HasData(
                new Servicio
                {
                    Id = 1,
                    Nombre = "Corte Estilo Bob",
                    Descripcion = "Un corte clásico y elegante por encima de los hombros.",
                    Precio = 50000,
                    DuracionMinutos = 60,
                    Imagen = "https://localhost:7274/images/bobCut.png",
                    FechaCreacion = new DateTime(2025, 10, 23),
                    Disponible = true,
                    CategoriaId = 1
                },
                new Servicio
                {
                    Id = 2,
                    Nombre = "Hidratación Profunda",
                    Descripcion = "Tratamiento a base de keratina para revitalizar tu cabello.",
                    Precio = 80000,
                    DuracionMinutos = 90,
                    Imagen = "https://localhost:7274/images/hidratacion.jpg",
                    FechaCreacion = new DateTime(2025, 10, 23),
                    Disponible = true,
                    CategoriaId = 2
                },
                 new Servicio
                 {
                     Id = 3,
                     Nombre = "Manicura SPA Completa",
                     Descripcion = "Una experiencia relajante que dejará tus manos suaves y perfectas.",
                     Precio = 80000,
                     DuracionMinutos = 75,
                     Imagen = "https://localhost:7274/images/manicura.jpg",
                     FechaCreacion = new DateTime(2025, 10, 23),
                     Disponible = true,
                     CategoriaId = 3
                 }
            );

            // Creación del usuario Admin de Identity
            var (adminUser, adminUserRole) = AdminUserSeed.CreateAdminUserWithRole();
            builder.Entity<AppUser>().HasData(adminUser);
            builder.Entity<IdentityUserRole<string>>().HasData(adminUserRole);

            // Creación del perfil Estilista para el usuario Admin 
            builder.Entity<Estilista>().HasData(
                new Estilista
                {
                    Id = 1,
                    IdentityId = AdminUserSeed.ADMIN_ID,
                    NombreCompleto = "Administrador Principal",
                    Telefono = "3001234567",
                    EstaActivo = true
                }
            );

            // Asociación de Servicios al Estilista Admin
            builder.Entity<EstilistaServicio>().HasData(
                new EstilistaServicio { EstilistaId = 1, ServicioId = 1 },
                new EstilistaServicio { EstilistaId = 1, ServicioId = 2 },
                new EstilistaServicio { EstilistaId = 1, ServicioId = 3 }
            );
        }
    }
}