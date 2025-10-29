using Microsoft.EntityFrameworkCore;
using ProyectoDBP.Models;

namespace ProyectoDBP.Datos
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<Servicio> Servicios { get; set; }
        public DbSet<StaffMedico> StaffMedico { get; set; }
        public DbSet<ServicioStaff> ServiciosStaff { get; set; }
        public DbSet<DoctorDisponibilidad> DoctorDisponibilidades { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ServicioStaff>()
                .HasKey(ss => ss.IdServiciosStaff);

            modelBuilder.Entity<ServicioStaff>()
                .HasOne(ss => ss.Servicio)
                .WithMany(s => s.ServiciosStaff)
                .HasForeignKey(ss => ss.IdServicio)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServicioStaff>()
                .HasOne(ss => ss.StaffMedico)
                .WithMany(m => m.ServiciosStaff)
                .HasForeignKey(ss => ss.IdStaffMedico)
                .OnDelete(DeleteBehavior.Cascade);

            // Evitar duplicados (mismo servicio–médico)
            modelBuilder.Entity<ServicioStaff>()
                .HasIndex(ss => new { ss.IdServicio, ss.IdStaffMedico })
                .IsUnique();
        }

    }
}
