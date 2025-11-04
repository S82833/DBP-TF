using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoDBP.Datos;
using ProyectoDBP.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProyectoDBP.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDBContext _context;
        public AccountController(ApplicationDBContext context) => _context = context;

        // ===============================
        // LOGIN
        // ===============================

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] string Email, [FromForm] string Password, string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ModelState.AddModelError(string.Empty, "Ingrese correo y contraseña.");
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            // Validación simple 
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == Email && u.Contraseña == Password);

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas.");
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            // GUARDAR SESIÓN COMPLETA
            HttpContext.Session.SetInt32("UserId", usuario.IdUsuario);
            HttpContext.Session.SetString("UserName", usuario.Nombre);   // o $"{usuario.Nombre} {usuario.Apellido}"
            HttpContext.Session.SetString("UserEmail", usuario.Correo);
            HttpContext.Session.SetInt32("UserRol", usuario.Rol);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Inicio");
        }


        // ===============================
        // REGISTRO
        // ===============================

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string DNI, string FullName, string Email, string Password,
                                                 DateTime FechaNacimiento, string Sexo)
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(DNI) || DNI.Length != 8)
            {
                ViewBag.Error = "El DNI debe tener 8 dígitos.";
                return View();
            }
            if (_context.Usuarios.Any(u => u.Correo == Email))
            {
                ViewBag.Error = "Ya existe una cuenta con este correo.";
                return View();
            }
            if (_context.Usuarios.Any(u => u.Dni == DNI))
            {
                ViewBag.Error = "Ya existe un usuario con este DNI.";
                return View();
            }

            // Calcular edad
            var hoy = DateTime.Today;
            var edad = hoy.Year - FechaNacimiento.Year;
            if (FechaNacimiento.Date > hoy.AddYears(-edad)) edad--;

            var nuevoUsuario = new Usuario
            {
                Dni = DNI,
                Nombre = FullName,
                Correo = Email,
                Contraseña = Password,      
                FechaNacimiento = FechaNacimiento,
                Sexo = Sexo,                // "M", "F" u "O"
                Edad = edad,
                Rol = 1,                    // Paciente por defecto
                Alergias = "",
                Antecedentes = "",
                AntecedentesFam = ""
            };

            _context.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            ViewBag.Message = "Cuenta creada exitosamente. Ahora puedes iniciar sesión.";
            return RedirectToAction("Login");
        }

        // ===============================
        // LOGOUT
        // ===============================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }


    }
}

