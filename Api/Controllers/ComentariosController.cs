using Application.DTOs.Comentarios;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ComentariosController : ControllerBase
    {
        private readonly IComentarioService _comentarioService;

        public ComentariosController(IComentarioService comentarioService)
        {
            _comentarioService = comentarioService;
        }

        /// <summary>
        /// Agregar un comentario a una solicitud
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Usuario,AgenteArea,Administrador,SuperAdministrador")]
        public async Task<IActionResult> AgregarComentario([FromBody] AgregarComentarioDto dto)
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var rol = User.FindFirstValue(ClaimTypes.Role)!;
            var areaIdClaim = User.FindFirstValue("AreaId");
            int? areaId = areaIdClaim != null ? int.Parse(areaIdClaim) : null;

            await _comentarioService.AgregarComentarioAsync(dto.SolicitudId, usuarioId, rol, areaId, dto);
            
            return Ok(new { message = "Comentario agregado exitosamente" });
        }

        /// <summary>
        /// Obtener comentarios de una solicitud
        /// </summary>
        [HttpGet("{solicitudId}")]
        [Authorize(Roles = "Usuario,AgenteArea,Administrador,SuperAdministrador")]
        public async Task<IActionResult> ObtenerComentarios(int solicitudId)
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var rol = User.FindFirstValue(ClaimTypes.Role)!;
            var areaIdClaim = User.FindFirstValue("AreaId");
            int? areaId = areaIdClaim != null ? int.Parse(areaIdClaim) : null;

            var comentarios = await _comentarioService.ObtenerPorSolicitudAsync(solicitudId, usuarioId, rol, areaId);
            
            return Ok(comentarios);
        }
    }
}
