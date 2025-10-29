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
            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName))
            {
                var returnUrl = Url.Action("agendarCita", "Cliente", new { especialidad, idMedico });
                TempData["ErrorMessage"] = "Debes iniciar sesión para agendar una cita.";
                return RedirectToAction("Login", "Account", new { returnUrl });
            }

            // 1) ESPECIALIDADES
            var servicios = _context.Servicios
                                    .AsNoTracking()
                                    .OrderBy(s => s.Nombre)
                                    .ToList();

            ViewBag.Especialidades = new SelectList(servicios, "Nombre", "Nombre", especialidad);

            // 2) MÉDICOS
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
                    // Si no encuentra el servicio, devuelve lista vacía
                    medicosQuery = _context.StaffMedico.Where(m => false);
                }
            }

            var medicos = medicosQuery
                .AsNoTracking()
                .OrderBy(m => m.Apellido)
                .Select(m => new { m.IdStaffMedico, Nombre = m.Nombre + " " + m.Apellido })
                .ToList();

            ViewBag.Medicos = new SelectList(medicos, "IdStaffMedico", "Nombre", idMedico);

            // 3) HORARIOS DISPONIBLES
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
                    mensajeHorario = "El doctor seleccionado no está atendiendo actualmente.";
                }
            }

            ViewBag.Horarios = horariosSelect;
            ViewBag.MensajeHorario = mensajeHorario;

            return View();
        }

        [HttpGet]
        public IActionResult ObtenerHorarios(int idMedico, DateTime fecha)
        {
            // Determinar el nombre del día
            var nombreDia = fecha.ToString("dddd", new System.Globalization.CultureInfo("es-ES"));
            nombreDia = char.ToUpper(nombreDia[0]) + nombreDia.Substring(1); // capitalizar (Lunes, Martes, etc.)

            var disponibilidad = _context.DoctorDisponibilidades
                .AsNoTracking()
                .FirstOrDefault(d => d.IdStaffMedico == idMedico && d.DiaSemana == nombreDia);

            if (disponibilidad == null)
            {
                return Json(new { success = false, mensaje = $"El doctor no atiende los días {nombreDia}." });
            }

            // Generar intervalos de 30 minutos
            var horaInicio = TimeSpan.Parse(disponibilidad.HoraInicio);
            var horaFin = TimeSpan.Parse(disponibilidad.HoraFin);
            var horarios = new List<string>();

            for (var h = horaInicio; h < horaFin; h = h.Add(TimeSpan.FromMinutes(30)))
            {
                horarios.Add(h.ToString(@"hh\:mm"));
            }

            return Json(new { success = true, horarios });
        }


    }
}
