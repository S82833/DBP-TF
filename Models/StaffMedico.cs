using System.ComponentModel.DataAnnotations;

namespace ProyectoDBP.Models
{
    public class StaffMedico
    {
        [Key]
        public int IdStaffMedico { get; set; }

        public string Nombre { get; set; }
        public string Apellido { get; set; }

        public string Especialidad { get; set; }
        public string Biografia { get; set; }

        public virtual ICollection<Cita> Citas { get; set; }
        public ICollection<ServicioStaff> ServiciosStaff { get; set; } = new List<ServicioStaff>();
        public virtual ICollection<DoctorDisponibilidad> Disponibilidades { get; set; }
    }
}
