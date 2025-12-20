using Application.DTOs.Solicitudes;

namespace Application.Interfaces
{
    public interface ISolicitudService
    {
        Task<SolicitudDto> CrearAsync(CrearSolicitudDto dto, int usuarioId);
        Task<IEnumerable<SolicitudDto>> ObtenerPorUsuarioAsync(int usuarioId);
        Task<IEnumerable<SolicitudDto>> ObtenerPorAreaAsync(int areaId);
        Task<IEnumerable<SolicitudDto>> ObtenerTodasAsync();
    }
}
