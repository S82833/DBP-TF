using System.ComponentModel.DataAnnotations;

namespace ProyectoDBP.Models
{
    public class Servicio
    {
        [Key]
        public int IdServicio { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public virtual ICollection<Cita> Citas { get; set; }
        public ICollection<ServicioStaff> ServiciosStaff { get; set; } = new List<ServicioStaff>();
    }
}
