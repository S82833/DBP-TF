using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProyectoDBP.Models;

namespace ProyectoDBP.ViewModels
{
    public class DoctorDisponibilidadResumenViewModel
    {
        public int IdStaffMedico { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public int DisponibilidadesRegistradas { get; set; }
    }

    public class DoctorDisponibilidadFormModel
    {
        [Required]
        public int IdStaffMedico { get; set; }

        [Required(ErrorMessage = "Selecciona un día de la semana.")]
        [Display(Name = "Día")]
        public string DiaSemana { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona una hora de inicio.")]
        [Display(Name = "Hora inicio")]
        public string HoraInicio { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona una hora de fin.")]
        [Display(Name = "Hora fin")]
        public string HoraFin { get; set; } = string.Empty;
    }

    public class DoctorDisponibilidadGestionViewModel
    {
        public int IdStaffMedico { get; set; }
        public string DoctorNombre { get; set; } = string.Empty;
        public IList<DoctorDisponibilidad> Disponibilidades { get; set; } = new List<DoctorDisponibilidad>();
        public DoctorDisponibilidadFormModel Form { get; set; } = new();
        public IEnumerable<SelectListItem> DiasSemana { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> HorariosInicio { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> HorariosFin { get; set; } = Enumerable.Empty<SelectListItem>();
    }
}
