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
        
        // Información de Área con ID
        public int AreaId { get; set; }
        public string Area { get; set; } = string.Empty;
        
        // Información de Tipo de Solicitud con ID
        public int TipoSolicitudId { get; set; }
        public string TipoSolicitud { get; set; } = string.Empty;
        
        // Información del Solicitante con ID y email
        public int SolicitanteId { get; set; }
        public string Solicitante { get; set; } = string.Empty;
        public string SolicitanteEmail { get; set; } = string.Empty;
        
        // Información del Gestor Asignado (opcional)
        public int? GestorAsignadoId { get; set; }
        public string? GestorAsignado { get; set; }
        public string? GestorAsignadoEmail { get; set; }
        
        // Fechas
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaCierre { get; set; }
        
        // Campos adicionales
        public string? MotivoRechazo { get; set; }
        
        // Comentarios
        public List<Application.DTOs.Comentarios.ComentarioDto> Comentarios { get; set; } = new();
    }
}
