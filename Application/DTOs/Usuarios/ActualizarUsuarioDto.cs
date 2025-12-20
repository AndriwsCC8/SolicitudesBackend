namespace Application.DTOs.Usuarios
{
    public class ActualizarUsuarioDto
    {
        public string? Nombre { get; set; }
        public string? Email { get; set; }
        public string? Rol { get; set; }
        public int? AreaId { get; set; }
        public bool? Activo { get; set; }
    }
}
