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

            // Guardar archivo si existe
            if (dto.Archivo != null && dto.Archivo.Length > 0)
            {
                _logger.LogInformation("Procesando archivo adjunto: {FileName}, Tamaño: {Size} bytes", 
                    dto.Archivo.FileName, dto.Archivo.Length);
                
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadPath);
                
                var fileName = $"{Guid.NewGuid()}_{dto.Archivo.FileName}";
                var filePath = Path.Combine(uploadPath, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Archivo.CopyToAsync(stream);
                }
                
                solicitud.ArchivoNombre = dto.Archivo.FileName;
                solicitud.ArchivoRuta = $"/uploads/{fileName}";
                solicitud.ArchivoContentType = dto.Archivo.ContentType;
                
                _logger.LogInformation("Archivo guardado: {FilePath}", filePath);
            }

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
                            .ThenInclude(u => u.Area)
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

        public async Task<SolicitudDto> TomarSolicitudAsync(int solicitudId, int usuarioId)
        {
            var solicitud = await _context.Solicitudes
                .Include(s => s.Area)
                .Include(s => s.TipoSolicitud)
                .Include(s => s.Solicitante)
                .Include(s => s.GestorAsignado)
                .FirstOrDefaultAsync(s => s.Id == solicitudId);

            if (solicitud == null)
                throw new NotFoundException("Solicitud no encontrada");

            // Validar que no esté ya asignada
            if (solicitud.GestorAsignadoId.HasValue)
                throw new BusinessException("La solicitud ya está asignada a otro gestor");

            // Validar que no esté cerrada/rechazada/cancelada
            if (solicitud.Estado == EstadoSolicitudEnum.Cerrada ||
                solicitud.Estado == EstadoSolicitudEnum.Rechazada ||
                solicitud.Estado == EstadoSolicitudEnum.Cancelada)
                throw new BusinessException("No se puede tomar una solicitud cerrada, rechazada o cancelada");

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
                throw new NotFoundException("Usuario no encontrado");

            // Validar permisos
            bool esAdmin = usuario.Rol == RolEnum.Administrador || usuario.Rol == RolEnum.SuperAdministrador;
            bool esAgenteDelArea = usuario.Rol == RolEnum.AgenteArea && usuario.AreaId == solicitud.AreaId;

            if (!esAdmin && !esAgenteDelArea)
                throw new UnauthorizedActionException("No tienes permiso para tomar solicitudes de esta área");

            // Asignar solicitud al usuario actual
            var estadoAnterior = solicitud.Estado;
            solicitud.GestorAsignadoId = usuarioId;

            // Cambiar estado a EnProceso si estaba Nueva
            if (solicitud.Estado == EstadoSolicitudEnum.Nueva)
            {
                solicitud.Estado = EstadoSolicitudEnum.EnProceso;
                await RegistrarHistorialAsync(solicitud.Id, usuarioId, estadoAnterior, EstadoSolicitudEnum.EnProceso,
                    $"Solicitud tomada por {usuario.Nombre}");
            }
            else
            {
                await RegistrarHistorialAsync(solicitud.Id, usuarioId, estadoAnterior, estadoAnterior,
                    $"Solicitud tomada por {usuario.Nombre}");
            }

            await _context.SaveChangesAsync();
            await _context.Entry(solicitud).Reference(s => s.GestorAsignado).LoadAsync();

            _logger.LogInformation("Usuario {UsuarioId} ({Nombre}) tomó la solicitud {SolicitudId}", 
                usuarioId, usuario.Nombre, solicitudId);

            return MapToDto(solicitud);
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

        public async Task<SolicitudDto> DesasignarGestorAsync(int solicitudId, int usuarioId)
        {
            var solicitud = await _context.Solicitudes
                .Include(s => s.Area)
                .Include(s => s.TipoSolicitud)
                .Include(s => s.Solicitante)
                .Include(s => s.GestorAsignado)
                .FirstOrDefaultAsync(s => s.Id == solicitudId);

            if (solicitud == null)
                throw new NotFoundException("Solicitud no encontrada");

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
                throw new NotFoundException("Usuario no encontrado");

            // Validar permisos: Admin, SuperAdmin o el gestor actualmente asignado
            bool esAdmin = usuario.Rol == RolEnum.Administrador || usuario.Rol == RolEnum.SuperAdministrador;
            bool esGestorAsignado = solicitud.GestorAsignadoId.HasValue && solicitud.GestorAsignadoId == usuarioId;

            if (!esAdmin && !esGestorAsignado)
                throw new UnauthorizedActionException("No tienes permiso para desasignar esta solicitud");

            // Si no había gestor asignado, no hay nada que desasignar
            if (!solicitud.GestorAsignadoId.HasValue)
                throw new BusinessException("La solicitud no tiene un gestor asignado");

            var gestorAnterior = solicitud.GestorAsignado?.Nombre ?? "Desconocido";
            var estadoAnterior = solicitud.Estado;

            // Limpiar el gestor asignado
            solicitud.GestorAsignadoId = null;

            // Cambiar estado a Nueva si no está ya en ese estado
            if (solicitud.Estado != EstadoSolicitudEnum.Nueva)
            {
                solicitud.Estado = EstadoSolicitudEnum.Nueva;
                await RegistrarHistorialAsync(solicitud.Id, usuarioId, estadoAnterior, EstadoSolicitudEnum.Nueva,
                    $"Gestor {gestorAnterior} desasignado. Estado cambiado a Nueva");
            }
            else
            {
                await RegistrarHistorialAsync(solicitud.Id, usuarioId, estadoAnterior, estadoAnterior,
                    $"Gestor {gestorAnterior} desasignado");
            }

            await _context.SaveChangesAsync();

            // Recargar sin el gestor asignado
            await _context.Entry(solicitud).Reference(s => s.GestorAsignado).LoadAsync();

            _logger.LogInformation("Solicitud {SolicitudId} desasignada. Gestor anterior: {GestorAnterior}",
                solicitudId, gestorAnterior);

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

            // Validar que se proporcione motivo si el estado es Rechazada
            if (nuevoEstado == EstadoSolicitudEnum.Rechazada && string.IsNullOrWhiteSpace(dto.MotivoRechazo))
            {
                throw new BusinessException("Debes proporcionar un motivo para rechazar la solicitud");
            }

            var estadoAnterior = solicitud.Estado;
            solicitud.Estado = nuevoEstado;

            // Guardar motivo de rechazo si el estado es Rechazada
            if (nuevoEstado == EstadoSolicitudEnum.Rechazada)
            {
                solicitud.MotivoRechazo = dto.MotivoRechazo;
                solicitud.FechaCierre = DateTime.Now;
                _logger.LogInformation($"Guardando MotivoRechazo: '{dto.MotivoRechazo}' para solicitud {solicitud.Id}");
            }
            else
            {
                // Limpiar motivo si cambia a otro estado
                solicitud.MotivoRechazo = null;
            }

            // Marcar explícitamente como modificado
            _context.Entry(solicitud).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            
            await RegistrarHistorialAsync(solicitud.Id, agenteId, estadoAnterior, nuevoEstado, dto.MotivoRechazo);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Cambios guardados. Solicitud {solicitud.Id} - Estado: {solicitud.Estado}, MotivoRechazo: '{solicitud.MotivoRechazo}'");

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
            // Nueva → EnProceso, Resuelta, Rechazada o Cancelada
            // EnProceso → Resuelta, Rechazada o Cancelada  
            // Resuelta → EnProceso (reabrir), Rechazada, Cerrada o Cancelada
            // Cancelada ↔ Rechazada (pueden corregir entre estados cerrados)
            // Cerrada → Sin cambios (estado final inmutable para agentes)
            // Los administradores NO pasan por esta validación (pueden hacer cualquier transición)
            return estadoActual switch
            {
                EstadoSolicitudEnum.Nueva => estadoNuevo == EstadoSolicitudEnum.EnProceso || 
                                              estadoNuevo == EstadoSolicitudEnum.Resuelta ||
                                              estadoNuevo == EstadoSolicitudEnum.Rechazada ||
                                              estadoNuevo == EstadoSolicitudEnum.Cancelada,
                EstadoSolicitudEnum.EnProceso => estadoNuevo == EstadoSolicitudEnum.Resuelta ||
                                                   estadoNuevo == EstadoSolicitudEnum.Rechazada ||
                                                   estadoNuevo == EstadoSolicitudEnum.Cancelada,
                EstadoSolicitudEnum.Resuelta => estadoNuevo == EstadoSolicitudEnum.EnProceso ||
                                                 estadoNuevo == EstadoSolicitudEnum.Rechazada ||
                                                 estadoNuevo == EstadoSolicitudEnum.Cerrada ||
                                                 estadoNuevo == EstadoSolicitudEnum.Cancelada,
                EstadoSolicitudEnum.Cancelada => estadoNuevo == EstadoSolicitudEnum.Rechazada,
                EstadoSolicitudEnum.Rechazada => estadoNuevo == EstadoSolicitudEnum.Cancelada,
                EstadoSolicitudEnum.Cerrada => false, // Estado final inmutable para agentes
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
                SolicitanteDepartamento = solicitud.Solicitante?.Area?.Nombre,
                SolicitanteRol = (int?)solicitud.Solicitante?.Rol,
                SolicitanteRolNombre = solicitud.Solicitante != null ? ObtenerNombreRol((int)solicitud.Solicitante.Rol) : null,
                
                // Gestor Asignado (opcional) con ID y email
                GestorAsignadoId = solicitud.GestorAsignadoId,
                GestorAsignado = solicitud.GestorAsignado?.Nombre,
                GestorAsignadoEmail = solicitud.GestorAsignado?.Email,
                
                // Fechas
                FechaCreacion = solicitud.FechaCreacion,
                FechaCierre = solicitud.FechaCierre,
                
                // Campos adicionales
                MotivoRechazo = solicitud.MotivoRechazo,
                
                // Archivo adjunto
                Archivo = !string.IsNullOrEmpty(solicitud.ArchivoNombre) ? new Application.DTOs.Solicitudes.ArchivoAdjuntoDto
                {
                    NombreArchivo = solicitud.ArchivoNombre,
                    ContentType = solicitud.ArchivoContentType,
                    TamanoBytes = null // No tenemos el tamaño guardado en BD
                } : null,
                
                // Comentarios
                Comentarios = solicitud.Comentarios?.Select(c => new Application.DTOs.Comentarios.ComentarioDto
                {
                    Id = c.Id,
                    Contenido = c.Texto,
                    FechaCreacion = c.FechaCreacion,
                    UsuarioId = c.UsuarioId,
                    NombreUsuario = c.Usuario?.Nombre ?? string.Empty,
                    UsuarioRol = (int?)c.Usuario?.Rol,
                    UsuarioRolNombre = c.Usuario != null ? ObtenerNombreRol((int)c.Usuario.Rol) : null,
                    UsuarioDepartamento = c.Usuario?.Area?.Nombre
                }).OrderBy(c => c.FechaCreacion).ToList() ?? new List<Application.DTOs.Comentarios.ComentarioDto>()
            };
        }

        public async Task<SolicitudDto> EditarSolicitudAsync(int solicitudId, EditarSolicitudDto dto, int usuarioId)
        {
            var solicitud = await _context.Solicitudes
                .Include(s => s.Area)
                .Include(s => s.TipoSolicitud)
                .Include(s => s.Solicitante)
                .Include(s => s.GestorAsignado)
                .Include(s => s.Comentarios)
                    .ThenInclude(c => c.Usuario)
                        .ThenInclude(u => u.Area)
                .FirstOrDefaultAsync(s => s.Id == solicitudId);
            
            if (solicitud == null)
                throw new NotFoundException("Solicitud no encontrada");
            
            // VALIDACIÓN 1: Solo el creador puede editar
            if (solicitud.SolicitanteId != usuarioId)
                throw new UnauthorizedActionException("Solo puedes editar tus propias solicitudes");
            
            // VALIDACIÓN 2: Solo si está en estado Nueva
            if (solicitud.Estado != EstadoSolicitudEnum.Nueva)
                throw new BusinessException("Solo puedes editar solicitudes en estado 'Nueva'");
            
            // VALIDACIÓN 3: Solo si no tiene agente asignado
            if (solicitud.GestorAsignadoId.HasValue)
                throw new BusinessException("No puedes editar una solicitud que ya tiene agente asignado");
            
            // Guardar valores anteriores para el comentario de historial
            var cambios = new List<string>();
            
            if (solicitud.Asunto != dto.Asunto)
                cambios.Add($"Asunto: '{solicitud.Asunto}' → '{dto.Asunto}'");
            
            if (solicitud.Descripcion != dto.Descripcion)
                cambios.Add("Descripción modificada");
            
            if ((int)solicitud.Prioridad != dto.Prioridad)
            {
                var prioridadAnterior = solicitud.Prioridad == PrioridadEnum.Baja ? "Baja" : 
                                       solicitud.Prioridad == PrioridadEnum.Media ? "Media" : "Alta";
                var prioridadNueva = dto.Prioridad == 1 ? "Baja" : dto.Prioridad == 2 ? "Media" : "Alta";
                cambios.Add($"Prioridad: {prioridadAnterior} → {prioridadNueva}");
            }
            
            // Actualizar campos
            solicitud.Asunto = dto.Asunto;
            solicitud.Descripcion = dto.Descripcion;
            solicitud.Prioridad = (PrioridadEnum)dto.Prioridad;
            
            // Manejar archivo adjunto
            if (dto.EliminarArchivo && !string.IsNullOrEmpty(solicitud.ArchivoRuta))
            {
                // Eliminar archivo físico
                var archivoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", solicitud.ArchivoRuta.TrimStart('/').Replace("/", "\\"));
                if (File.Exists(archivoPath))
                {
                    File.Delete(archivoPath);
                    _logger.LogInformation("Archivo eliminado: {ArchivoPath}", archivoPath);
                }
                
                solicitud.ArchivoNombre = null;
                solicitud.ArchivoRuta = null;
                solicitud.ArchivoContentType = null;
                cambios.Add("Archivo adjunto eliminado");
            }
            else if (dto.Archivo != null && dto.Archivo.Length > 0)
            {
                // Eliminar archivo anterior si existe
                if (!string.IsNullOrEmpty(solicitud.ArchivoRuta))
                {
                    var archivoAnterior = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", solicitud.ArchivoRuta.TrimStart('/').Replace("/", "\\"));
                    if (File.Exists(archivoAnterior))
                    {
                        File.Delete(archivoAnterior);
                        _logger.LogInformation("Archivo anterior eliminado: {ArchivoPath}", archivoAnterior);
                    }
                }
                
                // Guardar nuevo archivo
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadPath);
                
                var fileName = $"{Guid.NewGuid()}_{dto.Archivo.FileName}";
                var filePath = Path.Combine(uploadPath, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.Archivo.CopyToAsync(stream);
                }
                
                solicitud.ArchivoNombre = dto.Archivo.FileName;
                solicitud.ArchivoRuta = $"/uploads/{fileName}";
                solicitud.ArchivoContentType = dto.Archivo.ContentType;
                
                _logger.LogInformation("Nuevo archivo guardado: {FileName}", fileName);
                cambios.Add($"Archivo actualizado: {dto.Archivo.FileName}");
            }
            
            // Agregar comentario de historial si hubo cambios
            if (cambios.Any())
            {
                var comentario = new Comentario
                {
                    SolicitudId = solicitudId,
                    UsuarioId = usuarioId,
                    Texto = $"📝 Solicitud editada:\n{string.Join("\n", cambios)}",
                    FechaCreacion = DateTime.Now
                };
                
                _context.Comentarios.Add(comentario);
                _logger.LogInformation("Comentario de edición agregado para solicitud {SolicitudId}", solicitudId);
            }
            
            _context.Entry(solicitud).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Solicitud {SolicitudId} editada exitosamente", solicitudId);
            
            // Devolver DTO actualizado
            return MapToDto(solicitud);
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
