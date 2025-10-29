using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoDBP.Datos;
using ProyectoDBP.Models;

namespace ProyectoDBP.Controllers
{
    public class ServiciosStaffController : Controller
    {
        private readonly ApplicationDBContext _context;
        public ServiciosStaffController(ApplicationDBContext context) => _context = context;

        // LISTADO STAFF
        public IActionResult StaffMedico()
        {
            // Trae todos los médicos (si no hay, devuelve lista vacía, nunca null)
            var medicos = _context.StaffMedico.AsNoTracking().ToList();
            return View(medicos);
        }

        // DETALLE DE UN MÉDICO
        public IActionResult Medico(int id)
        {
            var medico = _context.StaffMedico.AsNoTracking().FirstOrDefault(m => m.IdStaffMedico == id);
            if (medico == null) return NotFound();
            return View(medico);
        }

        // (si también tienes la vista Servicios)
        public IActionResult Servicios()
        {
            var servicios = _context.Servicios.AsNoTracking().ToList();
            return View(servicios);
        }
    }
}
