using Application.DTOs.Solicitudes;

namespace Application.Interfaces
{
    public interface ISolicitudService
    {
        // Consultas
        Task<SolicitudDto> CrearAsync(CrearSolicitudDto dto, int usuarioId);
        Task<SolicitudDto?> ObtenerPorIdAsync(int solicitudId, int usuarioId);
        Task<IEnumerable<SolicitudDto>> ObtenerPorUsuarioAsync(int usuarioId);
        Task<IEnumerable<SolicitudDto>> ObtenerPorAreaAsync(int areaId);
        Task<IEnumerable<SolicitudDto>> ObtenerTodasAsync();
        
        // Workflow
        Task<SolicitudDto> AsignarAgenteAsync(AsignarAgenteDto dto, int adminId);
        Task<SolicitudDto> CambiarEstadoAsync(CambiarEstadoDto dto, int agenteId);
        Task<SolicitudDto> RechazarAsync(RechazarSolicitudDto dto, int agenteId);
        Task<SolicitudDto> CerrarAsync(CerrarSolicitudDto dto, int usuarioId);
    }
}
