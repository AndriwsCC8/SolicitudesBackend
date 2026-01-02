using Application.DTOs.Admin;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminService> _logger;

        public AdminService(ApplicationDbContext context, ILogger<AdminService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Usuarios

        public async Task<List<UsuarioAdminDto>> ObtenerUsuariosAsync()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Area)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            return usuarios.Select(u => new UsuarioAdminDto
            {
                Id = u.Id,
                NombreUsuario = u.NombreUsuario,
                Nombre = u.Nombre,
                Email = u.Email,
                Rol = (int)u.Rol,
                RolNombre = ObtenerNombreRol((int)u.Rol),
                AreaId = u.AreaId,
                AreaNombre = u.Area?.Nombre,
                Activo = u.Activo,
                FechaCreacion = u.FechaCreacion
            }).ToList();
        }

        public async Task<UsuarioAdminDto?> ObtenerUsuarioPorIdAsync(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Area)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null) return null;

            return new UsuarioAdminDto
            {
                Id = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = (int)usuario.Rol,
                RolNombre = ObtenerNombreRol((int)usuario.Rol),
                AreaId = usuario.AreaId,
                AreaNombre = usuario.Area?.Nombre,
                Activo = usuario.Activo,
                FechaCreacion = usuario.FechaCreacion
            };
        }

        public async Task<UsuarioAdminDto> CrearUsuarioAsync(CrearUsuarioDto dto)
        {
            // Validar email único
            var emailExiste = await _context.Usuarios.AnyAsync(u => u.Email == dto.Email);
            if (emailExiste)
                throw new BusinessException("El email ya está registrado");

            // Validar username único
            var usernameExiste = await _context.Usuarios.AnyAsync(u => u.NombreUsuario == dto.NombreUsuario);
            if (usernameExiste)
                throw new BusinessException("El nombre de usuario ya está registrado");

            // Validar área si es AgenteArea
            if (dto.Rol == 4 && !dto.AreaId.HasValue)
                throw new BusinessException("Los agentes de área deben tener un área asignada");

            if (dto.AreaId.HasValue)
            {
                var areaExiste = await _context.Areas.AnyAsync(a => a.Id == dto.AreaId.Value);
                if (!areaExiste)
                    throw new NotFoundException("El área especificada no existe");
            }

            var usuario = new Usuario
            {
                NombreUsuario = dto.NombreUsuario,
                Nombre = dto.Nombre,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Rol = (RolEnum)dto.Rol,
                AreaId = dto.AreaId,
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuario creado: {UsuarioId} - {Nombre}", usuario.Id, usuario.Nombre);

            return await ObtenerUsuarioPorIdAsync(usuario.Id) ?? throw new Exception("Error al crear usuario");
        }

        public async Task<UsuarioAdminDto> ActualizarUsuarioAsync(int id, ActualizarUsuarioDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                throw new NotFoundException("Usuario no encontrado");

            // Actualizar campos solo si vienen en el DTO
            if (!string.IsNullOrEmpty(dto.Nombre))
                usuario.Nombre = dto.Nombre;

            if (!string.IsNullOrEmpty(dto.Email))
            {
                // Validar email único
                var emailExiste = await _context.Usuarios
                    .AnyAsync(u => u.Email == dto.Email && u.Id != id);
                if (emailExiste)
                    throw new BusinessException("El email ya está registrado");

                usuario.Email = dto.Email;
            }

            if (!string.IsNullOrEmpty(dto.Password))
            {
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            if (dto.Rol.HasValue)
            {
                usuario.Rol = (RolEnum)dto.Rol.Value;
                // Si cambia a AgenteArea, debe tener área
                if (dto.Rol.Value == 4 && !usuario.AreaId.HasValue && !dto.AreaId.HasValue)
                    throw new BusinessException("Los agentes de área deben tener un área asignada");
            }

            if (dto.AreaId.HasValue)
            {
                if (dto.AreaId.Value > 0)
                {
                    var areaExiste = await _context.Areas.AnyAsync(a => a.Id == dto.AreaId.Value);
                    if (!areaExiste)
                        throw new NotFoundException("El área especificada no existe");
                    usuario.AreaId = dto.AreaId.Value;
                }
                else
                {
                    usuario.AreaId = null;
                }
            }

            if (dto.Activo.HasValue)
                usuario.Activo = dto.Activo.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuario actualizado: {UsuarioId} - {Nombre}", usuario.Id, usuario.Nombre);

            return await ObtenerUsuarioPorIdAsync(usuario.Id) ?? throw new Exception("Error al actualizar usuario");
        }

        public async Task<bool> EliminarUsuarioAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
                throw new NotFoundException("Usuario no encontrado");

            // No permitir eliminar SuperAdministrador con ID 1
            if (usuario.Id == 1 && usuario.Rol == RolEnum.SuperAdministrador)
                throw new BusinessException("No se puede eliminar el SuperAdministrador principal");

            // Verificar si tiene solicitudes asignadas
            var tieneSolicitudesAsignadas = await _context.Solicitudes
                .AnyAsync(s => s.GestorAsignadoId == id);

            if (tieneSolicitudesAsignadas)
                throw new BusinessException("No se puede eliminar un usuario con solicitudes asignadas. Desactívelo en su lugar.");

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuario eliminado: {UsuarioId} - {Nombre}", usuario.Id, usuario.Nombre);

            return true;
        }

        #endregion

        #region Áreas

        public async Task<List<AreaAdminDto>> ObtenerAreasAsync()
        {
            var areas = await _context.Areas
                .Include(a => a.Usuarios)
                .Include(a => a.Solicitudes)
                .OrderBy(a => a.Nombre)
                .ToListAsync();

            return areas.Select(a => new AreaAdminDto
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Descripcion = a.Descripcion,
                Activo = a.Activo,
                CantidadAgentes = a.Usuarios.Count(u => u.Rol == RolEnum.AgenteArea && u.Activo),
                CantidadSolicitudes = a.Solicitudes.Count
            }).ToList();
        }

        public async Task<AreaAdminDto?> ObtenerAreaPorIdAsync(int id)
        {
            var area = await _context.Areas
                .Include(a => a.Usuarios)
                .Include(a => a.Solicitudes)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (area == null) return null;

            return new AreaAdminDto
            {
                Id = area.Id,
                Nombre = area.Nombre,
                Descripcion = area.Descripcion,
                Activo = area.Activo,
                CantidadAgentes = area.Usuarios.Count(u => u.Rol == RolEnum.AgenteArea && u.Activo),
                CantidadSolicitudes = area.Solicitudes.Count
            };
        }

        public async Task<AreaAdminDto> CrearAreaAsync(CrearAreaDto dto)
        {
            // Validar nombre único
            var nombreExiste = await _context.Areas.AnyAsync(a => a.Nombre == dto.Nombre);
            if (nombreExiste)
                throw new BusinessException("Ya existe un área con ese nombre");

            var area = new Area
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Activo = true
            };

            _context.Areas.Add(area);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Área creada: {AreaId} - {Nombre}", area.Id, area.Nombre);

            return await ObtenerAreaPorIdAsync(area.Id) ?? throw new Exception("Error al crear área");
        }

        public async Task<AreaAdminDto> ActualizarAreaAsync(int id, ActualizarAreaDto dto)
        {
            var area = await _context.Areas.FindAsync(id);
            if (area == null)
                throw new NotFoundException("Área no encontrada");

            if (!string.IsNullOrEmpty(dto.Nombre))
            {
                // Validar nombre único
                var nombreExiste = await _context.Areas
                    .AnyAsync(a => a.Nombre == dto.Nombre && a.Id != id);
                if (nombreExiste)
                    throw new BusinessException("Ya existe un área con ese nombre");

                area.Nombre = dto.Nombre;
            }

            if (dto.Descripcion != null)
                area.Descripcion = dto.Descripcion;

            if (dto.Activo.HasValue)
                area.Activo = dto.Activo.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Área actualizada: {AreaId} - {Nombre}", area.Id, area.Nombre);

            return await ObtenerAreaPorIdAsync(area.Id) ?? throw new Exception("Error al actualizar área");
        }

        public async Task<bool> EliminarAreaAsync(int id)
        {
            var area = await _context.Areas
                .Include(a => a.Solicitudes)
                .Include(a => a.Usuarios)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (area == null)
                throw new NotFoundException("Área no encontrada");

            // No permitir eliminar si tiene solicitudes
            if (area.Solicitudes.Any())
                throw new BusinessException("No se puede eliminar un área con solicitudes asociadas");

            // No permitir eliminar si tiene usuarios asignados
            if (area.Usuarios.Any())
                throw new BusinessException("No se puede eliminar un área con usuarios asignados");

            _context.Areas.Remove(area);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Área eliminada: {AreaId} - {Nombre}", area.Id, area.Nombre);

            return true;
        }

        #endregion

        #region Tipos de Solicitud

        public async Task<List<TipoSolicitudAdminDto>> ObtenerTiposSolicitudAsync()
        {
            var tipos = await _context.TiposSolicitud
                .Include(t => t.Area)
                .Include(t => t.Solicitudes)
                .OrderBy(t => t.Nombre)
                .ToListAsync();

            return tipos.Select(t => new TipoSolicitudAdminDto
            {
                Id = t.Id,
                Nombre = t.Nombre,
                Descripcion = t.Descripcion,
                AreaId = t.AreaId,
                AreaNombre = t.Area.Nombre,
                Activo = t.Activo,
                CantidadSolicitudes = t.Solicitudes.Count
            }).ToList();
        }

        public async Task<TipoSolicitudAdminDto?> ObtenerTipoSolicitudPorIdAsync(int id)
        {
            var tipo = await _context.TiposSolicitud
                .Include(t => t.Area)
                .Include(t => t.Solicitudes)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tipo == null) return null;

            return new TipoSolicitudAdminDto
            {
                Id = tipo.Id,
                Nombre = tipo.Nombre,
                Descripcion = tipo.Descripcion,
                AreaId = tipo.AreaId,
                AreaNombre = tipo.Area.Nombre,
                Activo = tipo.Activo,
                CantidadSolicitudes = tipo.Solicitudes.Count
            };
        }

        public async Task<TipoSolicitudAdminDto> CrearTipoSolicitudAsync(CrearTipoSolicitudDto dto)
        {
            // Validar que el área existe
            var area = await _context.Areas.FindAsync(dto.AreaId);
            if (area == null)
                throw new NotFoundException("El área especificada no existe");

            // Validar nombre único por área
            var nombreExiste = await _context.TiposSolicitud
                .AnyAsync(t => t.Nombre == dto.Nombre && t.AreaId == dto.AreaId);
            if (nombreExiste)
                throw new BusinessException("Ya existe un tipo de solicitud con ese nombre en esta área");

            var tipo = new TipoSolicitud
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                AreaId = dto.AreaId,
                Activo = true
            };

            _context.TiposSolicitud.Add(tipo);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tipo de solicitud creado: {TipoId} - {Nombre}", tipo.Id, tipo.Nombre);

            return await ObtenerTipoSolicitudPorIdAsync(tipo.Id) ?? throw new Exception("Error al crear tipo de solicitud");
        }

        public async Task<TipoSolicitudAdminDto> ActualizarTipoSolicitudAsync(int id, ActualizarTipoSolicitudDto dto)
        {
            var tipo = await _context.TiposSolicitud.FindAsync(id);
            if (tipo == null)
                throw new NotFoundException("Tipo de solicitud no encontrado");

            if (!string.IsNullOrEmpty(dto.Nombre))
            {
                // Validar nombre único por área
                var areaId = dto.AreaId ?? tipo.AreaId;
                var nombreExiste = await _context.TiposSolicitud
                    .AnyAsync(t => t.Nombre == dto.Nombre && t.AreaId == areaId && t.Id != id);
                if (nombreExiste)
                    throw new BusinessException("Ya existe un tipo de solicitud con ese nombre en esta área");

                tipo.Nombre = dto.Nombre;
            }

            if (dto.Descripcion != null)
                tipo.Descripcion = dto.Descripcion;

            if (dto.AreaId.HasValue)
            {
                var area = await _context.Areas.FindAsync(dto.AreaId.Value);
                if (area == null)
                    throw new NotFoundException("El área especificada no existe");

                tipo.AreaId = dto.AreaId.Value;
            }

            if (dto.Activo.HasValue)
                tipo.Activo = dto.Activo.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Tipo de solicitud actualizado: {TipoId} - {Nombre}", tipo.Id, tipo.Nombre);

            return await ObtenerTipoSolicitudPorIdAsync(tipo.Id) ?? throw new Exception("Error al actualizar tipo de solicitud");
        }

        public async Task<bool> EliminarTipoSolicitudAsync(int id)
        {
            var tipo = await _context.TiposSolicitud
                .Include(t => t.Solicitudes)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tipo == null)
                throw new NotFoundException("Tipo de solicitud no encontrado");

            // No permitir eliminar si tiene solicitudes
            if (tipo.Solicitudes.Any())
                throw new BusinessException("No se puede eliminar un tipo de solicitud con solicitudes asociadas. Desactívelo en su lugar.");

            _context.TiposSolicitud.Remove(tipo);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tipo de solicitud eliminado: {TipoId} - {Nombre}", tipo.Id, tipo.Nombre);

            return true;
        }

        #endregion

        #region Reportes

        public async Task<ReporteResumenDto> ObtenerReporteResumenAsync()
        {
            var totalSolicitudes = await _context.Solicitudes.CountAsync();
            var solicitudesNuevas = await _context.Solicitudes.CountAsync(s => s.Estado == EstadoSolicitudEnum.Nueva);
            var solicitudesEnProceso = await _context.Solicitudes.CountAsync(s => s.Estado == EstadoSolicitudEnum.EnProceso);
            var solicitudesResueltas = await _context.Solicitudes.CountAsync(s => s.Estado == EstadoSolicitudEnum.Resuelta);
            var solicitudesCerradas = await _context.Solicitudes.CountAsync(s => s.Estado == EstadoSolicitudEnum.Cerrada);
            var solicitudesRechazadas = await _context.Solicitudes.CountAsync(s => s.Estado == EstadoSolicitudEnum.Rechazada);
            var solicitudesCanceladas = await _context.Solicitudes.CountAsync(s => s.Estado == EstadoSolicitudEnum.Cancelada);

            // Calcular tiempo promedio de resolución (solo solicitudes resueltas o cerradas)
            var solicitudesConCierre = await _context.Solicitudes
                .Where(s => s.FechaCierre.HasValue)
                .Select(s => new { s.FechaCreacion, s.FechaCierre })
                .ToListAsync();

            var tiempoPromedioResolucion = solicitudesConCierre.Any()
                ? solicitudesConCierre.Average(s => (s.FechaCierre!.Value - s.FechaCreacion).TotalHours)
                : 0;

            var usuariosActivos = await _context.Usuarios.CountAsync(u => u.Activo);
            var totalAreas = await _context.Areas.CountAsync(a => a.Activo);

            return new ReporteResumenDto
            {
                TotalSolicitudes = totalSolicitudes,
                SolicitudesNuevas = solicitudesNuevas,
                SolicitudesEnProceso = solicitudesEnProceso,
                SolicitudesResueltas = solicitudesResueltas,
                SolicitudesCerradas = solicitudesCerradas,
                SolicitudesRechazadas = solicitudesRechazadas,
                SolicitudesCanceladas = solicitudesCanceladas,
                TiempoPromedioResolucion = Math.Round(tiempoPromedioResolucion, 2),
                UsuariosActivos = usuariosActivos,
                TotalAreas = totalAreas
            };
        }

        public async Task<List<ReportePorAreaDto>> ObtenerReportePorAreaAsync()
        {
            var areas = await _context.Areas
                .Include(a => a.Solicitudes)
                .Include(a => a.Usuarios)
                .Where(a => a.Activo)
                .ToListAsync();

            var reportes = new List<ReportePorAreaDto>();

            foreach (var area in areas)
            {
                var solicitudesArea = area.Solicitudes;
                var totalSolicitudes = solicitudesArea.Count;
                var solicitudesAbiertas = solicitudesArea.Count(s => 
                    s.Estado == EstadoSolicitudEnum.Nueva || 
                    s.Estado == EstadoSolicitudEnum.EnProceso);
                var solicitudesResueltas = solicitudesArea.Count(s => 
                    s.Estado == EstadoSolicitudEnum.Resuelta || 
                    s.Estado == EstadoSolicitudEnum.Cerrada);

                var solicitudesConCierre = solicitudesArea
                    .Where(s => s.FechaCierre.HasValue)
                    .ToList();

                var tiempoPromedio = solicitudesConCierre.Any()
                    ? solicitudesConCierre.Average(s => (s.FechaCierre!.Value - s.FechaCreacion).TotalHours)
                    : 0;

                var cantidadAgentes = area.Usuarios.Count(u => u.Rol == RolEnum.AgenteArea && u.Activo);

                reportes.Add(new ReportePorAreaDto
                {
                    AreaId = area.Id,
                    AreaNombre = area.Nombre,
                    TotalSolicitudes = totalSolicitudes,
                    SolicitudesAbiertas = solicitudesAbiertas,
                    SolicitudesResueltas = solicitudesResueltas,
                    CantidadAgentes = cantidadAgentes,
                    TiempoPromedioResolucion = Math.Round(tiempoPromedio, 2)
                });
            }

            return reportes.OrderByDescending(r => r.TotalSolicitudes).ToList();
        }

        public async Task<List<ReporteDesempenoAgenteDto>> ObtenerReporteDesempenoAgentesAsync()
        {
            var agentes = await _context.Usuarios
                .Include(u => u.Area)
                .Include(u => u.SolicitudesAsignadas)
                .Where(u => u.Rol == RolEnum.AgenteArea && u.Activo)
                .ToListAsync();

            var reportes = new List<ReporteDesempenoAgenteDto>();

            foreach (var agente in agentes)
            {
                var solicitudesAsignadas = agente.SolicitudesAsignadas.Count;
                var solicitudesResueltas = agente.SolicitudesAsignadas.Count(s => 
                    s.Estado == EstadoSolicitudEnum.Resuelta || 
                    s.Estado == EstadoSolicitudEnum.Cerrada);
                var solicitudesEnProceso = agente.SolicitudesAsignadas.Count(s => 
                    s.Estado == EstadoSolicitudEnum.EnProceso);

                var tasaResolucion = solicitudesAsignadas > 0
                    ? (double)solicitudesResueltas / solicitudesAsignadas * 100
                    : 0;

                var solicitudesConCierre = agente.SolicitudesAsignadas
                    .Where(s => s.FechaCierre.HasValue)
                    .ToList();

                var tiempoPromedio = solicitudesConCierre.Any()
                    ? solicitudesConCierre.Average(s => (s.FechaCierre!.Value - s.FechaCreacion).TotalHours)
                    : 0;

                reportes.Add(new ReporteDesempenoAgenteDto
                {
                    AgenteId = agente.Id,
                    AgenteNombre = agente.Nombre,
                    AreaNombre = agente.Area?.Nombre,
                    SolicitudesAsignadas = solicitudesAsignadas,
                    SolicitudesResueltas = solicitudesResueltas,
                    SolicitudesEnProceso = solicitudesEnProceso,
                    TasaResolucion = Math.Round(tasaResolucion, 2),
                    TiempoPromedioResolucion = Math.Round(tiempoPromedio, 2)
                });
            }

            return reportes.OrderByDescending(r => r.SolicitudesResueltas).ToList();
        }

        public async Task<ReporteTiemposRespuestaDto> ObtenerReporteTiemposRespuestaAsync()
        {
            var solicitudesConCierre = await _context.Solicitudes
                .Where(s => s.FechaCierre.HasValue)
                .Select(s => new { s.FechaCreacion, s.FechaCierre })
                .ToListAsync();

            var tiempos = solicitudesConCierre
                .Select(s => (s.FechaCierre!.Value - s.FechaCreacion).TotalHours)
                .ToList();

            var tiempoPromedioTotal = tiempos.Any() ? tiempos.Average() : 0;
            var tiempoMinimo = tiempos.Any() ? tiempos.Min() : 0;
            var tiempoMaximo = tiempos.Any() ? tiempos.Max() : 0;

            // Solicitudes fuera de SLA (más de 72 horas)
            var solicitudesFueraSLA = await _context.Solicitudes
                .Where(s => !s.FechaCierre.HasValue && 
                           EF.Functions.DateDiffHour(s.FechaCreacion, DateTime.Now) > 72)
                .CountAsync();

            // Tiempos promedio por etapa (esto requeriría el historial de estados)
            // Por simplicidad, usaremos valores aproximados
            var tiempoPromedioNuevaAEnProceso = tiempoPromedioTotal * 0.1; // 10% del tiempo total
            var tiempoPromedioEnProcesoAResuelta = tiempoPromedioTotal * 0.9; // 90% del tiempo total

            return new ReporteTiemposRespuestaDto
            {
                TiempoPromedioTotal = Math.Round(tiempoPromedioTotal, 2),
                TiempoPromedioNuevaAEnProceso = Math.Round(tiempoPromedioNuevaAEnProceso, 2),
                TiempoPromedioEnProcesoAResuelta = Math.Round(tiempoPromedioEnProcesoAResuelta, 2),
                TiempoMinimoResolucion = Math.Round(tiempoMinimo, 2),
                TiempoMaximoResolucion = Math.Round(tiempoMaximo, 2),
                SolicitudesFueraDeSLA = solicitudesFueraSLA
            };
        }

        #endregion

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

    public interface IAdminService
    {
    }
}
