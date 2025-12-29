using Application.DTOs.Solicitudes;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class SolicitudService : ISolicitudService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SolicitudService> _logger;

        public SolicitudService(ApplicationDbContext context, ILogger<SolicitudService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SolicitudDto> CrearAsync(CrearSolicitudDto dto, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Iniciando CrearAsync. UsuarioId: {UsuarioId}, TipoSolicitudId: {TipoSolicitudId}", 
                    usuarioId, dto.TipoSolicitudId);

                // Validar que el tipo de solicitud existe
                var tipoSolicitud = await _context.TiposSolicitud
                    .Include(t => t.Area)
                    .FirstOrDefaultAsync(t => t.Id == dto.TipoSolicitudId && t.Activo);

                if (tipoSolicitud == null)
                {
                    _logger.LogWarning("TipoSolicitud no encontrado o inactivo. Id: {TipoSolicitudId}", dto.TipoSolicitudId);
                    throw new NotFoundException("Tipo de solicitud no encontrado o inactivo");
                }

                _logger.LogInformation("TipoSolicitud encontrado: {Nombre}, AreaId: {AreaId}", 
                    tipoSolicitud.Nombre, tipoSolicitud.AreaId);

                // Obtener usuario
                var usuario = await _context.Usuarios.FindAsync(usuarioId);
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario no encontrado. Id: {UsuarioId}", usuarioId);
                    throw new NotFoundException("Usuario no encontrado");
                }

                _logger.LogInformation("Usuario encontrado: {NombreUsuario}", usuario.NombreUsuario);

            // Generar número de solicitud único (SOL-YYYY-####)
            var año = DateTime.Now.Year;
                var prefijo = $"SOL-{año}-";
                
                // Obtener el máximo número de solicitud del año actual
                var ultimaSolicitudDelAño = await _context.Solicitudes
                    .Where(s => s.Numero.StartsWith(prefijo))
                    .OrderByDescending(s => s.Id) // Ordenar por ID en lugar de Numero
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
                
                // Verificar que el número no exista (por si acaso)
                string numeroSolicitud;
                bool numeroExiste;
                int intentos = 0;
                do
                {
                    numeroSolicitud = $"SOL-{año}-{siguienteNumero:D4}";
                    numeroExiste = await _context.Solicitudes.AnyAsync(s => s.Numero == numeroSolicitud);
                    if (numeroExiste)
                    {
                        _logger.LogWarning("El número {Numero} ya existe, incrementando...", numeroSolicitud);
                        siguienteNumero++;
                        intentos++;
                        if (intentos > 100)
                        {
                            throw new BusinessException("No se pudo generar un número único de solicitud");
                        }
                    }
                } while (numeroExiste);

                _logger.LogInformation("Generando solicitud con número: {Numero}", numeroSolicitud);

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
            _logger.LogInformation("Guardando solicitud en la base de datos...");
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Solicitud guardada. Id: {SolicitudId}", solicitud.Id);

            // Crear comentario inicial (directo al contexto, sin llamadas HTTP)
            var comentarioInicial = new Comentario
            {
                SolicitudId = solicitud.Id,
                UsuarioId = usuarioId,
                Texto = "Solicitud creada",
                FechaCreacion = DateTime.Now
            };

            _context.Comentarios.Add(comentarioInicial);
            _logger.LogInformation("Guardando comentario inicial...");
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Comentario guardado. Id: {ComentarioId}", comentarioInicial.Id);

            // Cargar relaciones para el DTO
            _logger.LogInformation("Cargando relaciones de la solicitud...");
            await _context.Entry(solicitud)
                .Reference(s => s.Area)
                .LoadAsync();
            await _context.Entry(solicitud)
                .Reference(s => s.TipoSolicitud)
                .LoadAsync();
            await _context.Entry(solicitud)
                .Reference(s => s.Solicitante)
                .LoadAsync();

            _logger.LogInformation("Relaciones cargadas correctamente");
            
            var solicitudDto = MapToDto(solicitud);
            
            _logger.LogInformation("Solicitud creada exitosamente. Número: {Numero}", solicitudDto.Numero);
            
            return solicitudDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear solicitud. UsuarioId: {UsuarioId}, TipoSolicitudId: {TipoSolicitudId}",
                    usuarioId, dto.TipoSolicitudId);
                throw;
            }
        }

        public async Task<SolicitudDto?> ObtenerPorIdAsync(int solicitudId, int usuarioId)
        {
            try
            {
                _logger.LogInformation("Obteniendo solicitud {SolicitudId} para usuario {UsuarioId}", 
                    solicitudId, usuarioId);

                // Obtener el usuario para verificar su rol
                var usuario = await _context.Usuarios.FindAsync(usuarioId);
                if (usuario == null)
                {
                    _logger.LogWarning("Usuario {UsuarioId} no encontrado", usuarioId);
                    return null;
                }

                _logger.LogInformation("Usuario {UsuarioId} tiene rol {Rol}", usuarioId, usuario.Rol);

                // Buscar la solicitud con todas sus relaciones
                var solicitud = await _context.Solicitudes
                    .Include(s => s.Area)
                    .Include(s => s.TipoSolicitud)
                    .Include(s => s.Solicitante)
                    .Include(s => s.GestorAsignado)
                    .Include(s => s.Comentarios)
                        .ThenInclude(c => c.Usuario)
                    .FirstOrDefaultAsync(s => s.Id == solicitudId);

                if (solicitud == null)
                {
                    _logger.LogWarning("Solicitud {SolicitudId} no existe en la BD", solicitudId);
                    return null;
                }

                _logger.LogInformation("Solicitud encontrada. SolicitanteId: {SolicitanteId}, AreaId: {AreaId}", 
                    solicitud.SolicitanteId, solicitud.AreaId);

                // Verificar permisos:
                // 1. Si es el solicitante, puede verla
                // 2. Si es Administrador (Rol 2) o SuperAdministrador (Rol 3), puede verla
                // 3. Si es AgenteArea (Rol 4) y pertenece al área de la solicitud, puede verla
                bool tienePermiso = false;

                if (solicitud.SolicitanteId == usuarioId)
                {
                    tienePermiso = true;
                    _logger.LogInformation("Permiso concedido: Usuario es el solicitante");
                }
                else if ((int)usuario.Rol >= 2) // Administrador o SuperAdministrador
                {
                    tienePermiso = true;
                    _logger.LogInformation("Permiso concedido: Usuario es Administrador/SuperAdmin (Rol {Rol})", usuario.Rol);
                }
                else if ((int)usuario.Rol == 4 && usuario.AreaId.HasValue && usuario.AreaId == solicitud.AreaId)
                {
                    tienePermiso = true;
                    _logger.LogInformation("Permiso concedido: Usuario es AgenteArea del área {AreaId}", usuario.AreaId);
                }

                if (!tienePermiso)
                {
                    _logger.LogWarning("Usuario {UsuarioId} sin permisos para ver solicitud {SolicitudId}", 
                        usuarioId, solicitudId);
                    return null;
                }

                _logger.LogInformation("Solicitud {SolicitudId} devuelta exitosamente", solicitudId);
                return MapToDto(solicitud);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener solicitud {SolicitudId} para usuario {UsuarioId}", 
                    solicitudId, usuarioId);
                throw;
            }
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

            if (solicitud.Estado == EstadoSolicitudEnum.Cerrada || 
                solicitud.Estado == EstadoSolicitudEnum.Rechazada ||
                solicitud.Estado == EstadoSolicitudEnum.Cancelada)
                throw new BusinessException("No se puede asignar agente a una solicitud cerrada, rechazada o cancelada");

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

            // Permitir si es Administrador/SuperAdministrador O si es AgenteArea del área correcta
            bool esAdmin = agente.Rol == RolEnum.Administrador || agente.Rol == RolEnum.SuperAdministrador;
            bool esAgenteDelArea = agente.Rol == RolEnum.AgenteArea && agente.AreaId == solicitud.AreaId;

            if (!esAdmin && !esAgenteDelArea)
            {
                throw new UnauthorizedActionException("No tienes permiso para gestionar solicitudes de esta área");
            }

            var nuevoEstado = (EstadoSolicitudEnum)dto.NuevoEstado;

            // Los administradores pueden cambiar a cualquier estado, los agentes solo transiciones válidas
            if (!esAdmin && !EsTransicionValida(solicitud.Estado, nuevoEstado))
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

            // Permitir si es Administrador/SuperAdministrador O si es AgenteArea del área correcta
            bool esAdmin = agente.Rol == RolEnum.Administrador || agente.Rol == RolEnum.SuperAdministrador;
            bool esAgenteDelArea = agente.Rol == RolEnum.AgenteArea && agente.AreaId == solicitud.AreaId;

            if (!esAdmin && !esAgenteDelArea)
            {
                throw new UnauthorizedActionException("No tienes permiso para gestionar solicitudes de esta área");
            }

            if (solicitud.Estado == EstadoSolicitudEnum.Cerrada || 
                solicitud.Estado == EstadoSolicitudEnum.Rechazada ||
                solicitud.Estado == EstadoSolicitudEnum.Cancelada)
                throw new BusinessException("No se puede rechazar una solicitud cerrada, rechazada o cancelada");

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
            // Transiciones permitidas para agentes del área:
            // Nueva → EnProceso, Resuelta, Rechazada o Cancelada (permite resolver directamente)
            // EnProceso → Resuelta, Rechazada o Cancelada  
            // Resuelta → Cerrada o Cancelada
            // Los administradores NO pasan por esta validación
            return estadoActual switch
            {
                EstadoSolicitudEnum.Nueva => estadoNuevo == EstadoSolicitudEnum.EnProceso || 
                                              estadoNuevo == EstadoSolicitudEnum.Resuelta ||
                                              estadoNuevo == EstadoSolicitudEnum.Rechazada ||
                                              estadoNuevo == EstadoSolicitudEnum.Cancelada,
                EstadoSolicitudEnum.EnProceso => estadoNuevo == EstadoSolicitudEnum.Resuelta ||
                                                   estadoNuevo == EstadoSolicitudEnum.Rechazada ||
                                                   estadoNuevo == EstadoSolicitudEnum.Cancelada,
                EstadoSolicitudEnum.Resuelta => estadoNuevo == EstadoSolicitudEnum.Cerrada ||
                                                 estadoNuevo == EstadoSolicitudEnum.Cancelada,
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
                
                // Área con ID
                AreaId = solicitud.AreaId,
                Area = solicitud.Area?.Nombre ?? string.Empty,
                
                // TipoSolicitud con ID
                TipoSolicitudId = solicitud.TipoSolicitudId,
                TipoSolicitud = solicitud.TipoSolicitud?.Nombre ?? string.Empty,
                
                // Solicitante con ID y email
                SolicitanteId = solicitud.SolicitanteId,
                Solicitante = solicitud.Solicitante?.Nombre ?? string.Empty,
                SolicitanteEmail = solicitud.Solicitante?.Email ?? string.Empty,
                
                // Gestor Asignado (opcional) con ID y email
                GestorAsignadoId = solicitud.GestorAsignadoId,
                GestorAsignado = solicitud.GestorAsignado?.Nombre,
                GestorAsignadoEmail = solicitud.GestorAsignado?.Email,
                
                // Fechas
                FechaCreacion = solicitud.FechaCreacion,
                FechaCierre = solicitud.FechaCierre,
                
                // Campos adicionales
                MotivoRechazo = solicitud.MotivoRechazo,
                
                // Comentarios
                Comentarios = solicitud.Comentarios?.Select(c => new Application.DTOs.Comentarios.ComentarioDto
                {
                    Id = c.Id,
                    Contenido = c.Texto,
                    FechaCreacion = c.FechaCreacion,
                    UsuarioId = c.UsuarioId,
                    NombreUsuario = c.Usuario?.Nombre ?? string.Empty
                }).OrderBy(c => c.FechaCreacion).ToList() ?? new List<Application.DTOs.Comentarios.ComentarioDto>()
            };
        }
    }
}
