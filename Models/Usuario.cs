using System.ComponentModel.DataAnnotations;

namespace ProyectoDBP.Models
{
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        [Required, MaxLength(100)]
        public string Nombre { get; set; }

        [Required, EmailAddress]
        public string Correo { get; set; }

        [Required]
        public string Contraseña { get; set; }

        public string Dni { get; set; }

        public DateTime FechaNacimiento { get; set; }

        public int Edad { get; set; }

        public string Sexo { get; set; }

        public string? Ocupacion { get; set; }
        public string? Direccion { get; set; }

        public string? Telefono { get; set; }
        public string? TelefonoEmergencia { get; set; }

        public string? Alergias { get; set; }
        public string? Antecedentes { get; set; }
        public string? AntecedentesFam { get; set; }

        public int Rol { get; set; } = 1; // 1 = paciente, 2 = staff médico

        // Relación: un usuario puede tener muchas citas
        public virtual ICollection<Cita> Citas { get; set; }
    }
}
