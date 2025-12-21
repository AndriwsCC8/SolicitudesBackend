namespace Application.DTOs.Solicitudes
{
    public class AsignarAgenteDto
    {
        public int SolicitudId { get; set; }
        public int AgenteId { get; set; }
    }

    public class CambiarEstadoDto
    {
        public int SolicitudId { get; set; }
        public int NuevoEstado { get; set; }
        public string? Observacion { get; set; }
    }

    public class RechazarSolicitudDto
    {
        public int SolicitudId { get; set; }
        public string MotivoRechazo { get; set; } = string.Empty;
    }

    public class CerrarSolicitudDto
    {
        public int SolicitudId { get; set; }
        public string? Observacion { get; set; }
    }
}
