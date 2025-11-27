using Microsoft.EntityFrameworkCore;
using peluqueria.reservaciones.Core.Dominio;

namespace peluqueria.reservaciones.Infraestructura.Persistencia
{
    public class ReservacionesDbContext : DbContext
    {
        // 1. EL CONSTRUCTOR
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
            // 1. Mapeo de la relación 1:N entre HorarioBase y DiaHorario
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

            // Configura HorariosSemanales como una colección de Tipos Poseídos
            modelBuilder.Entity<HorarioBase>()
                .OwnsMany(h => h.HorariosSemanales, dh =>
                {
                    // La tabla de la base de datos se llamará HorarioBase_DiaHorario
                    dh.ToTable("HorarioBase_Dias");

                    // Define una clave primaria compuesta para la tabla de días/horarios
                    dh.HasKey("Id"); // EF Core creará una clave 'Id' oculta

                    // Añade una FK en sombra que apunta a HorarioBase (EstilistaId)
                    dh.WithOwner().HasForeignKey("HorarioBaseEstilistaId");
                });


            // 2. Mapeo de la relación 1:N entre DescansoFijo y DiaHorario
            modelBuilder.Entity<DescansoFijo>().HasKey(d => d.EstilistaId);

            modelBuilder.Entity<DescansoFijo>()
            .Property(d => d.EstilistaId)
            .ValueGeneratedNever();

            // Configura DescansosFijos como una colección de Tipos Poseídos
            modelBuilder.Entity<DescansoFijo>()
                .OwnsMany(d => d.DescansosFijos, df =>
                {
                    // La tabla de la base de datos se llamará DescansoFijo_Dias
                    df.ToTable("DescansoFijo_Dias");

                    // Define una clave primaria compuesta para la tabla de días/horarios
                    df.HasKey("Id");

                    // Añade una FK en sombra que apunta a DescansoFijo (EstilistaId)
                    df.WithOwner().HasForeignKey("DescansoFijoEstilistaId");
                });


            // 3. Configuración de Clientes
            // Aseguramos que la Identificacion sea la clave primaria
            modelBuilder.Entity<Cliente>().HasKey(c => c.Identificacion);

            // 4. Configuración de BloqueoRangoDiasLibres
            // Ya que no tiene una clave 'Id' auto-generada, usamos una clave compuesta (EstilistaId, FechaInicioBloqueo)
            modelBuilder.Entity<BloqueoRangoDiasLibres>()
                .HasKey(b => new { b.EstilistaId, b.FechaInicioBloqueo });

            // 1. Relación con Cliente
            modelBuilder.Entity<Reservacion>()
                .HasOne(r => r.Cliente) // Una reservación tiene UN cliente
                .WithMany()             // El cliente puede tener MUCHAS reservaciones (no mapeamos la lista en Cliente)
                .HasForeignKey(r => r.ClienteIdentificacion) // Usamos el campo que acabamos de agregar
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict); // Evitamos la eliminación en cascada de citas si se borra un cliente

            // 2. Relación con Servicio
            modelBuilder.Entity<Reservacion>()
                .HasOne(r => r.Servicio) // Una reservación tiene UN servicio
                .WithMany()
                .HasForeignKey(r => r.ServicioId) // Usamos el campo ServicioId
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Relación con Estilista
            modelBuilder.Entity<Reservacion>()
                .HasOne(r => r.Estilista) // Una reservación tiene UN estilista
                .WithMany()
                .HasForeignKey(r => r.EstilistaId) // Usamos el campo EstilistaId
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Llama al base para convenciones por defecto
            base.OnModelCreating(modelBuilder);
        }
    }
}