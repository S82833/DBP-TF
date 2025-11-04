using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProyectoDBP.ViewModels
{
    public class AgendarCitaViewModels
    {
        public int IdServicio { get; set; }
        public int IdStaffMedico { get; set; }

        // Fecha seleccionada (solo la parte de día, sin hora)
        public DateTime Fecha { get; set; }  // del input type="date"

        // Hora seleccionada en el <select> (formato "HH:mm", ej: "09:00")
        public string Hora { get; set; } = "";

        public IEnumerable<SelectListItem>? Especialidades { get; set; }
        public IEnumerable<SelectListItem>? Medicos { get; set; }
        public IEnumerable<string>? Horarios { get; set; }
    }
}
