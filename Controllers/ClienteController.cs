using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoDBP.Datos;
using ProyectoDBP.Models;
using ProyectoDBP.Services;

namespace ProyectoDBP.Controllers
{
    public class ClienteController : Controller
    {
        private readonly ApplicationDBContext _context;
        private readonly EmailService _emailService;

        public ClienteController(ApplicationDBContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgendarCita(string Especialidad, int Medico, DateTime Fecha, string Hora)
        {
            // --- Sesión y fallback por correo, para evitar que te redirija a Login al confirmar ---
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName");
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (userId == null && !string.IsNullOrEmpty(userEmail))
            {
                userId = await _context.Usuarios
                    .Where(u => u.Correo == userEmail)
                    .Select(u => (int?)u.IdUsuario)
                    .FirstOrDefaultAsync();

                if (userId != null)
                    HttpContext.Session.SetInt32("UserId", userId.Value);
            }

            if (userId == null || string.IsNullOrEmpty(userName))
            {
                TempData["ErrorMessage"] = "Debes iniciar sesión para agendar.";
                return RedirectToAction("Login", "Account",
                    new { returnUrl = Url.Action("agendarCita", "Cliente") });
            }

            // --- Validaciones de entrada ---
            if (string.IsNullOrWhiteSpace(Especialidad) || Medico <= 0 || Fecha == default || string.IsNullOrWhiteSpace(Hora))
            {
                TempData["ErrorMessage"] = "Completa Especialidad, Médico, Fecha y Hora.";
                return RedirectToAction("agendarCita", new { especialidad = Especialidad, idMedico = Medico });
            }

            var servicioId = await _context.Servicios
                .Where(s => s.Nombre == Especialidad)
                .Select(s => s.IdServicio)
                .FirstOrDefaultAsync();

            if (servicioId == 0)
            {
                TempData["ErrorMessage"] = "La especialidad seleccionada no existe.";
                return RedirectToAction("agendarCita");
            }

            // --- Fecha/hora seleccionada ---
            var ts = TimeSpan.Parse(Hora);
            var fechaHora = Fecha.Date.Add(ts);

            // --- Verificación rápida en memoria ---
            var yaTomada = await _context.Citas
                .AnyAsync(c => c.IdStaffMedico == Medico && c.Fecha == fechaHora);

            if (yaTomada)
            {
                TempData["ErrorMessage"] = "Esa hora ya fue tomada por otro paciente. Elige otra.";
                return RedirectToAction("agendarCita", new { especialidad = Especialidad, idMedico = Medico });
            }

            // --- Inserción con blindaje. El índice único en BD evita carreras. ---
            var cita = new Cita
            {
                IdUsuario = userId.Value,
                IdServicio = servicioId,
                IdStaffMedico = Medico,
                Fecha = fechaHora
            };

            _context.Citas.Add(cita);

            try
            {
                await _context.SaveChangesAsync();

                // ====== Enviar correo de confirmación ======
                if (!string.IsNullOrEmpty(userEmail))
                {
                    string cuerpo = $@"
            <div style='font-family:Arial,sans-serif; color:#333;'>
                <h2>Confirmación de Cita</h2>
                <p>Estimado/a <strong>{userName}</strong>,</p>
                <p>Tu cita ha sido agendada correctamente.</p>
                <ul>
                    <li><b>Especialidad:</b> {Especialidad}</li>
                    <li><b>Doctor:</b> {await ObtenerNombreDoctor(Medico)}</li>
                    <li><b>Fecha:</b> {Fecha:dd/MM/yyyy}</li>
                    <li><b>Hora:</b> {Hora}</li>
                </ul>
                <p>Gracias por confiar en <strong>Odontobrass</strong>.</p>
            </div>";

                    await _emailService.EnviarCorreo(
                        userEmail,
                        "Confirmación de Cita - Odontobrass",
                        cuerpo
                    );
                }
            }
            catch (DbUpdateException)
            {
                // Si otro usuario ganó la carrera, el índice único rechaza la inserción
                TempData["ErrorMessage"] = "No se pudo reservar: esa hora ya fue tomada. Intenta con otra hora.";
                return RedirectToAction("agendarCita", new { especialidad = Especialidad, idMedico = Medico });
            }

            TempData["Success"] = $"Cita reservada para {fechaHora:dd/MM/yyyy HH:mm}.";


            return RedirectToAction("cronogramaCitas", "Cliente");
        }




        [HttpGet]
        public async Task<IActionResult> ObtenerHorarios(int idMedico, DateTime fecha)
        {
            // Día de la semana en ES (Lunes, Martes, ...)
            var nombreDia = fecha.ToString("dddd", new System.Globalization.CultureInfo("es-ES"));
            nombreDia = char.ToUpper(nombreDia[0]) + nombreDia.Substring(1);

            // Todas las disponibilidades del doctor para ese día
            var bloques = await _context.DoctorDisponibilidades
                .AsNoTracking()
                .Where(d => d.IdStaffMedico == idMedico && d.DiaSemana == nombreDia)
                .Select(d => new { d.HoraInicio, d.HoraFin })
                .ToListAsync();

            if (!bloques.Any())
            {
                return Json(new { success = false, mensaje = $"El doctor no atiende los días {nombreDia}." });
            }

            // Generar slots de 30 min fusionando rangos
            var slots = new HashSet<string>(); // evita duplicados
            foreach (var b in bloques)
            {
                var hi = TimeSpan.Parse(b.HoraInicio);
                var hf = TimeSpan.Parse(b.HoraFin);
                for (var t = hi; t < hf; t = t.Add(TimeSpan.FromMinutes(30)))
                    slots.Add(t.ToString(@"hh\:mm"));
            }

            // Quitar horas ya reservadas para ese doctor y fecha
            var citasEseDia = await _context.Citas
                .AsNoTracking()
                .Where(c => c.IdStaffMedico == idMedico && c.Fecha.Date == fecha.Date)
                .Select(c => c.Fecha.TimeOfDay)
                .ToListAsync();

            foreach (var taken in citasEseDia)
                slots.Remove(taken.ToString(@"hh\:mm"));

            var resultado = slots
                .Select(s => TimeSpan.Parse(s))
                .OrderBy(s => s)
                .Select(s => s.ToString(@"hh\:mm"))
                .ToList();

            if (!resultado.Any())
                return Json(new { success = false, mensaje = "No quedan horas disponibles para la fecha seleccionada." });

            return Json(new { success = true, horarios = resultado });
        }


        // ===============================
        // CRONOGRAMA (paciente o médico)
        // ===============================
        [HttpGet]
        public async Task<IActionResult> cronogramaCitas()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var rol = HttpContext.Session.GetInt32("UserRol");
            var userName = HttpContext.Session.GetString("UserName");

            if (userId == null || rol == null)
                return RedirectToAction("Login", "Account",
                    new { returnUrl = Url.Action("cronogramaCitas", "Cliente") });

            // Si es médico (rol=2) intento mapear su IdStaffMedico por nombre completo
            int? doctorId = null;
            if (rol == 2 && !string.IsNullOrWhiteSpace(userName))
            {
                var doc = await _context.StaffMedico.AsNoTracking()
                    .FirstOrDefaultAsync(m =>
                        (m.Nombre + " " + m.Apellido).ToLower() == userName.ToLower());
                if (doc != null) doctorId = doc.IdStaffMedico;
            }

            // IMPORTANTE: usar las navegaciones reales: Usuario, Servicio, StaffMedico
            var query = _context.Citas
                .Include(c => c.Usuario)
                .Include(c => c.Servicio)
                .Include(c => c.StaffMedico)
                .AsNoTracking()
                .AsQueryable();

            if (rol == 1)                       // paciente
                query = query.Where(c => c.IdUsuario == userId.Value);
            else if (rol == 2 && doctorId.HasValue) // médico
                query = query.Where(c => c.IdStaffMedico == doctorId.Value);
            else
                query = query.Where(c => false);     // por si no mapea

            var citas = await query
                .OrderBy(c => c.Fecha)
                .Select(c => new
                {
                    c.IdCita,
                    Paciente = c.Usuario.Nombre,
                    Doctor = c.StaffMedico.Nombre + " " + c.StaffMedico.Apellido,
                    Servicio = c.Servicio.Nombre,
                    Fecha = c.Fecha
                })
                .ToListAsync();

            ViewBag.EsMedico = (rol == 2);
            return View("cronogramaCitas", citas); // tu vista puede ser dinámica; si luego quieres fuertemente tipada, armamos un ViewModel.
        }


        // ===============================
        // CANCELAR CITA (POST)
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarCita(int id)
        {
            var (ok, rol, userId, doctorId) = await GetPermisoSobreCita(id);
            if (!ok) return Forbid();

            var cita = await _context.Citas
                .Include(c => c.Usuario)
                .Include(c => c.Servicio)
                .Include(c => c.StaffMedico)
                .FirstOrDefaultAsync(c => c.IdCita == id);

            if (cita == null) return NotFound();

            // validación de autorización
            if (!PuedeOperar(rol, userId, doctorId, cita)) return Forbid();

            // eliminamos la cita
            _context.Citas.Remove(cita);
            await _context.SaveChangesAsync();

            // ====== Enviar correo de cancelación ======
            var userEmail = cita.Usuario?.Correo;
            var userName = cita.Usuario?.Nombre;

            if (!string.IsNullOrEmpty(userEmail))
            {
                string cuerpo = $@"
        <div style='font-family:Arial,sans-serif; color:#333;'>
            <h2>Cancelación de Cita</h2>
            <p>Estimado/a <strong>{userName}</strong>,</p>
            <p>Tu cita ha sido cancelada correctamente.</p>
            <ul>
                <li><b>Especialidad:</b> {cita.Servicio?.Nombre}</li>
                <li><b>Doctor:</b> {cita.StaffMedico?.Nombre} {cita.StaffMedico?.Apellido}</li>
                <li><b>Fecha original:</b> {cita.Fecha:dd/MM/yyyy HH:mm}</li>
            </ul>
            <p>Si fue un error, puedes reprogramarla desde tu cuenta.</p>
            <p>Atentamente,<br><strong>Odontobrass</strong></p>
        </div>";

                await _emailService.EnviarCorreo(
                    userEmail,
                    "Cancelación de Cita - Odontobrass",
                    cuerpo
                );
            }
            // ==========================================

            TempData["Success"] = "La cita fue cancelada y se notificó por correo.";
            return RedirectToAction(nameof(cronogramaCitas));
        }

        // ===============================
        // REPROGRAMAR CITA (GET) — muestra la vista con la cita actual
        // ===============================
        [HttpGet]
        public async Task<IActionResult> ReprogramarCita(int id)
        {
            var (ok, rol, userId, doctorId) = await GetPermisoSobreCita(id);
            if (!ok) return Forbid();

            var cita = await _context.Citas
                .Include(c => c.Servicio)
                .Include(c => c.StaffMedico)
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.IdCita == id);

            if (cita == null) return NotFound();
            if (!PuedeOperar(rol, userId, doctorId, cita)) return Forbid();

            ViewBag.Info = new
            {
                cita.IdCita,
                IdStaffMedico = cita.IdStaffMedico,
                Doctor = $"{cita.StaffMedico?.Nombre} {cita.StaffMedico?.Apellido}",
                Servicio = cita.Servicio?.Nombre,
                FechaActual = cita.Fecha.ToString("dd/MM/yyyy HH:mm")
            };

            // La vista usará /Cliente/ObtenerHorarios para cargar horas válidas del doctor en la nueva fecha
            return View("reprogramarCita");
        }



        // ===============================
        // REPROGRAMAR CITA (POST) 
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReprogramarCita(int id, DateTime NuevaFecha, string NuevaHora)
        {
            var (ok, rol, userId, doctorId) = await GetPermisoSobreCita(id);
            if (!ok) return Forbid();

            var cita = await _context.Citas
                .Include(c => c.Usuario)
                .Include(c => c.Servicio)
                .Include(c => c.StaffMedico)
                .FirstOrDefaultAsync(c => c.IdCita == id);

            if (cita == null) return NotFound();
            if (!PuedeOperar(rol, userId, doctorId, cita)) return Forbid();

            if (NuevaFecha == default || string.IsNullOrWhiteSpace(NuevaHora))
            {
                TempData["ErrorMessage"] = "Selecciona nueva fecha y hora.";
                return RedirectToAction(nameof(ReprogramarCita), new { id });
            }

            // Validar contra duplicado (misma regla que crear)
            var ts = TimeSpan.Parse(NuevaHora);
            var nuevaFechaHora = NuevaFecha.Date.Add(ts);

            var colision = await _context.Citas
                .AnyAsync(c => c.IdStaffMedico == cita.IdStaffMedico && c.Fecha == nuevaFechaHora && c.IdCita != cita.IdCita);
            if (colision)
            {
                TempData["ErrorMessage"] = "Ese horario ya está ocupado, elige otro.";
                return RedirectToAction(nameof(ReprogramarCita), new { id });
            }

            // Guardar nueva fecha
            cita.Fecha = nuevaFechaHora;
            await _context.SaveChangesAsync();

            // ====== Enviar correo de reprogramación ======
            var userEmail = cita.Usuario?.Correo;
            var userName = cita.Usuario?.Nombre;

            if (!string.IsNullOrEmpty(userEmail))
            {
                string cuerpo = $@"
        <div style='font-family:Arial,sans-serif; color:#333;'>
            <h2>Reprogramación de Cita</h2>
            <p>Estimado/a <strong>{userName}</strong>,</p>
            <p>Tu cita ha sido reprogramada con éxito. Aquí tienes los nuevos detalles:</p>
            <ul>
                <li><b>Especialidad:</b> {cita.Servicio?.Nombre}</li>
                <li><b>Doctor:</b> {cita.StaffMedico?.Nombre} {cita.StaffMedico?.Apellido}</li>
                <li><b>Nueva Fecha:</b> {cita.Fecha:dd/MM/yyyy}</li>
                <li><b>Nueva Hora:</b> {cita.Fecha:HH:mm}</li>
            </ul>
            <p>Gracias por confiar en <strong>Odontobrass</strong>.</p>
        </div>";

                await _emailService.EnviarCorreo(
                    userEmail,
                    "Tu cita ha sido reprogramada - Odontobrass",
                    cuerpo
                );
            }
            // =============================================

            TempData["Success"] = "La cita fue reprogramada y se notificó por correo.";
            return RedirectToAction(nameof(cronogramaCitas));
        }

        // ---------- helpers de autorización ----------
        private async Task<(bool ok, int rol, int? userId, int? doctorId)> GetPermisoSobreCita(int idCita)
        {
            var rol = HttpContext.Session.GetInt32("UserRol") ?? 0;
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName");

            int? doctorId = null;
            if (rol == 2 && !string.IsNullOrEmpty(userName))
            {
                var doc = await _context.StaffMedico
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => (m.Nombre + " " + m.Apellido).ToLower() == userName.ToLower());
                doctorId = doc?.IdStaffMedico;
            }
            var logueado = (userId != null && rol > 0);
            return (logueado, rol, userId, doctorId);
        }

        private static bool PuedeOperar(int rol, int? userId, int? doctorId, Cita c)
        {
            if (rol == 1) return c.IdUsuario == userId;
            if (rol == 2) return doctorId != null && c.IdStaffMedico == doctorId.Value;
            return false;
        }

        private async Task<string> ObtenerNombreDoctor(int idMedico)
        {
            var medico = await _context.StaffMedico
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdStaffMedico == idMedico);

            return medico != null ? $"{medico.Nombre} {medico.Apellido}" : "No identificado";
        }

        // GET: Cliente/ComentariosCita/5
        public async Task<IActionResult> ComentariosCita(int id)
        {
            var cita = await _context.Citas
                .Include(c => c.Usuario)
                .Include(c => c.StaffMedico)
                .Include(c => c.Servicio)
                .FirstOrDefaultAsync(c => c.IdCita == id);

            if (cita == null) return NotFound();

            ViewBag.EsMedico = HttpContext.Session.GetInt32("UserRol") == 2; // 2 = Médico (ajusta según tu sistema)
            return View(cita);
        }

        // POST: Cliente/GuardarComentario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarComentario(int id, string comentarios)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null) return NotFound();

            cita.Comentarios = comentarios;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Comentario guardado correctamente.";
            return RedirectToAction(nameof(cronogramaCitas));
        }




    }
}
