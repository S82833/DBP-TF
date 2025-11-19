using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoDBP.Datos;

namespace ProyectoDBP.Controllers
{
    public class EstadisticasController : Controller
    {
        private readonly ApplicationDBContext _context;

        public EstadisticasController(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var acceso = ValidarAcceso();
            if (acceso != null) return acceso;

            await CargarMetricasAsync();
            await CargarGraficoAsync();

            return View();
        }

        private async Task<int?> GetMedicoIdAsync()
        {
            var nombre = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(nombre)) return null;

            var medico = await _context.StaffMedico
                .AsNoTracking()
                .FirstOrDefaultAsync(m =>
                    (m.Nombre + " " + m.Apellido).ToLower() == nombre.ToLower());

            return medico?.IdStaffMedico;
        }


        private async Task CargarMetricasAsync()
        {
            var doctorId = await GetMedicoIdAsync();
            if (doctorId == null) return;

            var hoy = DateTime.Now;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

            // Total de pacientes atendidos por el doctor
            ViewBag.TotalPacientes = await _context.Citas
                .Where(c => c.IdStaffMedico == doctorId)
                .Select(c => c.IdUsuario)
                .Distinct()
                .CountAsync();

            // Total de citas del mes (solo del doctor)
            ViewBag.TotalCitasMes = await _context.Citas
                .Where(c => c.IdStaffMedico == doctorId && c.Fecha >= inicioMes)
                .CountAsync();

            // Servicio más solicitado (global)
            var servicio = await _context.Citas
                .Include(c => c.Servicio)
                .GroupBy(c => c.Servicio.Nombre)
                .Select(g => new { Nombre = g.Key, Cantidad = g.Count() })
                .OrderByDescending(g => g.Cantidad)
                .FirstOrDefaultAsync();

            ViewBag.ServicioPopular = servicio?.Nombre ?? "N/D";

            // Doctor con más citas (global)
            var doctor = await _context.Citas
                .Include(c => c.StaffMedico)
                .GroupBy(c => c.StaffMedico.Nombre + " " + c.StaffMedico.Apellido)
                .Select(g => new { Nombre = g.Key, Cantidad = g.Count() })
                .OrderByDescending(g => g.Cantidad)
                .FirstOrDefaultAsync();

            ViewBag.DoctorActivo = doctor?.Nombre ?? "N/D";
        }


        private async Task CargarGraficoAsync()
        {
            var doctorId = await GetMedicoIdAsync();
            if (doctorId == null) return;

            var hoy = DateTime.Now.Date;
            var inicio = hoy.AddDays(-30);

            var data = await _context.Citas
                .Where(c => c.IdStaffMedico == doctorId &&
                            c.Fecha.Date >= inicio &&
                            c.Fecha.Date <= hoy)
                .GroupBy(c => c.Fecha.Date)
                .Select(g => new { Fecha = g.Key, Cantidad = g.Count() })
                .OrderBy(g => g.Fecha)
                .ToListAsync();

            ViewBag.Fechas = data.Select(d => d.Fecha.ToString("dd/MM")).ToList();
            ViewBag.Cantidades = data.Select(d => d.Cantidad).ToList();
        }



        private IActionResult? ValidarAcceso()
        {
            var correo = HttpContext.Session.GetString("UserEmail");
            var rol = HttpContext.Session.GetInt32("UserRol");

            // 1 = Admin, 2 = Médico
            if (string.IsNullOrEmpty(correo) || (rol != 2))
            {
                var returnUrl = Url.Action(nameof(Index), "Estadisticas");
                return RedirectToAction("Login", "Account", new { returnUrl });
            }

            return null;
        }
    }
}
