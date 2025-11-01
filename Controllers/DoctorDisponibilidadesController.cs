using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProyectoDBP.Datos;
using ProyectoDBP.Models;
using ProyectoDBP.ViewModels;

namespace ProyectoDBP.Controllers
{
    public class DoctorDisponibilidadesController : Controller
    {
        private readonly ApplicationDBContext _context;
        private static readonly Dictionary<string, int> DiaOrden = new()
        {
            ["Lunes"] = 1,
            ["Martes"] = 2,
            ["Miércoles"] = 3,
            ["Miercoles"] = 3,
            ["Jueves"] = 4,
            ["Viernes"] = 5,
            ["Sábado"] = 6,
            ["Sabado"] = 6,
            ["Domingo"] = 7
        };

        public DoctorDisponibilidadesController(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var medicos = await _context.StaffMedico
                .Include(m => m.Disponibilidades)
                .AsNoTracking()
                .OrderBy(m => m.Apellido)
                .ThenBy(m => m.Nombre)
                .Select(m => new DoctorDisponibilidadResumenViewModel
                {
                    IdStaffMedico = m.IdStaffMedico,
                    NombreCompleto = m.Nombre + " " + m.Apellido,
                    Especialidad = m.Especialidad,
                    DisponibilidadesRegistradas = m.Disponibilidades.Count
                })
                .ToListAsync();

            return View(medicos);
        }

        public async Task<IActionResult> MisHorarios()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var nombreSesion = HttpContext.Session.GetString("UserName");
            if (!string.IsNullOrEmpty(nombreSesion))
            {
                var medico = await _context.StaffMedico
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m =>
                        (m.Nombre + " " + m.Apellido).ToLower()
                        == (nombreSesion ?? string.Empty).ToLower());

                if (medico != null)
                {
                    return RedirectToAction(nameof(Gestionar), new { idStaffMedico = medico.IdStaffMedico });
                }
            }

            TempData["Error"] = "No encontramos un registro en el staff asociado a tu usuario. Selecciona tu perfil manualmente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Gestionar(int idStaffMedico)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var medicoIdSesion = await GetMedicoIdFromSessionAsync();
            if (medicoIdSesion == null)
            {
                TempData["Error"] = "No se encontró tu perfil de médico asociado.";
                return RedirectToAction(nameof(Index));
            }

            if (medicoIdSesion != idStaffMedico)
            {
                TempData["Error"] = "No tienes permiso para modificar los horarios de otro médico.";
                return RedirectToAction(nameof(Gestionar), new { idStaffMedico = medicoIdSesion.Value });
            }

            var viewModel = await ConstruirGestionViewModel(idStaffMedico);
            if (viewModel == null) return NotFound();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(DoctorDisponibilidadFormModel form)
        {
            // 1) Debe estar logueado como médico
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            // 2) Validar que el médico logueado solo edite su propio perfil
            var medicoIdSesion = await GetMedicoIdFromSessionAsync();
            if (medicoIdSesion == null || medicoIdSesion != form.IdStaffMedico)
            {
                TempData["Error"] = "No puedes registrar disponibilidad para otro médico.";
                return RedirectToAction(nameof(Index));
            }

            // 3) Validaciones de modelo
            if (!ModelState.IsValid)
            {
                var vmInvalido = await ConstruirGestionViewModel(form.IdStaffMedico, form);
                if (vmInvalido == null) return NotFound();
                return View("Gestionar", vmInvalido);
            }

            // 4) Verificar existencia del médico
            var existeMedico = await _context.StaffMedico
                .AsNoTracking()
                .AnyAsync(m => m.IdStaffMedico == form.IdStaffMedico);

            if (!existeMedico)
            {
                ModelState.AddModelError(string.Empty, "El médico seleccionado no existe.");
                var vmNotFound = await ConstruirGestionViewModel(form.IdStaffMedico, form);
                if (vmNotFound == null) return NotFound();
                return View("Gestionar", vmNotFound);
            }

            // 5) Validar horas (formato, orden e intervalos de 30’)
            if (!TimeSpan.TryParse(form.HoraInicio, out var hi) ||
                !TimeSpan.TryParse(form.HoraFin, out var hf) ||
                hi >= hf ||
                (hf - hi).TotalMinutes % 30 != 0 ||
                hi.Minutes % 30 != 0 || hf.Minutes % 30 != 0)
            {
                ModelState.AddModelError(string.Empty, "Rango de horas inválido (intervalos de 30 min, inicio < fin).");
                var vmHoras = await ConstruirGestionViewModel(form.IdStaffMedico, form);
                if (vmHoras == null) return NotFound();
                return View("Gestionar", vmHoras);
            }

            // 6) Validar traslape (primero a memoria -> luego Any() con cálculo en C#)
            var disponibilidades = await _context.DoctorDisponibilidades
                .Where(d => d.IdStaffMedico == form.IdStaffMedico && d.DiaSemana == form.DiaSemana)
                .AsNoTracking()
                .ToListAsync();

            var existeTraslape = disponibilidades.Any(d =>
            {
                var ie = TimeSpan.Parse(d.HoraInicio);
                var fe = TimeSpan.Parse(d.HoraFin);
                return hi < fe && ie < hf; // traslape si se intersectan abiertos
            });

            if (existeTraslape)
            {
                ModelState.AddModelError(string.Empty, "Ya existe una disponibilidad que se superpone con el horario elegido.");
                var vmTraslape = await ConstruirGestionViewModel(form.IdStaffMedico, form);
                if (vmTraslape == null) return NotFound();
                return View("Gestionar", vmTraslape);
            }

            // 7) Guardar
            var nuevaDisponibilidad = new DoctorDisponibilidad
            {
                IdStaffMedico = form.IdStaffMedico,
                DiaSemana = form.DiaSemana,
                HoraInicio = hi.ToString(@"hh\:mm"),
                HoraFin = hf.ToString(@"hh\:mm")
            };

            _context.DoctorDisponibilidades.Add(nuevaDisponibilidad);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Disponibilidad registrada correctamente.";
            return RedirectToAction(nameof(Gestionar), new { idStaffMedico = form.IdStaffMedico });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            var disponibilidad = await _context.DoctorDisponibilidades.FindAsync(id);
            if (disponibilidad == null) return NotFound();

            var medicoIdSesion = await GetMedicoIdFromSessionAsync();
            if (medicoIdSesion == null || medicoIdSesion != disponibilidad.IdStaffMedico)
            {
                TempData["Error"] = "No puedes eliminar la disponibilidad de otro médico.";
                return RedirectToAction(nameof(Index));
            }

            var idStaff = disponibilidad.IdStaffMedico;
            _context.DoctorDisponibilidades.Remove(disponibilidad);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Disponibilidad eliminada.";
            return RedirectToAction(nameof(Gestionar), new { idStaffMedico = idStaff });
        }



        private IActionResult? ValidarAcceso()
        {
            var correo = HttpContext.Session.GetString("UserEmail");
            var rol = HttpContext.Session.GetInt32("UserRol");
            if (string.IsNullOrEmpty(correo) || rol != 2)
            {
                var returnUrl = Url.Action(nameof(Index), "DoctorDisponibilidades");
                return RedirectToAction("Login", "Account", new { returnUrl });
            }

            return null;
        }

        private async Task<DoctorDisponibilidadGestionViewModel?> ConstruirGestionViewModel(int idStaffMedico, DoctorDisponibilidadFormModel? form = null)
        {
            var doctor = await _context.StaffMedico
                .Include(m => m.Disponibilidades)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdStaffMedico == idStaffMedico);

            if (doctor == null) return null;

            var disponibilidadesOrdenadas = doctor.Disponibilidades
                .OrderBy(d => ObtenerOrdenDia(d.DiaSemana))
                .ThenBy(d => ObtenerHora(d.HoraInicio))
                .ToList();

            var modelo = new DoctorDisponibilidadGestionViewModel
            {
                IdStaffMedico = doctor.IdStaffMedico,
                DoctorNombre = doctor.Nombre + " " + doctor.Apellido,
                Disponibilidades = disponibilidadesOrdenadas,
                Form = form ?? new DoctorDisponibilidadFormModel
                {
                    IdStaffMedico = doctor.IdStaffMedico
                }
            };

            modelo.DiasSemana = ObtenerDias(modelo.Form.DiaSemana);
            modelo.HorariosInicio = ObtenerHorarios(true, modelo.Form.HoraInicio);
            modelo.HorariosFin = ObtenerHorarios(false, modelo.Form.HoraFin);

            return modelo;
        }

        private static IEnumerable<SelectListItem> ObtenerDias(string? seleccionado)
        {
            var dias = new[] { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };
            return dias.Select(d => new SelectListItem
            {
                Value = d,
                Text = d,
                Selected = string.Equals(d, seleccionado, StringComparison.InvariantCultureIgnoreCase)
            });
        }

        private static IEnumerable<SelectListItem> ObtenerHorarios(bool esInicio, string? seleccionado)
        {
            var items = new List<SelectListItem>();
            var horaInicio = new TimeSpan(7, 0, 0);
            var horaFin = new TimeSpan(21, 0, 0);
            var iteracion = esInicio ? horaInicio : horaInicio.Add(TimeSpan.FromMinutes(30));
            var limite = esInicio ? horaFin : horaFin;

            while (iteracion <= limite)
            {
                if (esInicio && iteracion == horaFin)
                {
                    break;
                }

                var valor = iteracion.ToString(@"hh\:mm");
                items.Add(new SelectListItem
                {
                    Value = valor,
                    Text = valor,
                    Selected = valor == seleccionado
                });

                iteracion = iteracion.Add(TimeSpan.FromMinutes(30));
            }

            return items;
        }

        private static int ObtenerOrdenDia(string? dia)
        {
            if (string.IsNullOrEmpty(dia)) return int.MaxValue;
            return DiaOrden.TryGetValue(dia, out var orden) ? orden : int.MaxValue;
        }

        private static TimeSpan ObtenerHora(string? hora)
        {
            return TimeSpan.TryParse(hora, out var resultado) ? resultado : TimeSpan.MaxValue;
        }
        private async Task<int?> GetMedicoIdFromSessionAsync()
        {
            var nombreSesion = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrWhiteSpace(nombreSesion)) return null;

            var medico = await _context.StaffMedico
                .AsNoTracking()
                .FirstOrDefaultAsync(m =>
                    (m.Nombre + " " + m.Apellido).ToLower() ==
                    nombreSesion.ToLower());

            return medico?.IdStaffMedico;
        }

    }
}
