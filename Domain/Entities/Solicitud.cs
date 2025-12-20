using Domain.Enums;

namespace Domain.Entities
{
    public class Solicitud
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty; // SOL-YYYY-0001
        public string Asunto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public PrioridadEnum Prioridad { get; set; }
        public EstadoSolicitudEnum Estado { get; set; } = EstadoSolicitudEnum.Nueva;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaCierre { get; set; }
        
        // Adjunto
        public string? ArchivoNombre { get; set; }
        public string? ArchivoRuta { get; set; }
        public string? ArchivoContentType { get; set; }

        // Relaciones
        public int AreaId { get; set; }
        public Area Area { get; set; } = null!;

        public int TipoSolicitudId { get; set; }
        public TipoSolicitud TipoSolicitud { get; set; } = null!;

        public int SolicitanteId { get; set; }
        public Usuario Solicitante { get; set; } = null!;

        public int? GestorAsignadoId { get; set; }
        public Usuario? GestorAsignado { get; set; }

        public string? MotivoRechazo { get; set; }

        // Colecciones
        public ICollection<Comentario> Comentarios { get; set; } = new List<Comentario>();
        public ICollection<HistorialEstado> HistorialEstados { get; set; } = new List<HistorialEstado>();
    }
}