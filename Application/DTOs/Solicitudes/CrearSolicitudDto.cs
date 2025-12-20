namespace Application.DTOs.Solicitudes
{
    public class CrearSolicitudDto
    {
        public int TipoSolicitudId { get; set; }
        public string Asunto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Prioridad { get; set; }
    }
}
