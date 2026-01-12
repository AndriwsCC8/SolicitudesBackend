namespace Application.DTOs.Comentarios
{
    public class ComentarioDto
    {
        public int Id { get; set; }
        public string Contenido { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }

        // Mantener campos individuales para compatibilidad
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public int? UsuarioRol { get; set; }
        public string? UsuarioRolNombre { get; set; }
        public string? UsuarioDepartamento { get; set; }

        // Nuevo campo con objeto completo del usuario
        public UsuarioComentarioDto? Usuario { get; set; }

        // Campos para identificar comentarios del sistema
        public bool EsSistema { get; set; }
        public string? TipoEvento { get; set; }
    }
}
