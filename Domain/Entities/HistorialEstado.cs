using Domain.Enums;

namespace Domain.Entities
{
    public class HistorialEstado
    {
        public int Id { get; set; }
        public EstadoSolicitudEnum EstadoAnterior { get; set; }
        public EstadoSolicitudEnum EstadoNuevo { get; set; }
        public string? Observacion { get; set; }
        public DateTime FechaCambio { get; set; } = DateTime.Now;

        // Relaciones
        public int SolicitudId { get; set; }
        public Solicitud Solicitud { get; set; } = null!;

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
    }
}