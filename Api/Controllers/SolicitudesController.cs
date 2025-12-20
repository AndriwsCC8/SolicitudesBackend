using Application.DTOs.Solicitudes;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SolicitudesController : ControllerBase
    {
        private readonly ISolicitudService _solicitudService;

        public SolicitudesController(ISolicitudService solicitudService)
        {
            _solicitudService = solicitudService;
        }

        /// <summary>
        /// Crear una nueva solicitud (Solo usuarios)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> CrearSolicitud([FromBody] CrearSolicitudDto dto)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var solicitud = await _solicitudService.CrearAsync(dto, usuarioId);
                return Ok(solicitud);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtener mis solicitudes (Solo usuarios)
        /// </summary>
        [HttpGet("mis-solicitudes")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> ObtenerMisSolicitudes()
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var solicitudes = await _solicitudService.ObtenerPorUsuarioAsync(usuarioId);
            return Ok(solicitudes);
        }

        /// <summary>
        /// Obtener solicitudes de un área (Solo agentes de área)
        /// </summary>
        [HttpGet("area/{areaId}")]
        [Authorize(Roles = "AgenteArea")]
        public async Task<IActionResult> ObtenerPorArea(int areaId)
        {
            try
            {
                // Validar que el agente pertenece al área solicitada
                var usuarioAreaId = User.FindFirstValue("AreaId");
                if (usuarioAreaId == null || int.Parse(usuarioAreaId) != areaId)
                {
                    return Forbid();
                }

                var solicitudes = await _solicitudService.ObtenerPorAreaAsync(areaId);
                return Ok(solicitudes);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtener todas las solicitudes (Solo administradores)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<IActionResult> ObtenerTodas()
        {
            var solicitudes = await _solicitudService.ObtenerTodasAsync();
            return Ok(solicitudes);
        }
    }
}
