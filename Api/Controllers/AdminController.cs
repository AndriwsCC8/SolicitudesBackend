using Application.DTOs.Admin;
using Application.Interfaces;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        #region Usuarios (Solo SuperAdministrador)

        [HttpGet("usuarios")]
        [Authorize(Roles = "SuperAdministrador")]
        public async Task<ActionResult<List<UsuarioAdminDto>>> ObtenerUsuarios()
        {
            try
            {
                var usuarios = await _adminService.ObtenerUsuariosAsync();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return StatusCode(500, new { mensaje = "Error al obtener usuarios" });
            }
        }

        [HttpGet("usuarios/{id}")]
        [Authorize(Roles = "SuperAdministrador")]
        public async Task<ActionResult<UsuarioAdminDto>> ObtenerUsuarioPorId(int id)
        {
            try
            {
                var usuario = await _adminService.ObtenerUsuarioPorIdAsync(id);
                if (usuario == null)
                    return NotFound(new { mensaje = "Usuario no encontrado" });

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario {UsuarioId}", id);
                return StatusCode(500, new { mensaje = "Error al obtener usuario" });
            }
        }

        [HttpPost("usuarios")]
        [Authorize(Roles = "SuperAdministrador")]
        public async Task<ActionResult<UsuarioAdminDto>> CrearUsuario([FromBody] CrearUsuarioDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var usuario = await _adminService.CrearUsuarioAsync(dto);
                return CreatedAtAction(nameof(ObtenerUsuarioPorId), new { id = usuario.Id }, usuario);
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                return StatusCode(500, new { mensaje = "Error al crear usuario" });
            }
        }

        [HttpPut("usuarios/{id}")]
        [Authorize(Roles = "SuperAdministrador")]
        public async Task<ActionResult<UsuarioAdminDto>> ActualizarUsuario(int id, [FromBody] ActualizarUsuarioDto dto)
        {
            try
            {
                var usuario = await _adminService.ActualizarUsuarioAsync(id, dto);
                return Ok(usuario);
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario {UsuarioId}", id);
                return StatusCode(500, new { mensaje = "Error al actualizar usuario" });
            }
        }

        [HttpDelete("usuarios/{id}")]
        [Authorize(Roles = "SuperAdministrador")]
        public async Task<ActionResult> EliminarUsuario(int id)
        {
            try
            {
                await _adminService.EliminarUsuarioAsync(id);
                return Ok(new { mensaje = "Usuario eliminado correctamente" });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario {UsuarioId}", id);
                return StatusCode(500, new { mensaje = "Error al eliminar usuario" });
            }
        }

        #endregion

        #region Áreas (Administrador y SuperAdministrador)

        [HttpGet("areas")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<ActionResult<List<AreaAdminDto>>> ObtenerAreas()
        {
            try
            {
                var areas = await _adminService.ObtenerAreasAsync();
                return Ok(areas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener áreas");
                return StatusCode(500, new { mensaje = "Error al obtener áreas" });
            }
        }

        [HttpGet("areas/{id}")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<ActionResult<AreaAdminDto>> ObtenerAreaPorId(int id)
        {
            try
            {
                var area = await _adminService.ObtenerAreaPorIdAsync(id);
                if (area == null)
                    return NotFound(new { mensaje = "Área no encontrada" });

                return Ok(area);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener área {AreaId}", id);
                return StatusCode(500, new { mensaje = "Error al obtener área" });
            }
        }

        [HttpPost("areas")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<ActionResult<AreaAdminDto>> CrearArea([FromBody] CrearAreaDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var area = await _adminService.CrearAreaAsync(dto);
                return CreatedAtAction(nameof(ObtenerAreaPorId), new { id = area.Id }, area);
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear área");
                return StatusCode(500, new { mensaje = "Error al crear área" });
            }
        }

        [HttpPut("areas/{id}")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<ActionResult<AreaAdminDto>> ActualizarArea(int id, [FromBody] ActualizarAreaDto dto)
        {
            try
            {
                var area = await _adminService.ActualizarAreaAsync(id, dto);
                return Ok(area);
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar área {AreaId}", id);
                return StatusCode(500, new { mensaje = "Error al actualizar área" });
            }
        }

        [HttpDelete("areas/{id}")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<ActionResult> EliminarArea(int id)
        {
            try
            {
                await _adminService.EliminarAreaAsync(id);
                return Ok(new { mensaje = "Área eliminada correctamente" });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar área {AreaId}", id);
                return StatusCode(500, new { mensaje = "Error al eliminar área" });
            }
        }

        #endregion

        #region Categorías/Tipos de Solicitud (Administrador y SuperAdministrador)

        [HttpGet("categorias")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<ActionResult<List<TipoSolicitudAdminDto>>> ObtenerCategorias()
        {
            try
            {
                var categorias = await _adminService.ObtenerTiposSolicitudAsync();
                return Ok(categorias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías");
                return StatusCode(500, new { mensaje = "Error al obtener categorías" });
            }
        }

        [HttpGet("categorias/{id}")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<ActionResult<TipoSolicitudAdminDto>> ObtenerCategoriaPorId(int id)
        {
            try
            {
                var categoria = await _adminService.ObtenerTipoSolicitudPorIdAsync(id);
                if (categoria == null)
                    return NotFound(new { mensaje = "Categoría no encontrada" });

                return Ok(categoria);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categoría {CategoriaId}", id);
                return StatusCode(500, new { mensaje = "Error al obtener categoría" });
            }
        }

        [HttpPost("categorias")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<ActionResult<TipoSolicitudAdminDto>> CrearCategoria([FromBody] CrearTipoSolicitudDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var categoria = await _adminService.CrearTipoSolicitudAsync(dto);
                return CreatedAtAction(nameof(ObtenerCategoriaPorId), new { id = categoria.Id }, categoria);
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear categoría");
                return StatusCode(500, new { mensaje = "Error al crear categoría" });
            }
        }

        [HttpPut("categorias/{id}")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<ActionResult<TipoSolicitudAdminDto>> ActualizarCategoria(int id, [FromBody] ActualizarTipoSolicitudDto dto)
        {
            try
            {
                var categoria = await _adminService.ActualizarTipoSolicitudAsync(id, dto);
                return Ok(categoria);
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar categoría {CategoriaId}", id);
                return StatusCode(500, new { mensaje = "Error al actualizar categoría" });
            }
        }

        [HttpDelete("categorias/{id}")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<ActionResult> EliminarCategoria(int id)
        {
            try
            {
                await _adminService.EliminarTipoSolicitudAsync(id);
                return Ok(new { mensaje = "Categoría eliminada correctamente" });
            }
            catch (BusinessException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar categoría {CategoriaId}", id);
                return StatusCode(500, new { mensaje = "Error al eliminar categoría" });
            }
        }

        #endregion

        #region Reportes (Administrador y SuperAdministrador)

        [HttpGet("reportes/resumen")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<ActionResult<ReporteResumenDto>> ObtenerReporteResumen()
        {
            try
            {
                var reporte = await _adminService.ObtenerReporteResumenAsync();
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reporte resumen");
                return StatusCode(500, new { mensaje = "Error al obtener reporte resumen" });
            }
        }

        [HttpGet("reportes/por-area")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<ActionResult<List<ReportePorAreaDto>>> ObtenerReportePorArea()
        {
            try
            {
                var reporte = await _adminService.ObtenerReportePorAreaAsync();
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reporte por área");
                return StatusCode(500, new { mensaje = "Error al obtener reporte por área" });
            }
        }

        [HttpGet("reportes/desempeno-agentes")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<ActionResult<List<ReporteDesempenoAgenteDto>>> ObtenerReporteDesempenoAgentes()
        {
            try
            {
                var reporte = await _adminService.ObtenerReporteDesempenoAgentesAsync();
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reporte de desempeño de agentes");
                return StatusCode(500, new { mensaje = "Error al obtener reporte de desempeño de agentes" });
            }
        }

        [HttpGet("reportes/tiempos-respuesta")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<ActionResult<ReporteTiemposRespuestaDto>> ObtenerReporteTiemposRespuesta()
        {
            try
            {
                var reporte = await _adminService.ObtenerReporteTiemposRespuestaAsync();
                return Ok(reporte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener reporte de tiempos de respuesta");
                return StatusCode(500, new { mensaje = "Error al obtener reporte de tiempos de respuesta" });
            }
        }

        #endregion
    }
}
