using Application.DTOs.Historial;

namespace Application.Interfaces
{
    public interface IHistorialService
    {
        Task<List<HistorialEstadoDto>> ObtenerHistorialAsync(int solicitudId, int usuarioId, string rol, int? areaId);
    }
}
