using Application.DTOs.Historial;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class HistorialService : IHistorialService
    {
        private readonly ApplicationDbContext _context;

        public HistorialService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<HistorialEstadoDto>> ObtenerHistorialAsync(int solicitudId, int usuarioId, string rol, int? areaId)
        {
            // Validar que la solicitud existe
            var solicitud = await _context.Solicitudes
                .Include(s => s.Solicitante)
                .Include(s => s.Area)
                .FirstOrDefaultAsync(s => s.Id == solicitudId);

            if (solicitud == null)
                throw new NotFoundException($"Solicitud con ID {solicitudId} no encontrada");

            // Validar permisos según el rol
            ValidarPermisoAcceso(solicitud, usuarioId, rol, areaId);

            // Obtener historial ordenado por fecha ascendente
            var historial = await _context.HistorialEstados
                .Include(h => h.Usuario)
                .Where(h => h.SolicitudId == solicitudId)
                .OrderBy(h => h.FechaCambio)
                .Select(h => new HistorialEstadoDto
                {
                    EstadoAnterior = h.EstadoAnterior.ToString(),
                    EstadoNuevo = h.EstadoNuevo.ToString(),
                    Observacion = h.Observacion,
                    Fecha = h.FechaCambio,
                    Usuario = h.Usuario.Nombre
                })
                .ToListAsync();

            return historial;
        }

        private void ValidarPermisoAcceso(Solicitud solicitud, int usuarioId, string rol, int? areaId)
        {
            switch (rol)
            {
                case "Usuario":
                    // El usuario solo puede acceder a sus propias solicitudes
                    if (solicitud.SolicitanteId != usuarioId)
                        throw new UnauthorizedActionException("No tienes permiso para acceder a esta solicitud");
                    break;

                case "AgenteArea":
                    // El agente solo puede acceder a solicitudes de su área
                    if (!areaId.HasValue || solicitud.AreaId != areaId.Value)
                        throw new UnauthorizedActionException("No tienes permiso para acceder a solicitudes de esta área");
                    break;

                case "Administrador":
                case "SuperAdministrador":
                    // Tienen acceso a todas las solicitudes
                    break;

                default:
                    throw new UnauthorizedActionException("Rol no autorizado");
            }
        }
    }
}
