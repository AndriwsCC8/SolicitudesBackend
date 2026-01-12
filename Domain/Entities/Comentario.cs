using Domain.Enums;

namespace Domain.Entities
{
    public class Comentario
    {
        public int Id { get; set; }
        public string Texto { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Campos para identificar comentarios del sistema
        public bool EsSistema { get; set; } = false;
        public TipoEventoEnum? TipoEvento { get; set; }

        // Relaciones
        public int SolicitudId { get; set; }
        public Solicitud Solicitud { get; set; } = null!;

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
    }
}