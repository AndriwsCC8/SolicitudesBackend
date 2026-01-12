using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace Infrastructure.Services
{
    public class ReporteService : IReporteService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReporteService> _logger;

        public ReporteService(ApplicationDbContext context, ILogger<ReporteService> logger)
        {
            _context = context;
            _logger = logger;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<byte[]> GenerarReporteExcelAsync()
        {
            try
            {
                using (var package = new ExcelPackage())
                {
                    // Hoja 1: Resumen General
                    await CrearHojaResumenGeneral(package);

                    // Hoja 2: Por Gestor
                    await CrearHojaPorGestor(package);

                    // Hoja 3: Por Área
                    await CrearHojaPorArea(package);

                    // Hoja 4: Por Estado
                    await CrearHojaPorEstado(package);

                    // Hoja 5: Por Prioridad
                    await CrearHojaPorPrioridad(package);

                    // Hoja 6: Por Tipo
                    await CrearHojaPorTipo(package);

                    // Hoja 7: Detalle Completo
                    await CrearHojaDetalleCompleto(package);

                    // Hoja 8: Historial Sistema
                    await CrearHojaHistorialSistema(package);

                    return package.GetAsByteArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reporte Excel");
                throw;
            }
        }

        private async Task CrearHojaResumenGeneral(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets.Add("Resumen General");

            // Cabeceras
            worksheet.Cells["A1"].Value = "Métrica";
            worksheet.Cells["B1"].Value = "Valor";
            FormatearCabecera(worksheet.Cells["A1:B1"]);

            // Obtener métricas
            var totalSolicitudes = await _context.Solicitudes.CountAsync();
            var nuevas = await _context.Solicitudes.CountAsync(s => s.Estado == EstadoSolicitudEnum.Nueva);
            var enProceso = await _context.Solicitudes.CountAsync(s => s.Estado == EstadoSolicitudEnum.EnProceso);
            var resueltas = await _context.Solicitudes.CountAsync(s => s.Estado == EstadoSolicitudEnum.Resuelta);
            var cerradas = await _context.Solicitudes.CountAsync(s => s.Estado == EstadoSolicitudEnum.Cerrada);
            var rechazadas = await _context.Solicitudes.CountAsync(s => s.Estado == EstadoSolicitudEnum.Rechazada);
            var canceladas = await _context.Solicitudes.CountAsync(s => s.Estado == EstadoSolicitudEnum.Cancelada);
            var totalUsuarios = await _context.Usuarios.CountAsync(u => u.Activo);
            var totalAgentes = await _context.Usuarios.CountAsync(u => u.Activo && u.Rol == RolEnum.AgenteArea);
            var totalAreas = await _context.Areas.CountAsync(a => a.Activo);

            // Llenar datos
            int row = 2;
            worksheet.Cells[$"A{row}"].Value = "Total de Solicitudes";
            worksheet.Cells[$"B{row}"].Value = totalSolicitudes;
            row++;

            worksheet.Cells[$"A{row}"].Value = "Solicitudes Nuevas";
            worksheet.Cells[$"B{row}"].Value = nuevas;
            row++;

            worksheet.Cells[$"A{row}"].Value = "Solicitudes En Proceso";
            worksheet.Cells[$"B{row}"].Value = enProceso;
            row++;

            worksheet.Cells[$"A{row}"].Value = "Solicitudes Resueltas";
            worksheet.Cells[$"B{row}"].Value = resueltas;
            row++;

            worksheet.Cells[$"A{row}"].Value = "Solicitudes Cerradas";
            worksheet.Cells[$"B{row}"].Value = cerradas;
            row++;

            worksheet.Cells[$"A{row}"].Value = "Solicitudes Rechazadas";
            worksheet.Cells[$"B{row}"].Value = rechazadas;
            row++;

            worksheet.Cells[$"A{row}"].Value = "Solicitudes Canceladas";
            worksheet.Cells[$"B{row}"].Value = canceladas;
            row++;

            worksheet.Cells[$"A{row}"].Value = "Total de Usuarios";
            worksheet.Cells[$"B{row}"].Value = totalUsuarios;
            row++;

            worksheet.Cells[$"A{row}"].Value = "Total de Agentes";
            worksheet.Cells[$"B{row}"].Value = totalAgentes;
            row++;

            worksheet.Cells[$"A{row}"].Value = "Total de Áreas";
            worksheet.Cells[$"B{row}"].Value = totalAreas;
            row++;

            worksheet.Cells[$"A{row}"].Value = "Fecha del Reporte";
            worksheet.Cells[$"B{row}"].Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            FormatearHoja(worksheet);
        }

        private async Task CrearHojaPorGestor(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets.Add("Por Gestor");

            // Cabeceras
            worksheet.Cells["A1"].Value = "Gestor";
            worksheet.Cells["B1"].Value = "Área";
            worksheet.Cells["C1"].Value = "Total";
            worksheet.Cells["D1"].Value = "Nuevas";
            worksheet.Cells["E1"].Value = "En Proceso";
            worksheet.Cells["F1"].Value = "Cerradas";
            worksheet.Cells["G1"].Value = "Resueltas";
            worksheet.Cells["H1"].Value = "Rechazadas";
            FormatearCabecera(worksheet.Cells["A1:H1"]);

            // Obtener datos agrupados por gestor
            var datos = await _context.Solicitudes
                .Where(s => s.GestorAsignadoId != null)
                .Include(s => s.GestorAsignado)
                    .ThenInclude(u => u!.Area)
                .GroupBy(s => new
                {
                    GestorNombre = s.GestorAsignado!.Nombre,
                    AreaNombre = s.GestorAsignado.Area != null ? s.GestorAsignado.Area.Nombre : "Sin Área"
                })
                .Select(g => new
                {
                    Gestor = g.Key.GestorNombre,
                    Area = g.Key.AreaNombre,
                    Total = g.Count(),
                    Nuevas = g.Count(s => s.Estado == EstadoSolicitudEnum.Nueva),
                    EnProceso = g.Count(s => s.Estado == EstadoSolicitudEnum.EnProceso),
                    Cerradas = g.Count(s => s.Estado == EstadoSolicitudEnum.Cerrada),
                    Resueltas = g.Count(s => s.Estado == EstadoSolicitudEnum.Resuelta),
                    Rechazadas = g.Count(s => s.Estado == EstadoSolicitudEnum.Rechazada)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            // Llenar datos
            int row = 2;
            foreach (var item in datos)
            {
                worksheet.Cells[$"A{row}"].Value = item.Gestor;
                worksheet.Cells[$"B{row}"].Value = item.Area;
                worksheet.Cells[$"C{row}"].Value = item.Total;
                worksheet.Cells[$"D{row}"].Value = item.Nuevas;
                worksheet.Cells[$"E{row}"].Value = item.EnProceso;
                worksheet.Cells[$"F{row}"].Value = item.Cerradas;
                worksheet.Cells[$"G{row}"].Value = item.Resueltas;
                worksheet.Cells[$"H{row}"].Value = item.Rechazadas;
                row++;
            }

            FormatearHoja(worksheet);
        }

        private async Task CrearHojaPorArea(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets.Add("Por Área");

            // Cabeceras
            worksheet.Cells["A1"].Value = "Área";
            worksheet.Cells["B1"].Value = "Total";
            worksheet.Cells["C1"].Value = "Nuevas";
            worksheet.Cells["D1"].Value = "En Proceso";
            worksheet.Cells["E1"].Value = "Resueltas";
            worksheet.Cells["F1"].Value = "Cerradas";
            worksheet.Cells["G1"].Value = "Rechazadas";
            FormatearCabecera(worksheet.Cells["A1:G1"]);

            // Obtener datos agrupados por área
            var datos = await _context.Solicitudes
                .Include(s => s.TipoSolicitud)
                    .ThenInclude(ts => ts.Area)
                .GroupBy(s => s.TipoSolicitud.Area != null ? s.TipoSolicitud.Area.Nombre : "Sin Área (Tipo Otro)")
                .Select(g => new
                {
                    Area = g.Key,
                    Total = g.Count(),
                    Nuevas = g.Count(s => s.Estado == EstadoSolicitudEnum.Nueva),
                    EnProceso = g.Count(s => s.Estado == EstadoSolicitudEnum.EnProceso),
                    Resueltas = g.Count(s => s.Estado == EstadoSolicitudEnum.Resuelta),
                    Cerradas = g.Count(s => s.Estado == EstadoSolicitudEnum.Cerrada),
                    Rechazadas = g.Count(s => s.Estado == EstadoSolicitudEnum.Rechazada)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            // Llenar datos
            int row = 2;
            foreach (var item in datos)
            {
                worksheet.Cells[$"A{row}"].Value = item.Area;
                worksheet.Cells[$"B{row}"].Value = item.Total;
                worksheet.Cells[$"C{row}"].Value = item.Nuevas;
                worksheet.Cells[$"D{row}"].Value = item.EnProceso;
                worksheet.Cells[$"E{row}"].Value = item.Resueltas;
                worksheet.Cells[$"F{row}"].Value = item.Cerradas;
                worksheet.Cells[$"G{row}"].Value = item.Rechazadas;
                row++;
            }

            FormatearHoja(worksheet);
        }

        private async Task CrearHojaPorEstado(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets.Add("Por Estado");

            // Cabeceras
            worksheet.Cells["A1"].Value = "Estado";
            worksheet.Cells["B1"].Value = "Cantidad";
            worksheet.Cells["C1"].Value = "Porcentaje";
            FormatearCabecera(worksheet.Cells["A1:C1"]);

            // Obtener datos
            var totalSolicitudes = await _context.Solicitudes.CountAsync();
            var datos = await _context.Solicitudes
                .GroupBy(s => s.Estado)
                .Select(g => new
                {
                    Estado = g.Key.ToString(),
                    Cantidad = g.Count()
                })
                .OrderByDescending(x => x.Cantidad)
                .ToListAsync();

            // Llenar datos
            int row = 2;
            foreach (var item in datos)
            {
                worksheet.Cells[$"A{row}"].Value = item.Estado;
                worksheet.Cells[$"B{row}"].Value = item.Cantidad;
                worksheet.Cells[$"C{row}"].Value = totalSolicitudes > 0 ? (double)item.Cantidad / totalSolicitudes : 0;
                worksheet.Cells[$"C{row}"].Style.Numberformat.Format = "0.0%";
                row++;
            }

            FormatearHoja(worksheet);
        }

        private async Task CrearHojaPorPrioridad(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets.Add("Por Prioridad");

            // Cabeceras
            worksheet.Cells["A1"].Value = "Prioridad";
            worksheet.Cells["B1"].Value = "Cantidad";
            worksheet.Cells["C1"].Value = "Porcentaje";
            FormatearCabecera(worksheet.Cells["A1:C1"]);

            // Obtener datos
            var totalSolicitudes = await _context.Solicitudes.CountAsync();
            var datos = await _context.Solicitudes
                .GroupBy(s => s.Prioridad)
                .Select(g => new
                {
                    Prioridad = g.Key,
                    Cantidad = g.Count()
                })
                .ToListAsync();

            // Ordenar manualmente por prioridad: Alta -> Media -> Baja
            var datosOrdenados = datos
                .OrderBy(x => x.Prioridad == PrioridadEnum.Alta ? 1 :
                             x.Prioridad == PrioridadEnum.Media ? 2 : 3)
                .ToList();

            // Llenar datos
            int row = 2;
            foreach (var item in datosOrdenados)
            {
                worksheet.Cells[$"A{row}"].Value = item.Prioridad.ToString();
                worksheet.Cells[$"B{row}"].Value = item.Cantidad;
                worksheet.Cells[$"C{row}"].Value = totalSolicitudes > 0 ? (double)item.Cantidad / totalSolicitudes : 0;
                worksheet.Cells[$"C{row}"].Style.Numberformat.Format = "0.0%";
                row++;
            }

            FormatearHoja(worksheet);
        }

        private async Task CrearHojaPorTipo(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets.Add("Por Tipo");

            // Cabeceras
            worksheet.Cells["A1"].Value = "Tipo de Solicitud";
            worksheet.Cells["B1"].Value = "Área";
            worksheet.Cells["C1"].Value = "Total";
            worksheet.Cells["D1"].Value = "Nuevas";
            worksheet.Cells["E1"].Value = "Resueltas";
            worksheet.Cells["F1"].Value = "Rechazadas";
            FormatearCabecera(worksheet.Cells["A1:F1"]);

            // Obtener datos
            var datos = await _context.Solicitudes
                .Include(s => s.TipoSolicitud)
                    .ThenInclude(ts => ts.Area)
                .GroupBy(s => new
                {
                    TipoNombre = s.TipoSolicitud.Nombre,
                    AreaNombre = s.TipoSolicitud.Area != null ? s.TipoSolicitud.Area.Nombre : "Sin Área"
                })
                .Select(g => new
                {
                    Tipo = g.Key.TipoNombre,
                    Area = g.Key.AreaNombre,
                    Total = g.Count(),
                    Nuevas = g.Count(s => s.Estado == EstadoSolicitudEnum.Nueva),
                    Resueltas = g.Count(s => s.Estado == EstadoSolicitudEnum.Resuelta),
                    Rechazadas = g.Count(s => s.Estado == EstadoSolicitudEnum.Rechazada)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            // Llenar datos
            int row = 2;
            foreach (var item in datos)
            {
                worksheet.Cells[$"A{row}"].Value = item.Tipo;
                worksheet.Cells[$"B{row}"].Value = item.Area;
                worksheet.Cells[$"C{row}"].Value = item.Total;
                worksheet.Cells[$"D{row}"].Value = item.Nuevas;
                worksheet.Cells[$"E{row}"].Value = item.Resueltas;
                worksheet.Cells[$"F{row}"].Value = item.Rechazadas;
                row++;
            }

            FormatearHoja(worksheet);
        }

        private async Task CrearHojaDetalleCompleto(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets.Add("Detalle Completo");

            // Cabeceras
            worksheet.Cells["A1"].Value = "N°";
            worksheet.Cells["B1"].Value = "Asunto";
            worksheet.Cells["C1"].Value = "Estado";
            worksheet.Cells["D1"].Value = "Prioridad";
            worksheet.Cells["E1"].Value = "Tipo";
            worksheet.Cells["F1"].Value = "Área";
            worksheet.Cells["G1"].Value = "Solicitante";
            worksheet.Cells["H1"].Value = "Gestor";
            worksheet.Cells["I1"].Value = "Creación";
            worksheet.Cells["J1"].Value = "Actualización";
            FormatearCabecera(worksheet.Cells["A1:J1"]);

            // Obtener datos
            var solicitudes = await _context.Solicitudes
                .Include(s => s.TipoSolicitud)
                    .ThenInclude(ts => ts.Area)
                .Include(s => s.Solicitante)
                .Include(s => s.GestorAsignado)
                .OrderByDescending(s => s.FechaCreacion)
                .ToListAsync();

            // Llenar datos
            int row = 2;
            foreach (var sol in solicitudes)
            {
                worksheet.Cells[$"A{row}"].Value = sol.Numero;
                worksheet.Cells[$"B{row}"].Value = sol.Asunto;
                worksheet.Cells[$"C{row}"].Value = sol.Estado.ToString();
                worksheet.Cells[$"D{row}"].Value = sol.Prioridad.ToString();
                worksheet.Cells[$"E{row}"].Value = sol.TipoSolicitud?.Nombre ?? "";
                worksheet.Cells[$"F{row}"].Value = sol.TipoSolicitud?.Area?.Nombre ?? "Sin Área";
                worksheet.Cells[$"G{row}"].Value = sol.Solicitante?.Nombre ?? "";
                worksheet.Cells[$"H{row}"].Value = sol.GestorAsignado?.Nombre ?? "Sin asignar";
                worksheet.Cells[$"I{row}"].Value = sol.FechaCreacion.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cells[$"J{row}"].Value = sol.FechaCierre?.ToString("dd/MM/yyyy HH:mm") ?? "-";
                row++;
            }

            FormatearHoja(worksheet);
        }

        private async Task CrearHojaHistorialSistema(ExcelPackage package)
        {
            var worksheet = package.Workbook.Worksheets.Add("Historial Sistema");

            // Cabeceras
            worksheet.Cells["A1"].Value = "Fecha";
            worksheet.Cells["B1"].Value = "Usuario";
            worksheet.Cells["C1"].Value = "Rol";
            worksheet.Cells["D1"].Value = "Acción";
            worksheet.Cells["E1"].Value = "Solicitud";
            worksheet.Cells["F1"].Value = "Detalle";
            FormatearCabecera(worksheet.Cells["A1:F1"]);

            // Obtener últimos 100 eventos del sistema
            var eventos = await _context.Comentarios
                .Where(c => c.EsSistema)
                .Include(c => c.Usuario)
                .Include(c => c.Solicitud)
                .OrderByDescending(c => c.FechaCreacion)
                .Take(100)
                .ToListAsync();

            // Llenar datos
            int row = 2;
            foreach (var evento in eventos)
            {
                worksheet.Cells[$"A{row}"].Value = evento.FechaCreacion.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cells[$"B{row}"].Value = evento.Usuario?.Nombre ?? "Sistema";
                worksheet.Cells[$"C{row}"].Value = ObtenerNombreRol((int)(evento.Usuario?.Rol ?? RolEnum.Usuario));
                worksheet.Cells[$"D{row}"].Value = evento.TipoEvento?.ToString() ?? "Sistema";
                worksheet.Cells[$"E{row}"].Value = evento.Solicitud?.Numero ?? "";
                worksheet.Cells[$"F{row}"].Value = evento.Texto;
                row++;
            }

            FormatearHoja(worksheet);
        }

        private void FormatearCabecera(ExcelRange range)
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(240, 240, 240));
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }

        private void FormatearHoja(ExcelWorksheet worksheet)
        {
            if (worksheet.Dimension != null)
            {
                // Aplicar bordes a todas las celdas
                var rango = worksheet.Cells[worksheet.Dimension.Address];
                rango.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                rango.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                rango.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                rango.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                // Aplicar auto-filtro a la primera fila
                worksheet.Cells[worksheet.Dimension.Address].AutoFilter = true;

                // Ajustar ancho de columnas
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }
        }

        private string ObtenerNombreRol(int rol)
        {
            return rol switch
            {
                1 => "Usuario",
                2 => "Administrador",
                3 => "Super Administrador",
                4 => "Agente de Área",
                _ => "Desconocido"
            };
        }
    }
}
