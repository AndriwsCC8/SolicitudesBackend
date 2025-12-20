using Application.DTOs.Comentarios;

namespace Application.Interfaces
{
    public interface IComentarioService
    {
        Task AgregarComentarioAsync(int solicitudId, int usuarioId, string rol, int? areaId, AgregarComentarioDto dto);
        Task<List<ComentarioDto>> ObtenerPorSolicitudAsync(int solicitudId, int usuarioId, string rol, int? areaId);
    }
}
