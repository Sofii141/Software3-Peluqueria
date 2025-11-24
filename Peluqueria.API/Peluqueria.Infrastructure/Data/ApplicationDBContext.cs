using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Peluqueria.Domain.Entities;
using Peluqueria.Infrastructure.Identity;

namespace Peluqueria.Infrastructure.Data
{
    public class ApplicationDBContext : IdentityDbContext<AppUser>
    {
        // Nuevas constantes para Seed Data de prueba
        private const string LAURA_ID = "b7e289d1-d21a-4c9f-8d7e-00bd9344e575";
        private const string JUAN_ID = "c7e289d1-d21a-4c9f-8d7e-00bd9344e575";
        private const int LAURA_ESTILISTA_ID = 2;

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
                new IdentityRole { Id = "e10b1a03-9f2d-45f8-8f8e-20bc8b67b7e3", Name = "Cliente", NormalizedName = "CLIENTE" },
                new IdentityRole { Id = "a20b1a03-9f2d-45f8-8f8e-20bc8b67b7e3", Name = "Estilista", NormalizedName = "ESTILISTA" },
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
                 },
                 new Servicio
                 {
                     Id = 4,
                     Nombre = "Corte para hombre",
                     Descripcion = "Corte clásico para caballero.",
                     Precio = 30000,
                     DuracionMinutos = 45,
                     Imagen = "https://localhost:7274/images/manCut.png",
                     FechaCreacion = new DateTime(2025, 10, 23),
                     Disponible = true,
                     CategoriaId = 1
                 }
            );

            var hasher = new PasswordHasher<AppUser>();
            var userList = new List<AppUser>();
            var userRoleList = new List<IdentityUserRole<string>>();


            // --- SEED DE USUARIO ADMIN (ID: 1) ---
            var (adminUser, adminUserRole) = AdminUserSeed.CreateAdminUserWithRole();
            userList.Add(adminUser);
            userRoleList.Add(adminUserRole);


            // --- SEED DE USUARIO ESTILISTA (LAURA - ID: 2) ---
            var lauraUser = new AppUser
            {
                Id = LAURA_ID,
                UserName = "laura.e",
                NormalizedUserName = "LAURA.E",
                Email = "laura.e@pelu.com",
                NormalizedEmail = "LAURA.E@PELU.COM",
                EmailConfirmed = true,
                NombreCompleto = "Laura Valencia",
                Telefono = "3001234568",
                SecurityStamp = new Guid().ToString(),
                ConcurrencyStamp = new Guid().ToString()
            };
            lauraUser.PasswordHash = hasher.HashPassword(lauraUser, "Laura123*");
            userList.Add(lauraUser);
            userRoleList.Add(new IdentityUserRole<string> { RoleId = "a20b1a03-9f2d-45f8-8f8e-20bc8b67b7e3", UserId = LAURA_ID });


            // --- SEED DE USUARIO CLIENTE (JUAN - ID: 3) ---
            var juanUser = new AppUser
            {
                Id = JUAN_ID,
                UserName = "juan.c",
                NormalizedUserName = "JUAN.C",
                Email = "juan.c@mail.com",
                NormalizedEmail = "JUAN.C@MAIL.COM",
                EmailConfirmed = true,
                NombreCompleto = "Juan Cliente",
                Telefono = "3109876543",
                SecurityStamp = new Guid().ToString(),
                ConcurrencyStamp = new Guid().ToString()
            };
            juanUser.PasswordHash = hasher.HashPassword(juanUser, "Cliente123*");
            userList.Add(juanUser);
            userRoleList.Add(new IdentityUserRole<string> { RoleId = "e10b1a03-9f2d-45f8-8f8e-20bc8b67b7e3", UserId = JUAN_ID });


            // Aplicar Seeds de Identity
            builder.Entity<AppUser>().HasData(userList);
            builder.Entity<IdentityUserRole<string>>().HasData(userRoleList);


            // --- SEED DE PERFILES DE DOMINIO ---

            // Estilista Admin (ID: 1)
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

            // Estilista Laura (ID: 2)
            builder.Entity<Estilista>().HasData(
                new Estilista
                {
                    Id = LAURA_ESTILISTA_ID,
                    IdentityId = LAURA_ID,
                    NombreCompleto = "Laura Valencia",
                    Telefono = "3001234568",
                    EstaActivo = true
                }
            );

            // --- SEED DE ASOCIACIÓN DE SERVICIOS (M:N) ---

            // Admin Servicios (ID: 1, 2, 3)
            builder.Entity<EstilistaServicio>().HasData(
                new EstilistaServicio { EstilistaId = 1, ServicioId = 1 },
                new EstilistaServicio { EstilistaId = 1, ServicioId = 2 },
                new EstilistaServicio { EstilistaId = 1, ServicioId = 3 }
            );

            // Laura Servicios (ID: 1, 4)
            builder.Entity<EstilistaServicio>().HasData(
                new EstilistaServicio { EstilistaId = LAURA_ESTILISTA_ID, ServicioId = 1 },
                new EstilistaServicio { EstilistaId = LAURA_ESTILISTA_ID, ServicioId = 4 }
            );

            // --- SEED DE AGENDA (PARA LAURA) ---

            // Horario Semanal Base (PEL-HU-12)
            builder.Entity<HorarioSemanalBase>().HasData(
                new HorarioSemanalBase { Id = 1, EstilistaId = LAURA_ESTILISTA_ID, DiaSemana = DayOfWeek.Monday, HoraInicioJornada = new TimeSpan(9, 0, 0), HoraFinJornada = new TimeSpan(18, 0, 0), EsLaborable = true },
                new HorarioSemanalBase { Id = 2, EstilistaId = LAURA_ESTILISTA_ID, DiaSemana = DayOfWeek.Wednesday, HoraInicioJornada = new TimeSpan(9, 0, 0), HoraFinJornada = new TimeSpan(18, 0, 0), EsLaborable = true },
                new HorarioSemanalBase { Id = 3, EstilistaId = LAURA_ESTILISTA_ID, DiaSemana = DayOfWeek.Friday, HoraInicioJornada = new TimeSpan(9, 0, 0), HoraFinJornada = new TimeSpan(18, 0, 0), EsLaborable = true }
            );

            // Bloqueo Descanso Fijo Diario (Almuerzo - PEL-HU-13)
            builder.Entity<BloqueoDescansoFijoDiario>().HasData(
                new BloqueoDescansoFijoDiario { Id = 1, EstilistaId = LAURA_ESTILISTA_ID, DiaSemana = DayOfWeek.Monday, HoraInicioDescanso = new TimeSpan(13, 0, 0), HoraFinDescanso = new TimeSpan(14, 0, 0), Razon = "Almuerzo" },
                new BloqueoDescansoFijoDiario { Id = 2, EstilistaId = LAURA_ESTILISTA_ID, DiaSemana = DayOfWeek.Wednesday, HoraInicioDescanso = new TimeSpan(13, 0, 0), HoraFinDescanso = new TimeSpan(14, 0, 0), Razon = "Almuerzo" },
                new BloqueoDescansoFijoDiario { Id = 3, EstilistaId = LAURA_ESTILISTA_ID, DiaSemana = DayOfWeek.Friday, HoraInicioDescanso = new TimeSpan(13, 0, 0), HoraFinDescanso = new TimeSpan(14, 0, 0), Razon = "Almuerzo" }
            );

            // Bloqueo Rango Días Libres (Vacaciones Futuras - PEL-HU-13)
            builder.Entity<BloqueoRangoDiasLibres>().HasData(
                new BloqueoRangoDiasLibres { Id = 1, EstilistaId = LAURA_ESTILISTA_ID, FechaInicioBloqueo = new DateTime(2026, 1, 15).Date, FechaFinBloqueo = new DateTime(2026, 1, 18).Date, Razon = "Vacaciones" }
            );
        }
    }
}