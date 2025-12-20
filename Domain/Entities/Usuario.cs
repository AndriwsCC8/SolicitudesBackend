using Domain.Enums;

namespace Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public RolEnum Rol { get; set; }
        public int? AreaId { get; set; }
        public Area? Area { get; set; }
        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relaciones
        public ICollection<Solicitud> SolicitudesCreadas { get; set; } = new List<Solicitud>();
        public ICollection<Solicitud> SolicitudesAsignadas { get; set; } = new List<Solicitud>();
        public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
    }
}