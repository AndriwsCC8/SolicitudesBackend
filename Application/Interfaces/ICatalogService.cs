using Application.DTOs.Catalogs;

namespace Application.Interfaces
{
    public interface ICatalogService
    {
        // Áreas
        Task<List<AreaDto>> GetAreasAsync();
        Task<AreaDto?> GetAreaByIdAsync(int id);
        Task<AreaDto> CreateAreaAsync(CreateAreaDto dto);
        Task<AreaDto?> UpdateAreaAsync(int id, UpdateAreaDto dto);
        Task<bool> DeleteAreaAsync(int id);

        // Tipos de Solicitud
        Task<List<TipoSolicitudDto>> GetTiposSolicitudAsync();
        Task<List<TipoSolicitudDto>> GetTiposSolicitudByAreaAsync(int areaId);
        Task<TipoSolicitudDto?> GetTipoSolicitudByIdAsync(int id);
        Task<TipoSolicitudDto> CreateTipoSolicitudAsync(CreateTipoSolicitudDto dto);
        Task<TipoSolicitudDto?> UpdateTipoSolicitudAsync(int id, UpdateTipoSolicitudDto dto);
        Task<bool> DeleteTipoSolicitudAsync(int id);

        // Prioridades
        Task<List<PrioridadDto>> GetPrioridadesAsync();
    }
}