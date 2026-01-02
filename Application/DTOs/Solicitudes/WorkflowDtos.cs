using System.Text.Json.Serialization;

namespace Application.DTOs.Solicitudes
{
    public class AsignarAgenteDto
    {
        public int SolicitudId { get; set; }
        
        public int AgenteId { get; set; }
        
        // Propiedad adicional para compatibilidad con frontend que envía "gestorId"
        // Esta propiedad se deserializa pero no se serializa
        [JsonPropertyName("gestorId")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int? GestorIdAlias 
        { 
            get => null; // Solo lectura
            set 
            { 
                if (value.HasValue && value.Value > 0)
                    AgenteId = value.Value;
            } 
        }
    }

    public class CambiarEstadoDto
    {
        public int SolicitudId { get; set; }
        public int NuevoEstado { get; set; }
        public string? MotivoRechazo { get; set; }
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
