using Application.DTOs.Admin;
using Application.DTOs.Solicitudes;

namespace Application.Interfaces
{
    public interface IAdminService
    {
        // Usuarios
        Task<List<UsuarioAdminDto>> ObtenerUsuariosAsync();
        Task<List<UsuarioAdminDto>> ObtenerAgentesAsync();
        Task<UsuarioAdminDto?> ObtenerUsuarioPorIdAsync(int id);
        Task<UsuarioAdminDto> CrearUsuarioAsync(CrearUsuarioDto dto);
        Task<UsuarioAdminDto> ActualizarUsuarioAsync(int id, ActualizarUsuarioDto dto);
        Task<bool> EliminarUsuarioAsync(int id, bool force = false);

        // Áreas
        Task<List<AreaAdminDto>> ObtenerAreasAsync();
        Task<AreaAdminDto?> ObtenerAreaPorIdAsync(int id);
        Task<AreaAdminDto> CrearAreaAsync(CrearAreaDto dto);
        Task<AreaAdminDto> ActualizarAreaAsync(int id, ActualizarAreaDto dto);
        Task<bool> EliminarAreaAsync(int id);

        // Tipos de Solicitud (Categorías)
        Task<List<TipoSolicitudAdminDto>> ObtenerTiposSolicitudAsync();
        Task<TipoSolicitudAdminDto?> ObtenerTipoSolicitudPorIdAsync(int id);
        Task<TipoSolicitudAdminDto> CrearTipoSolicitudAsync(CrearTipoSolicitudDto dto);
        Task<TipoSolicitudAdminDto> ActualizarTipoSolicitudAsync(int id, ActualizarTipoSolicitudDto dto);
        Task<bool> EliminarTipoSolicitudAsync(int id);

        // Solicitudes sin asignar
        Task<List<SolicitudDto>> ObtenerSolicitudesSinAsignarAsync();
        Task<List<SolicitudDto>> ObtenerSolicitudesTipoOtroAsync();

        // Reportes
        Task<ReporteResumenDto> ObtenerReporteResumenAsync();
        Task<List<ReportePorAreaDto>> ObtenerReportePorAreaAsync();
        Task<List<ReporteDesempenoAgenteDto>> ObtenerReporteDesempenoAgentesAsync();
        Task<ReporteTiemposRespuestaDto> ObtenerReporteTiemposRespuestaAsync();
    }
}
