using Application.DTOs.Comentarios;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class ComentarioService : IComentarioService
    {
        private readonly ApplicationDbContext _context;

        public ComentarioService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AgregarComentarioAsync(int solicitudId, int usuarioId, string rol, int? areaId, AgregarComentarioDto dto)
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

            // Validar que la solicitud no esté cerrada, rechazada o cancelada
            if (solicitud.Estado == EstadoSolicitudEnum.Cerrada || 
                solicitud.Estado == EstadoSolicitudEnum.Rechazada ||
                solicitud.Estado == EstadoSolicitudEnum.Cancelada)
                throw new BusinessException("No se pueden agregar comentarios a solicitudes cerradas, rechazadas o canceladas");

            // Validar que el contenido no esté vacío
            if (string.IsNullOrWhiteSpace(dto.Contenido))
                throw new BusinessException("El contenido del comentario es requerido");

            // Crear el comentario
            var comentario = new Comentario
            {
                SolicitudId = solicitudId,
                UsuarioId = usuarioId,
                Texto = dto.Contenido.Trim(),
                FechaCreacion = DateTime.Now
            };

            _context.Comentarios.Add(comentario);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ComentarioDto>> ObtenerPorSolicitudAsync(int solicitudId, int usuarioId, string rol, int? areaId)
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

            // Obtener comentarios ordenados por fecha ascendente
            var comentarios = await _context.Comentarios
                .Include(c => c.Usuario)
                .Where(c => c.SolicitudId == solicitudId)
                .OrderBy(c => c.FechaCreacion)
                .Select(c => new ComentarioDto
                {
                    Id = c.Id,
                    Contenido = c.Texto,
                    FechaCreacion = c.FechaCreacion,
                    UsuarioId = c.UsuarioId,
                    NombreUsuario = c.Usuario.Nombre
                })
                .ToListAsync();

            return comentarios;
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
