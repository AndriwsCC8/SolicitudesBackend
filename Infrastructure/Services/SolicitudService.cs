using Application.DTOs.Solicitudes;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
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
                throw new NotFoundException("Tipo de solicitud no encontrado o inactivo");

            // Obtener usuario
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
                throw new NotFoundException("Usuario no encontrado");

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

        public async Task<SolicitudDto> AsignarAgenteAsync(AsignarAgenteDto dto, int adminId)
        {
            var solicitud = await _context.Solicitudes
                .Include(s => s.Area)
                .Include(s => s.TipoSolicitud)
                .Include(s => s.Solicitante)
                .Include(s => s.GestorAsignado)
                .FirstOrDefaultAsync(s => s.Id == dto.SolicitudId);

            if (solicitud == null)
                throw new NotFoundException("Solicitud no encontrada");

            if (solicitud.Estado == EstadoSolicitudEnum.Cerrada || solicitud.Estado == EstadoSolicitudEnum.Rechazada)
                throw new BusinessException("No se puede asignar agente a una solicitud cerrada o rechazada");

            var agente = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == dto.AgenteId && u.Rol == RolEnum.AgenteArea && u.Activo);

            if (agente == null)
                throw new NotFoundException("Agente no encontrado o no tiene el rol AgenteArea");

            if (agente.AreaId != solicitud.AreaId)
                throw new BusinessException("El agente no pertenece al área de la solicitud");

            var estadoAnterior = solicitud.Estado;
            solicitud.GestorAsignadoId = dto.AgenteId;

            if (solicitud.Estado == EstadoSolicitudEnum.Nueva)
            {
                solicitud.Estado = EstadoSolicitudEnum.EnProceso;
                await RegistrarHistorialAsync(solicitud.Id, adminId, estadoAnterior, EstadoSolicitudEnum.EnProceso,
                    $"Asignado a {agente.Nombre}");
            }

            await _context.SaveChangesAsync();
            await _context.Entry(solicitud).Reference(s => s.GestorAsignado).LoadAsync();

            return MapToDto(solicitud);
        }

        public async Task<SolicitudDto> CambiarEstadoAsync(CambiarEstadoDto dto, int agenteId)
        {
            var solicitud = await _context.Solicitudes
                .Include(s => s.Area)
                .Include(s => s.TipoSolicitud)
                .Include(s => s.Solicitante)
                .Include(s => s.GestorAsignado)
                .FirstOrDefaultAsync(s => s.Id == dto.SolicitudId);

            if (solicitud == null)
                throw new NotFoundException("Solicitud no encontrada");

            var agente = await _context.Usuarios.FindAsync(agenteId);
            if (agente == null)
                throw new NotFoundException("Agente no encontrado");

            if (agente.AreaId != solicitud.AreaId)
                throw new UnauthorizedActionException("No tienes permiso para gestionar solicitudes de esta área");

            if (solicitud.GestorAsignadoId != agenteId)
                throw new UnauthorizedActionException("No estás asignado a esta solicitud");

            var nuevoEstado = (EstadoSolicitudEnum)dto.NuevoEstado;

            if (!EsTransicionValida(solicitud.Estado, nuevoEstado))
                throw new BusinessException($"Transición no válida de {solicitud.Estado} a {nuevoEstado}");

            var estadoAnterior = solicitud.Estado;
            solicitud.Estado = nuevoEstado;

            await RegistrarHistorialAsync(solicitud.Id, agenteId, estadoAnterior, nuevoEstado, dto.Observacion);
            await _context.SaveChangesAsync();

            return MapToDto(solicitud);
        }

        public async Task<SolicitudDto> RechazarAsync(RechazarSolicitudDto dto, int agenteId)
        {
            var solicitud = await _context.Solicitudes
                .Include(s => s.Area)
                .Include(s => s.TipoSolicitud)
                .Include(s => s.Solicitante)
                .Include(s => s.GestorAsignado)
                .FirstOrDefaultAsync(s => s.Id == dto.SolicitudId);

            if (solicitud == null)
                throw new NotFoundException("Solicitud no encontrada");

            var agente = await _context.Usuarios.FindAsync(agenteId);
            if (agente == null)
                throw new NotFoundException("Agente no encontrado");

            if (agente.AreaId != solicitud.AreaId)
                throw new UnauthorizedActionException("No tienes permiso para gestionar solicitudes de esta área");

            if (solicitud.GestorAsignadoId != agenteId)
                throw new UnauthorizedActionException("No estás asignado a esta solicitud");

            if (solicitud.Estado == EstadoSolicitudEnum.Cerrada || solicitud.Estado == EstadoSolicitudEnum.Rechazada)
                throw new BusinessException("No se puede rechazar una solicitud cerrada o ya rechazada");

            var estadoAnterior = solicitud.Estado;
            solicitud.Estado = EstadoSolicitudEnum.Rechazada;
            solicitud.MotivoRechazo = dto.MotivoRechazo;
            solicitud.FechaCierre = DateTime.Now;

            await RegistrarHistorialAsync(solicitud.Id, agenteId, estadoAnterior, EstadoSolicitudEnum.Rechazada,
                $"Rechazada: {dto.MotivoRechazo}");
            await _context.SaveChangesAsync();

            return MapToDto(solicitud);
        }

        public async Task<SolicitudDto> CerrarAsync(CerrarSolicitudDto dto, int usuarioId)
        {
            var solicitud = await _context.Solicitudes
                .Include(s => s.Area)
                .Include(s => s.TipoSolicitud)
                .Include(s => s.Solicitante)
                .Include(s => s.GestorAsignado)
                .FirstOrDefaultAsync(s => s.Id == dto.SolicitudId);

            if (solicitud == null)
                throw new NotFoundException("Solicitud no encontrada");

            if (solicitud.SolicitanteId != usuarioId)
                throw new UnauthorizedActionException("Solo el solicitante puede cerrar su solicitud");

            if (solicitud.Estado != EstadoSolicitudEnum.Resuelta)
                throw new BusinessException("Solo se pueden cerrar solicitudes en estado Resuelta");

            var estadoAnterior = solicitud.Estado;
            solicitud.Estado = EstadoSolicitudEnum.Cerrada;
            solicitud.FechaCierre = DateTime.Now;

            await RegistrarHistorialAsync(solicitud.Id, usuarioId, estadoAnterior, EstadoSolicitudEnum.Cerrada,
                dto.Observacion ?? "Cerrada por el solicitante");
            await _context.SaveChangesAsync();

            return MapToDto(solicitud);
        }

        private async Task RegistrarHistorialAsync(int solicitudId, int usuarioId, EstadoSolicitudEnum estadoAnterior,
            EstadoSolicitudEnum estadoNuevo, string? observacion)
        {
            var historial = new HistorialEstado
            {
                SolicitudId = solicitudId,
                UsuarioId = usuarioId,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = estadoNuevo,
                Observacion = observacion,
                FechaCambio = DateTime.Now
            };

            _context.HistorialEstados.Add(historial);
        }

        private bool EsTransicionValida(EstadoSolicitudEnum estadoActual, EstadoSolicitudEnum estadoNuevo)
        {
            return estadoActual switch
            {
                EstadoSolicitudEnum.Nueva => estadoNuevo == EstadoSolicitudEnum.EnProceso,
                EstadoSolicitudEnum.EnProceso => estadoNuevo == EstadoSolicitudEnum.Resuelta,
                EstadoSolicitudEnum.Resuelta => estadoNuevo == EstadoSolicitudEnum.Cerrada,
                _ => false
            };
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
