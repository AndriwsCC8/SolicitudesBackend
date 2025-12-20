namespace Domain.Entities
{
    public class TipoSolicitud
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int AreaId { get; set; }
        public Area Area { get; set; } = null!;
        public bool Activo { get; set; } = true;

        // Relaciones
        public ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();
    }
}