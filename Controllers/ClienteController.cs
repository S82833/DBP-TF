using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoDBP.Datos;

namespace ProyectoDBP.Controllers
{
    public class ClienteController : Controller
    {
        private readonly ApplicationDBContext _context;
        public ClienteController(ApplicationDBContext context) => _context = context;

        // GET /Cliente/agendarCita?especialidad=Endodoncia&idMedico=5
        public IActionResult agendarCita(string? especialidad, int? idMedico)
        {
            // Requiere sesión
            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName))
            {
                var returnUrl = Url.Action("agendarCita", "Cliente", new { especialidad, idMedico });
                TempData["ErrorMessage"] = "Debes iniciar sesión para agendar una cita.";
                return RedirectToAction("Login", "Account", new { returnUrl });
            }

            // 1) ESPECIALIDADES = tabla Servicios
            var servicios = _context.Servicios
                                    .AsNoTracking()
                                    .OrderBy(s => s.Nombre)
                                    .ToList();

            ViewBag.Especialidades = new SelectList(
                items: servicios,
                dataValueField: "Nombre",     // valor = nombre del servicio
                dataTextField: "Nombre",      // texto = nombre del servicio
                selectedValue: especialidad
            );

            // 2) MÉDICOS filtrados por servicio si viene 'especialidad' (via tabla puente ServiciosStaff)
            var medicosQuery = _context.StaffMedico.AsQueryable();

            if (!string.IsNullOrEmpty(especialidad))
            {
                var servicioId = _context.Servicios
                                         .Where(s => s.Nombre == especialidad)
                                         .Select(s => s.IdServicio)
                                         .FirstOrDefault();

                if (servicioId != 0)
                {
                    medicosQuery =
                        from ss in _context.ServiciosStaff.Include(ss => ss.StaffMedico)
                        where ss.IdServicio == servicioId
                        select ss.StaffMedico;
                }
                else
                {
            // 3) Horarios disponibles para el mdico seleccionado
            SelectList? horariosSelect = null;
            string? mensajeHorario = null;

            if (idMedico.HasValue)
            {
                var horarios = _context.DoctorDisponibilidades
                    .AsNoTracking()
                    .Where(h => h.IdStaffMedico == idMedico.Value)
                    .OrderBy(h => h.DiaSemana)
                    .ThenBy(h => h.HoraInicio)
                    .Select(h => new
                    {
                        h.IdDoctorDisponibilidad,
                        Descripcion = $"{h.DiaSemana}: {h.HoraInicio} - {h.HoraFin}"
                    })
                    .ToList();

                if (horarios.Any())
                {
                    horariosSelect = new SelectList(horarios, "IdDoctorDisponibilidad", "Descripcion");
                }
                else
                {
                    mensajeHorario = "El doctor seleccionado no est atendiendo actualmente.";
                }
            }

            ViewBag.Horarios = horariosSelect;
            ViewBag.MensajeHorario = mensajeHorario;

                    // Si no encuentra el servicio, devolvemos lista vacía para no confundir
                    medicosQuery = _context.StaffMedico.Where(m => false);
                }
            }

            var medicos = medicosQuery
                .AsNoTracking()
                .OrderBy(m => m.Apellido)
                .Select(m => new { m.IdStaffMedico, Nombre = m.Nombre + " " + m.Apellido })
                .ToList();

            ViewBag.Medicos = new SelectList(medicos, "IdStaffMedico", "Nombre", idMedico);

            return View();
        }
    }
}
