using Application.DTOs.Solicitudes;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class SolicitudService : ISolicitudService
    {
        private readonly ApplicationDbContext _context;

        public SolicitudService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SolicitudDto> CrearAsync(CrearSolicitudDto dto, int usuarioId)
        {
            // Validar que el tipo de solicitud existe
            var tipoSolicitud = await _context.TiposSolicitud
                .Include(t => t.Area)
                .FirstOrDefaultAsync(t => t.Id == dto.TipoSolicitudId && t.Activo);

            if (tipoSolicitud == null)
                throw new Exception("Tipo de solicitud no encontrado o inactivo");

            // Obtener usuario
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
                throw new Exception("Usuario no encontrado");

            // Generar número de solicitud único (SOL-YYYY-####)
            var año = DateTime.Now.Year;
            var ultimaSolicitudDelAño = await _context.Solicitudes
                .Where(s => s.Numero.StartsWith($"SOL-{año}-"))
                .OrderByDescending(s => s.Numero)
                .FirstOrDefaultAsync();

            int siguienteNumero = 1;
            if (ultimaSolicitudDelAño != null)
            {
                var partes = ultimaSolicitudDelAño.Numero.Split('-');
                if (partes.Length == 3 && int.TryParse(partes[2], out int numero))
                {
                    siguienteNumero = numero + 1;
                }
            }

            var numeroSolicitud = $"SOL-{año}-{siguienteNumero:D4}";

            // Crear la solicitud
            var solicitud = new Solicitud
            {
                Numero = numeroSolicitud,
                Asunto = dto.Asunto,
                Descripcion = dto.Descripcion,
                Prioridad = (PrioridadEnum)dto.Prioridad,
                Estado = EstadoSolicitudEnum.Nueva,
                TipoSolicitudId = dto.TipoSolicitudId,
                AreaId = tipoSolicitud.AreaId,
                SolicitanteId = usuarioId,
                FechaCreacion = DateTime.Now
            };

            _context.Solicitudes.Add(solicitud);
            await _context.SaveChangesAsync();

            // Cargar relaciones para el DTO
            await _context.Entry(solicitud)
                .Reference(s => s.Area)
                .LoadAsync();
            await _context.Entry(solicitud)
                .Reference(s => s.TipoSolicitud)
                .LoadAsync();
            await _context.Entry(solicitud)
                .Reference(s => s.Solicitante)
                .LoadAsync();

            return MapToDto(solicitud);
        }

        public async Task<IEnumerable<SolicitudDto>> ObtenerPorUsuarioAsync(int usuarioId)
        {
            var solicitudes = await _context.Solicitudes
                .Include(s => s.Area)
                .Include(s => s.TipoSolicitud)
                .Include(s => s.Solicitante)
                .Include(s => s.GestorAsignado)
                .Where(s => s.SolicitanteId == usuarioId)
                .OrderByDescending(s => s.FechaCreacion)
                .ToListAsync();

            return solicitudes.Select(MapToDto);
        }

        public async Task<IEnumerable<SolicitudDto>> ObtenerPorAreaAsync(int areaId)
        {
            var solicitudes = await _context.Solicitudes
                .Include(s => s.Area)
                .Include(s => s.TipoSolicitud)
                .Include(s => s.Solicitante)
                .Include(s => s.GestorAsignado)
                .Where(s => s.AreaId == areaId)
                .OrderByDescending(s => s.FechaCreacion)
                .ToListAsync();

            return solicitudes.Select(MapToDto);
        }

        public async Task<IEnumerable<SolicitudDto>> ObtenerTodasAsync()
        {
            var solicitudes = await _context.Solicitudes
                .Include(s => s.Area)
                .Include(s => s.TipoSolicitud)
                .Include(s => s.Solicitante)
                .Include(s => s.GestorAsignado)
                .OrderByDescending(s => s.FechaCreacion)
                .ToListAsync();

            return solicitudes.Select(MapToDto);
        }

        private SolicitudDto MapToDto(Solicitud solicitud)
        {
            return new SolicitudDto
            {
                Id = solicitud.Id,
                Numero = solicitud.Numero,
                Asunto = solicitud.Asunto,
                Descripcion = solicitud.Descripcion,
                Estado = solicitud.Estado.ToString(),
                Prioridad = solicitud.Prioridad.ToString(),
                Area = solicitud.Area.Nombre,
                TipoSolicitud = solicitud.TipoSolicitud.Nombre,
                Solicitante = solicitud.Solicitante.Nombre,
                GestorAsignado = solicitud.GestorAsignado?.Nombre,
                FechaCreacion = solicitud.FechaCreacion,
                FechaCierre = solicitud.FechaCierre
            };
        }
    }
}
