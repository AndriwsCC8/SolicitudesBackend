using Application.DTOs.Auth;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
        string GenerateJwtToken(int userId, string nombreUsuario, string rol, int? areaId);
    }
}