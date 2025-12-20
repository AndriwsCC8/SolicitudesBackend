namespace Application.DTOs.Usuarios
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string? Area { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
