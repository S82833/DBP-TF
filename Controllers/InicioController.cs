using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoDBP.Datos;

namespace ProyectoDBP.Controllers
{
    public class InicioController : Controller
    {
        private readonly ApplicationDBContext _context;
        public InicioController(ApplicationDBContext context) => _context = context;

        public IActionResult Index()
        {
            var medicos = _context.StaffMedico.AsNoTracking().ToList();
            ViewBag.Medicos = medicos;
            return View();
        }
    }
}
