namespace Application.DTOs.Admin
{
    public class TipoSolicitudAdminDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int? AreaId { get; set; }
        public string? AreaNombre { get; set; }
        public bool Activo { get; set; }
        public int CantidadSolicitudes { get; set; }
    }

    public class CrearTipoSolicitudDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int? AreaId { get; set; }
    }

    public class ActualizarTipoSolicitudDto
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int? AreaId { get; set; }
        public bool? Activo { get; set; }
    }
}
