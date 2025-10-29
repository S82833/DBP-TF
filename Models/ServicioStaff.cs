using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProyectoDBP.Models;

namespace ProyectoDBP.Models
{
    public class ServicioStaff
    {
        [Key]
        public int IdServiciosStaff { get; set; }

        public int IdServicio { get; set; }
        public int IdStaffMedico { get; set; }

        [ForeignKey(nameof(IdServicio))]
        public Servicio Servicio { get; set; } = null!;

        [ForeignKey(nameof(IdStaffMedico))]
        public StaffMedico StaffMedico { get; set; } = null!;
    }
}
