using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoDBP.Models
{
    public class DoctorDisponibilidad
    {
        [Key]
        public int IdDoctorDisponibilidad { get; set; }

        [ForeignKey("StaffMedico")]
        public int IdStaffMedico { get; set; }

        public string DiaSemana { get; set; }
        public string HoraInicio { get; set; }
        public string HoraFin { get; set; }

        public virtual StaffMedico StaffMedico { get; set; }
    }
}
