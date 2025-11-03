using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoDBP.Models
{
    public class Cita
    {
        [Key]
        public int IdCita { get; set; }

        [ForeignKey("Usuario")]
        public int IdUsuario { get; set; }

        [ForeignKey("Servicio")]
        public int IdServicio { get; set; }

        [ForeignKey("StaffMedico")]
        public int IdStaffMedico { get; set; }

        public DateTime Fecha { get; set; }

        public string? Comentarios { get; set; }

        // Navegación
        public virtual Usuario Usuario { get; set; }
        public virtual Servicio Servicio { get; set; }
        public virtual StaffMedico StaffMedico { get; set; }
    }
}
