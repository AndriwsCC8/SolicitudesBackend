using Application.DTOs.Usuarios;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Infrastructure.Data;
using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly ApplicationDbContext _context;

        public UsuarioService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<UsuarioDto>> ObtenerTodosAsync()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Area)
                .OrderBy(u => u.NombreUsuario)
                .Select(u => new UsuarioDto
                {
                    Id = u.Id,
                    NombreUsuario = u.NombreUsuario,
                    Nombre = u.Nombre,
                    Email = u.Email,
                    Rol = u.Rol.ToString(),
                    Area = u.Area != null ? u.Area.Nombre : null,
                    Activo = u.Activo,
                    FechaCreacion = u.FechaCreacion
                })
                .ToListAsync();

            return usuarios;
        }

        public async Task<UsuarioDto> ObtenerPorIdAsync(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Area)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                throw new NotFoundException($"Usuario con ID {id} no encontrado");

            return new UsuarioDto
            {
                Id = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = usuario.Rol.ToString(),
                Area = usuario.Area?.Nombre,
                Activo = usuario.Activo,
                FechaCreacion = usuario.FechaCreacion
            };
        }

        public async Task CrearAsync(CrearUsuarioDto dto)
        {
            // Validar campos requeridos
            if (string.IsNullOrWhiteSpace(dto.NombreUsuario))
                throw new BusinessException("El nombre de usuario es requerido");

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                throw new BusinessException("El nombre es requerido");

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new BusinessException("El email es requerido");

            if (string.IsNullOrWhiteSpace(dto.Password))
                throw new BusinessException("La contraseña es requerida");

            if (string.IsNullOrWhiteSpace(dto.Rol))
                throw new BusinessException("El rol es requerido");

            // Validar que el nombre de usuario sea único
            var existeUsuario = await _context.Usuarios
                .AnyAsync(u => u.NombreUsuario == dto.NombreUsuario);

            if (existeUsuario)
                throw new BusinessException($"Ya existe un usuario con el nombre '{dto.NombreUsuario}'");

            // Validar que el rol sea válido
            if (!Enum.TryParse<RolEnum>(dto.Rol, out var rol))
                throw new BusinessException($"Rol '{dto.Rol}' no es válido. Roles válidos: Usuario, Administrador, SuperAdministrador, AgenteArea");

            // Validar AreaId según el rol
            if (rol == RolEnum.AgenteArea)
            {
                if (!dto.AreaId.HasValue)
                    throw new BusinessException("El rol AgenteArea requiere un área asignada");

                var areaExiste = await _context.Areas.AnyAsync(a => a.Id == dto.AreaId.Value && a.Activo);
                if (!areaExiste)
                    throw new NotFoundException($"Área con ID {dto.AreaId.Value} no encontrada o inactiva");
            }
            else
            {
                // Si no es AgenteArea, el AreaId debe ser null
                dto.AreaId = null;
            }

            // Crear el usuario
            var usuario = new Usuario
            {
                NombreUsuario = dto.NombreUsuario.Trim(),
                Nombre = dto.Nombre.Trim(),
                Email = dto.Email.Trim(),
                PasswordHash = PasswordHasher.Hash(dto.Password),
                Rol = rol,
                AreaId = dto.AreaId,
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarAsync(int id, ActualizarUsuarioDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                throw new NotFoundException($"Usuario con ID {id} no encontrado");

            // Actualizar Nombre
            if (!string.IsNullOrWhiteSpace(dto.Nombre))
                usuario.Nombre = dto.Nombre.Trim();

            // Actualizar Email
            if (!string.IsNullOrWhiteSpace(dto.Email))
                usuario.Email = dto.Email.Trim();

            // Actualizar Rol
            if (!string.IsNullOrWhiteSpace(dto.Rol))
            {
                if (!Enum.TryParse<RolEnum>(dto.Rol, out var nuevoRol))
                    throw new BusinessException($"Rol '{dto.Rol}' no es válido. Roles válidos: Usuario, Administrador, SuperAdministrador, AgenteArea");

                usuario.Rol = nuevoRol;
            }

            // Validar y actualizar AreaId según el rol
            if (usuario.Rol == RolEnum.AgenteArea)
            {
                // Si es AgenteArea, validar que tenga área
                if (dto.AreaId.HasValue)
                {
                    var areaExiste = await _context.Areas.AnyAsync(a => a.Id == dto.AreaId.Value && a.Activo);
                    if (!areaExiste)
                        throw new NotFoundException($"Área con ID {dto.AreaId.Value} no encontrada o inactiva");

                    usuario.AreaId = dto.AreaId.Value;
                }
                else if (usuario.AreaId == null)
                {
                    throw new BusinessException("El rol AgenteArea requiere un área asignada");
                }
            }
            else
            {
                // Si no es AgenteArea, el AreaId debe ser null
                usuario.AreaId = null;
            }

            // Actualizar estado Activo
            if (dto.Activo.HasValue)
                usuario.Activo = dto.Activo.Value;

            await _context.SaveChangesAsync();
        }

        public async Task ResetPasswordAsync(int id, ResetPasswordDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                throw new NotFoundException($"Usuario con ID {id} no encontrado");

            if (string.IsNullOrWhiteSpace(dto.NuevaPassword))
                throw new BusinessException("La nueva contraseña es requerida");

            if (dto.NuevaPassword.Length < 6)
                throw new BusinessException("La contraseña debe tener al menos 6 caracteres");

            usuario.PasswordHash = PasswordHasher.Hash(dto.NuevaPassword);
            await _context.SaveChangesAsync();
        }
    }
}
