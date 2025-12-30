namespace Application.DTOs.Comentarios
{
    public class ComentarioDto
    {
        public int Id { get; set; }
        public string Contenido { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public int? UsuarioRol { get; set; }
        public string? UsuarioRolNombre { get; set; }
        public string? UsuarioDepartamento { get; set; }
    }
}
