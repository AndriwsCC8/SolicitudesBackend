using Domain.Enums;

namespace Application.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public RolEnum Rol { get; set; }
        public int? AreaId { get; set; }
        public string? AreaNombre { get; set; }
    }
}