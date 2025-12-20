namespace Domain.Entities
{
    public class Area
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; } = true;

        // Relaciones
        public ICollection<TipoSolicitud> TiposSolicitud { get; set; } = new List<TipoSolicitud>();
        public ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}