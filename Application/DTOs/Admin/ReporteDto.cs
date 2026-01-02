namespace Application.DTOs.Admin
{
    public class ReporteResumenDto
    {
        public int TotalSolicitudes { get; set; }
        public int SolicitudesNuevas { get; set; }
        public int SolicitudesEnProceso { get; set; }
        public int SolicitudesResueltas { get; set; }
        public int SolicitudesCerradas { get; set; }
        public int SolicitudesRechazadas { get; set; }
        public int SolicitudesCanceladas { get; set; }
        public double TiempoPromedioResolucion { get; set; } // En horas
        public int UsuariosActivos { get; set; }
        public int TotalAreas { get; set; }
    }

    public class ReportePorAreaDto
    {
        public int AreaId { get; set; }
        public string AreaNombre { get; set; } = string.Empty;
        public int TotalSolicitudes { get; set; }
        public int SolicitudesAbiertas { get; set; }
        public int SolicitudesResueltas { get; set; }
        public int CantidadAgentes { get; set; }
        public double TiempoPromedioResolucion { get; set; }
    }

    public class ReporteDesempenoAgenteDto
    {
        public int AgenteId { get; set; }
        public string AgenteNombre { get; set; } = string.Empty;
        public string? AreaNombre { get; set; }
        public int SolicitudesAsignadas { get; set; }
        public int SolicitudesResueltas { get; set; }
        public int SolicitudesEnProceso { get; set; }
        public double TasaResolucion { get; set; }
        public double TiempoPromedioResolucion { get; set; }
    }

    public class ReporteTiemposRespuestaDto
    {
        public double TiempoPromedioTotal { get; set; }
        public double TiempoPromedioNuevaAEnProceso { get; set; }
        public double TiempoPromedioEnProcesoAResuelta { get; set; }
        public double TiempoMinimoResolucion { get; set; }
        public double TiempoMaximoResolucion { get; set; }
        public int SolicitudesFueraDeSLA { get; set; } // Más de 72 horas
    }
}
