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

        public DbSet<Categoria> Categorias { get; set; } = null!;
        public DbSet<Servicio> Servicios { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            List<IdentityRole> roles = new List<IdentityRole>
    {
                    // El ID del rol Admin debe coincidir con el de AdminUserSeed.cs
                    new IdentityRole { Id = "d17abceb-8c0b-454e-b296-883bc029d82b", Name = "Admin", NormalizedName = "ADMIN" },
                    new IdentityRole { Name = "Cliente", NormalizedName = "CLIENTE" },
                    new IdentityRole { Name = "Estilista", NormalizedName = "ESTILISTA" },
                };
            builder.Entity<IdentityRole>().HasData(roles);

            builder.Entity<Categoria>().HasData(
                new Categoria { Id = 1, Nombre = "Cortes de Cabello" },
                new Categoria { Id = 2, Nombre = "Tratamientos Capilares" },
                new Categoria { Id = 3, Nombre = "Manicura y Pedicura" }
            );

            // Seeding de Servicios
            builder.Entity<Servicio>().HasData(
                new Servicio
                {
                    Id = 1,
                    Nombre = "Corte Estilo Bob",
                    Descripcion = "Un corte clásico y elegante por encima de los hombros.",
                    Precio = 50000,
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
                     Imagen = "https://localhost:7274/images/manicura.jpg",
                     FechaCreacion = new DateTime(2025, 10, 23),
                     Disponible = true,
                     CategoriaId = 3
                 }
            );


            // Seeding del Usuario Administrador
            // 1. Llamamos a nuestra clase de seeding para obtener los objetos.
            var (adminUser, adminUserRole) = AdminUserSeed.CreateAdminUserWithRole();

            // 2. Le decimos a EF Core que cree este usuario cuando se genere la base de datos.
            builder.Entity<AppUser>().HasData(adminUser);

            // 3. Le decimos a EF Core que cree la relación entre el usuario y el rol.
            builder.Entity<IdentityUserRole<string>>().HasData(adminUserRole);
        }
    }
}