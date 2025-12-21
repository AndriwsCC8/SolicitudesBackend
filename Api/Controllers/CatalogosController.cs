using Application.DTOs.Catalogs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogosController : ControllerBase
    {
        private readonly ICatalogService _catalogService;

        public CatalogosController(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        #region Áreas

        /// <summary>
        /// Obtener todas las áreas
        /// </summary>
        [HttpGet("areas")]
        [Authorize]
        public async Task<IActionResult> GetAreas()
        {
            var areas = await _catalogService.GetAreasAsync();
            return Ok(areas);
        }

        /// <summary>
        /// Obtener un área por ID
        /// </summary>
        [HttpGet("areas/{id}")]
        [Authorize]
        public async Task<IActionResult> GetAreaById(int id)
        {
            var area = await _catalogService.GetAreaByIdAsync(id);
            if (area == null)
                return NotFound();
            
            return Ok(area);
        }

        /// <summary>
        /// Crear una nueva área (Solo administradores)
        /// </summary>
        [HttpPost("areas")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<IActionResult> CreateArea([FromBody] CreateAreaDto dto)
        {
            var area = await _catalogService.CreateAreaAsync(dto);
            return CreatedAtAction(nameof(GetAreaById), new { id = area.Id }, area);
        }

        /// <summary>
        /// Actualizar un área (Solo administradores)
        /// </summary>
        [HttpPut("areas/{id}")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<IActionResult> UpdateArea(int id, [FromBody] UpdateAreaDto dto)
        {
            var area = await _catalogService.UpdateAreaAsync(id, dto);
            if (area == null)
                return NotFound();
            
            return Ok(area);
        }

        /// <summary>
        /// Eliminar un área (Solo administradores)
        /// </summary>
        [HttpDelete("areas/{id}")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<IActionResult> DeleteArea(int id)
        {
            var result = await _catalogService.DeleteAreaAsync(id);
            if (!result)
                return NotFound();
            
            return NoContent();
        }

        #endregion

        #region Tipos de Solicitud

        /// <summary>
        /// Obtener todos los tipos de solicitud
        /// </summary>
        [HttpGet("tipos-solicitud")]
        [Authorize]
        public async Task<IActionResult> GetTiposSolicitud()
        {
            var tipos = await _catalogService.GetTiposSolicitudAsync();
            return Ok(tipos);
        }

        /// <summary>
        /// Obtener tipos de solicitud por área
        /// </summary>
        [HttpGet("tipos-solicitud/area/{areaId}")]
        [Authorize]
        public async Task<IActionResult> GetTiposSolicitudByArea(int areaId)
        {
            var tipos = await _catalogService.GetTiposSolicitudByAreaAsync(areaId);
            return Ok(tipos);
        }

        /// <summary>
        /// Obtener un tipo de solicitud por ID
        /// </summary>
        [HttpGet("tipos-solicitud/{id}")]
        [Authorize]
        public async Task<IActionResult> GetTipoSolicitudById(int id)
        {
            var tipo = await _catalogService.GetTipoSolicitudByIdAsync(id);
            if (tipo == null)
                return NotFound();
            
            return Ok(tipo);
        }

        /// <summary>
        /// Crear un nuevo tipo de solicitud (Solo administradores)
        /// </summary>
        [HttpPost("tipos-solicitud")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<IActionResult> CreateTipoSolicitud([FromBody] CreateTipoSolicitudDto dto)
        {
            var tipo = await _catalogService.CreateTipoSolicitudAsync(dto);
            return CreatedAtAction(nameof(GetTipoSolicitudById), new { id = tipo.Id }, tipo);
        }

        /// <summary>
        /// Actualizar un tipo de solicitud (Solo administradores)
        /// </summary>
        [HttpPut("tipos-solicitud/{id}")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<IActionResult> UpdateTipoSolicitud(int id, [FromBody] UpdateTipoSolicitudDto dto)
        {
            var tipo = await _catalogService.UpdateTipoSolicitudAsync(id, dto);
            if (tipo == null)
                return NotFound();
            
            return Ok(tipo);
        }

        /// <summary>
        /// Eliminar un tipo de solicitud (Solo administradores)
        /// </summary>
        [HttpDelete("tipos-solicitud/{id}")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<IActionResult> DeleteTipoSolicitud(int id)
        {
            var result = await _catalogService.DeleteTipoSolicitudAsync(id);
            if (!result)
                return NotFound();
            
            return NoContent();
        }

        #endregion

        #region Prioridades

        /// <summary>
        /// Obtener todas las prioridades (Enum)
        /// </summary>
        [HttpGet("prioridades")]
        [Authorize]
        public async Task<IActionResult> GetPrioridades()
        {
            var prioridades = await _catalogService.GetPrioridadesAsync();
            return Ok(prioridades);
        }

        #endregion
    }
}
