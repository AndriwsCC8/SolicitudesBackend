using Application.DTOs.Catalogs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly ApplicationDbContext _context;

        public CatalogService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ÁREAS
        public async Task<List<AreaDto>> GetAreasAsync()
        {
            return await _context.Areas
                .Where(a => a.Activo)
                .Select(a => new AreaDto
                {
                    Id = a.Id,
                    Nombre = a.Nombre,
                    Descripcion = a.Descripcion,
                    Activo = a.Activo
                })
                .ToListAsync();
        }

        public async Task<AreaDto?> GetAreaByIdAsync(int id)
        {
            var area = await _context.Areas.FindAsync(id);
            if (area == null) return null;

            return new AreaDto
            {
                Id = area.Id,
                Nombre = area.Nombre,
                Descripcion = area.Descripcion,
                Activo = area.Activo
            };
        }

        public async Task<AreaDto> CreateAreaAsync(CreateAreaDto dto)
        {
            var area = new Area
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Activo = true
            };

            _context.Areas.Add(area);
            await _context.SaveChangesAsync();

            return new AreaDto
            {
                Id = area.Id,
                Nombre = area.Nombre,
                Descripcion = area.Descripcion,
                Activo = area.Activo
            };
        }

        public async Task<AreaDto?> UpdateAreaAsync(int id, UpdateAreaDto dto)
        {
            var area = await _context.Areas.FindAsync(id);
            if (area == null) return null;

            area.Nombre = dto.Nombre;
            area.Descripcion = dto.Descripcion;
            area.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return new AreaDto
            {
                Id = area.Id,
                Nombre = area.Nombre,
                Descripcion = area.Descripcion,
                Activo = area.Activo
            };
        }

        public async Task<bool> DeleteAreaAsync(int id)
        {
            var area = await _context.Areas.FindAsync(id);
            if (area == null) return false;

            area.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<GestorDto>> GetGestoresPorAreaAsync(int areaId)
        {
            return await _context.Usuarios
                .Include(u => u.Area)
                .Where(u => u.AreaId == areaId && 
                           u.Rol == RolEnum.AgenteArea && 
                           u.Activo)
                .Select(u => new GestorDto
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Email = u.Email,
                    Departamento = u.Area != null ? u.Area.Nombre : null
                })
                .ToListAsync();
        }

        // TIPOS DE SOLICITUD
        public async Task<List<TipoSolicitudDto>> GetTiposSolicitudAsync()
        {
            return await _context.TiposSolicitud
                .Include(t => t.Area)
                .Where(t => t.Activo)
                .Select(t => new TipoSolicitudDto
                {
                    Id = t.Id,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion,
                    AreaId = t.AreaId,
                    AreaNombre = t.Area.Nombre,
                    Activo = t.Activo
                })
                .ToListAsync();
        }

        public async Task<List<TipoSolicitudDto>> GetTiposSolicitudByAreaAsync(int areaId)
        {
            return await _context.TiposSolicitud
                .Include(t => t.Area)
                .Where(t => t.AreaId == areaId && t.Activo)
                .Select(t => new TipoSolicitudDto
                {
                    Id = t.Id,
                    Nombre = t.Nombre,
                    Descripcion = t.Descripcion,
                    AreaId = t.AreaId,
                    AreaNombre = t.Area.Nombre,
                    Activo = t.Activo
                })
                .ToListAsync();
        }

        public async Task<TipoSolicitudDto?> GetTipoSolicitudByIdAsync(int id)
        {
            var tipo = await _context.TiposSolicitud
                .Include(t => t.Area)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tipo == null) return null;

            return new TipoSolicitudDto
            {
                Id = tipo.Id,
                Nombre = tipo.Nombre,
                Descripcion = tipo.Descripcion,
                AreaId = tipo.AreaId,
                AreaNombre = tipo.Area.Nombre,
                Activo = tipo.Activo
            };
        }

        public async Task<TipoSolicitudDto> CreateTipoSolicitudAsync(CreateTipoSolicitudDto dto)
        {
            var tipo = new TipoSolicitud
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                AreaId = dto.AreaId,
                Activo = true
            };

            _context.TiposSolicitud.Add(tipo);
            await _context.SaveChangesAsync();

            var area = await _context.Areas.FindAsync(dto.AreaId);

            return new TipoSolicitudDto
            {
                Id = tipo.Id,
                Nombre = tipo.Nombre,
                Descripcion = tipo.Descripcion,
                AreaId = tipo.AreaId,
                AreaNombre = area?.Nombre ?? string.Empty,
                Activo = tipo.Activo
            };
        }

        public async Task<TipoSolicitudDto?> UpdateTipoSolicitudAsync(int id, UpdateTipoSolicitudDto dto)
        {
            var tipo = await _context.TiposSolicitud
                .Include(t => t.Area)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tipo == null) return null;

            tipo.Nombre = dto.Nombre;
            tipo.Descripcion = dto.Descripcion;
            tipo.AreaId = dto.AreaId;
            tipo.Activo = dto.Activo;

            await _context.SaveChangesAsync();

            return new TipoSolicitudDto
            {
                Id = tipo.Id,
                Nombre = tipo.Nombre,
                Descripcion = tipo.Descripcion,
                AreaId = tipo.AreaId,
                AreaNombre = tipo.Area.Nombre,
                Activo = tipo.Activo
            };
        }

        public async Task<bool> DeleteTipoSolicitudAsync(int id)
        {
            var tipo = await _context.TiposSolicitud.FindAsync(id);
            if (tipo == null) return false;

            tipo.Activo = false;
            await _context.SaveChangesAsync();
            return true;
        }

        // PRIORIDADES
        public async Task<List<PrioridadDto>> GetPrioridadesAsync()
        {
            return Enum.GetValues(typeof(PrioridadEnum))
                .Cast<PrioridadEnum>()
                .Select(p => new PrioridadDto
                {
                    Id = (int)p,
                    Nombre = p.ToString()
                })
                .ToList();
        }
    }
}