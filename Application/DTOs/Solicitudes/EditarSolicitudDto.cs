using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Solicitudes
{
    public class EditarSolicitudDto
    {
        public string Asunto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Prioridad { get; set; }  // 1=Baja, 2=Media, 3=Alta
        public IFormFile? Archivo { get; set; }
        public bool EliminarArchivo { get; set; } = false;  // Para eliminar archivo existente
    }
}
