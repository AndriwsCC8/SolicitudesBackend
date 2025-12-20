using Application.DTOs.Usuarios;

namespace Application.Interfaces
{
    public interface IUsuarioService
    {
        Task<List<UsuarioDto>> ObtenerTodosAsync();
        Task<UsuarioDto> ObtenerPorIdAsync(int id);
        Task CrearAsync(CrearUsuarioDto dto);
        Task ActualizarAsync(int id, ActualizarUsuarioDto dto);
        Task ResetPasswordAsync(int id, ResetPasswordDto dto);
    }
}
