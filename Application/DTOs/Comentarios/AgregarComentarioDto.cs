namespace Application.DTOs.Comentarios
{
    public class AgregarComentarioDto
    {
        public int SolicitudId { get; set; }
        public string Contenido { get; set; } = string.Empty;
    }
}
