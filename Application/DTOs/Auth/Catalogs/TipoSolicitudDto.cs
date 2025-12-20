namespace Application.DTOs.Catalogs
{
    public class TipoSolicitudDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int AreaId { get; set; }
        public string AreaNombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class CreateTipoSolicitudDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int AreaId { get; set; }
    }

    public class UpdateTipoSolicitudDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int AreaId { get; set; }
        public bool Activo { get; set; }
    }
}