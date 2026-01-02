namespace Application.DTOs.Admin
{
    public class AreaAdminDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public int CantidadAgentes { get; set; }
        public int CantidadSolicitudes { get; set; }
    }

    public class CrearAreaDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }

    public class ActualizarAreaDto
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public bool? Activo { get; set; }
    }
}
