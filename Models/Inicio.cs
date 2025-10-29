namespace ProyectoDBP.Models
{
    public class Inicio
    {
        public string Titulo { get; set; }
        public string Resumen { get; set; }
        public string Direccion { get; set; } 
        public string Telefono { get; set; }
        public string MapaLink { get; set; }

        public List<ServicioItem> Servicios { get; set; } = new();
    }

    
    public class ServicioItem
    {
        public string? Titulo { get; set; } 
        public string? Descripcion { get; set; } 
    }
}
