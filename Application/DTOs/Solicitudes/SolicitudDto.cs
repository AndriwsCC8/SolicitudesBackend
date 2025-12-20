namespace Application.DTOs.Solicitudes
{
    public class SolicitudDto
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string Asunto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Prioridad { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string TipoSolicitud { get; set; } = string.Empty;
        public string Solicitante { get; set; } = string.Empty;
        public string? GestorAsignado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaCierre { get; set; }
    }
}
