using Microsoft.EntityFrameworkCore;
using peluqueria.reservaciones.Core.Dominio;

/*
 @author: Juan david Moran
    @description: Clase DbContext para la configuración de la base de datos de reservaciones.
 */

namespace peluqueria.reservaciones.Infraestructura.Persistencia
{
    public class ReservacionesDbContext : DbContext
    {
        public ReservacionesDbContext(DbContextOptions<ReservacionesDbContext> options)
            : base(options)
        {
        }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<Estilista> Estilistas { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<HorarioBase> Horarios { get; set; }
        public DbSet<BloqueoRangoDiasLibres> BloqueoRangoDias { get; set; }
        public DbSet<DescansoFijo> DescansoFijo { get; set; }
        public DbSet<Reservacion> Reservaciones { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HorarioBase>().HasKey(h => h.EstilistaId);

            modelBuilder.Entity<HorarioBase>()
            .Property(h => h.EstilistaId)
            .ValueGeneratedNever();

            modelBuilder.Entity<Categoria>()
        .Property(c => c.Id)
        .ValueGeneratedNever();

            modelBuilder.Entity<Servicio>()
                .Property(s => s.Precio)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Servicio>()
            .Property(s => s.Id)
            .ValueGeneratedNever();

            modelBuilder.Entity<Estilista>()
                .Property(e => e.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<HorarioBase>()
                .OwnsMany(h => h.HorariosSemanales, dh =>
                {
                    dh.ToTable("HorarioBase_Dias");

                    dh.HasKey("Id"); 

                    dh.WithOwner().HasForeignKey("HorarioBaseEstilistaId");
                });


            modelBuilder.Entity<DescansoFijo>().HasKey(d => d.EstilistaId);

            modelBuilder.Entity<DescansoFijo>()
            .Property(d => d.EstilistaId)
            .ValueGeneratedNever();

            modelBuilder.Entity<DescansoFijo>()
                .OwnsMany(d => d.DescansosFijos, df =>
                {
                    df.ToTable("DescansoFijo_Dias");

                    df.HasKey("Id");

                    df.WithOwner().HasForeignKey("DescansoFijoEstilistaId");
                });


            modelBuilder.Entity<Cliente>().HasKey(c => c.Identificacion);

            modelBuilder.Entity<BloqueoRangoDiasLibres>()
                .HasKey(b => new { b.EstilistaId, b.FechaInicioBloqueo });

            modelBuilder.Entity<Reservacion>()
                .HasOne(r => r.Cliente) 
                .WithMany()             
                .HasForeignKey(r => r.ClienteIdentificacion) 
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservacion>()
                .HasOne(r => r.Servicio) 
                .WithMany()
                .HasForeignKey(r => r.ServicioId) 
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservacion>()
                .HasOne(r => r.Estilista) 
                .WithMany()
                .HasForeignKey(r => r.EstilistaId) 
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}