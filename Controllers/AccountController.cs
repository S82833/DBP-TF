using Microsoft.AspNetCore.Mvc;
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

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string Email, string Password, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ViewBag.Error = "Debes ingresar ambos campos.";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
            var user = _context.Usuarios
                .FirstOrDefault(u => u.Correo == Email && u.Contraseña == Password);

            if (user == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos.";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            // Sesión
            HttpContext.Session.SetString("UserEmail", user.Correo);
            HttpContext.Session.SetString("UserName", user.Nombre);
            HttpContext.Session.SetInt32("UserRol", user.Rol);

            // Respeta returnUrl si vino de “Reservar cita”
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

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}

