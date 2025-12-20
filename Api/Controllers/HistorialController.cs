using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class HistorialController : ControllerBase
    {
        private readonly IHistorialService _historialService;

        public HistorialController(IHistorialService historialService)
        {
            _historialService = historialService;
        }

        /// <summary>
        /// Obtener historial de estados de una solicitud
        /// </summary>
        [HttpGet("{solicitudId}")]
        [Authorize(Roles = "Usuario,AgenteArea,Administrador,SuperAdministrador")]
        public async Task<IActionResult> ObtenerHistorial(int solicitudId)
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var rol = User.FindFirstValue(ClaimTypes.Role)!;
            var areaIdClaim = User.FindFirstValue("AreaId");
            int? areaId = areaIdClaim != null ? int.Parse(areaIdClaim) : null;

            var historial = await _historialService.ObtenerHistorialAsync(solicitudId, usuarioId, rol, areaId);
            
            return Ok(historial);
        }
    }
}
