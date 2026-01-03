namespace Application.DTOs.Admin
{
    public class UsuarioAdminDto
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Rol { get; set; }
        public string RolNombre { get; set; } = string.Empty;
        public int? AreaId { get; set; }
        public string? AreaNombre { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class CrearUsuarioDto
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Rol { get; set; }
        public int? AreaId { get; set; }
        public bool? Activo { get; set; }
    }

    public class ActualizarUsuarioDto
    {
        public string? NombreUsuario { get; set; }
        public string? Nombre { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public int? Rol { get; set; }
        public int? AreaId { get; set; }
        public bool? Activo { get; set; }
    }
}
