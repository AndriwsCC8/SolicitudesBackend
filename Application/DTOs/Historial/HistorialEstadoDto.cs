namespace Application.DTOs.Historial
{
    public class HistorialEstadoDto
    {
        public string EstadoAnterior { get; set; } = string.Empty;
        public string EstadoNuevo { get; set; } = string.Empty;
        public string? Observacion { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }
}
