using Application.DTOs.Solicitudes;
using Application.DTOs.Comentarios;
using Application.Interfaces;
using Infrastructure.Services;
using Infrastructure.Data;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SolicitudesController : ControllerBase
    {
        private readonly ISolicitudService _solicitudService;
        private readonly ILogger<SolicitudesController> _logger;
        private readonly IPdfExportService _pdfExportService;
        private readonly ApplicationDbContext _context;

        public SolicitudesController(
            ISolicitudService solicitudService, 
            ILogger<SolicitudesController> logger,
            IPdfExportService pdfExportService,
            ApplicationDbContext context)
        {
            _solicitudService = solicitudService;
            _logger = logger;
            _pdfExportService = pdfExportService;
            _context = context;
        }

        /// <summary>
        /// Crear una nueva solicitud (Todos los roles autenticados)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CrearSolicitud([FromBody] CrearSolicitudDto dto)
        {
            try
            {
                _logger.LogInformation("Iniciando creación de solicitud. DTO: {@Dto}", dto);

                // Validar que el DTO no sea nulo
                if (dto == null)
                {
                    _logger.LogWarning("DTO recibido es null");
                    return BadRequest(new { message = "Los datos de la solicitud son requeridos" });
                }

                // Validar campos requeridos
                if (dto.TipoSolicitudId <= 0)
                {
                    _logger.LogWarning("TipoSolicitudId inválido: {TipoSolicitudId}", dto.TipoSolicitudId);
                    return BadRequest(new { message = "El tipo de solicitud es requerido" });
                }

                if (string.IsNullOrWhiteSpace(dto.Asunto))
                {
                    _logger.LogWarning("Asunto vacío o nulo");
                    return BadRequest(new { message = "El asunto es requerido" });
                }

                if (string.IsNullOrWhiteSpace(dto.Descripcion))
                {
                    _logger.LogWarning("Descripción vacía o nula");
                    return BadRequest(new { message = "La descripción es requerida" });
                }

                // Obtener usuarioId del token
                var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(usuarioIdClaim))
                {
                    _logger.LogError("No se pudo obtener el UsuarioId del token JWT. Claims disponibles: {@Claims}", 
                        User.Claims.Select(c => new { c.Type, c.Value }));
                    return Unauthorized(new { message = "Token inválido: no se pudo identificar el usuario" });
                }

                if (!int.TryParse(usuarioIdClaim, out int usuarioId))
                {
                    _logger.LogError("UsuarioId del token no es un número válido: {UsuarioIdClaim}", usuarioIdClaim);
                    return Unauthorized(new { message = "Token inválido: identificador de usuario incorrecto" });
                }

                _logger.LogInformation("UsuarioId obtenido del token: {UsuarioId}", usuarioId);

                var solicitud = await _solicitudService.CrearAsync(dto, usuarioId);
                
                _logger.LogInformation("Solicitud creada exitosamente. Id: {SolicitudId}, Numero: {Numero}", 
                    solicitud.Id, solicitud.Numero);
                
                return CreatedAtAction(nameof(CrearSolicitud), new { id = solicitud.Id }, solicitud);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear solicitud. DTO: {@Dto}", dto);
                throw; // El middleware de excepciones lo manejará
            }
        }

        /// <summary>
        /// Obtener una solicitud por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                _logger.LogInformation("Obteniendo solicitud por ID: {SolicitudId}", id);

                var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out int usuarioId))
                {
                    return Unauthorized(new { message = "Token inválido" });
                }

                // Obtener el rol del usuario
                var rolClaim = User.FindFirstValue(ClaimTypes.Role);
                _logger.LogInformation("Usuario {UsuarioId} con rol {Rol} solicitando solicitud {SolicitudId}", 
                    usuarioId, rolClaim, id);

                var solicitud = await _solicitudService.ObtenerPorIdAsync(id, usuarioId);

                if (solicitud == null)
                {
                    _logger.LogWarning("Solicitud {SolicitudId} no encontrada o sin permisos para usuario {UsuarioId}", 
                        id, usuarioId);
                    return NotFound(new { message = "Solicitud no encontrada" });
                }

                _logger.LogInformation("Solicitud {SolicitudId} devuelta exitosamente", id);
                return Ok(solicitud);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener solicitud {SolicitudId}", id);
                throw;
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
            var usuarioAreaId = User.FindFirstValue("AreaId");
            if (usuarioAreaId == null || int.Parse(usuarioAreaId) != areaId)
            {
                throw new Domain.Exceptions.UnauthorizedActionException("No tienes permiso para acceder a las solicitudes de esta área");
            }

            var solicitudes = await _solicitudService.ObtenerPorAreaAsync(areaId);
            return Ok(solicitudes);
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

        /// <summary>
        /// Asignar agente a una solicitud (Solo administradores)
        /// </summary>
        [HttpPost("asignar-agente")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<IActionResult> AsignarAgente([FromBody] AsignarAgenteDto dto)
        {
            var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var solicitud = await _solicitudService.AsignarAgenteAsync(dto, adminId);
            return Ok(solicitud);
        }

        /// <summary>
        /// Cambiar estado de una solicitud (Agentes del área asignados y Administradores)
        /// </summary>
        [HttpPut("cambiar-estado")]
        [Authorize(Roles = "AgenteArea,Administrador,SuperAdministrador")]
        public async Task<IActionResult> CambiarEstado([FromBody] CambiarEstadoDto dto)
        {
            var agenteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var solicitud = await _solicitudService.CambiarEstadoAsync(dto, agenteId);
            return Ok(solicitud);
        }

        /// <summary>
        /// Rechazar una solicitud (Solo agentes del área asignados)
        /// </summary>
        [HttpPost("rechazar")]
        [Authorize(Roles = "AgenteArea")]
        public async Task<IActionResult> Rechazar([FromBody] RechazarSolicitudDto dto)
        {
            var agenteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var solicitud = await _solicitudService.RechazarAsync(dto, agenteId);
            return Ok(solicitud);
        }

        /// <summary>
        /// Cerrar una solicitud (Solo el solicitante)
        /// </summary>
        [HttpPost("cerrar")]
        [Authorize(Roles = "Usuario")]
        public async Task<IActionResult> Cerrar([FromBody] CerrarSolicitudDto dto)
        {
            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var solicitud = await _solicitudService.CerrarAsync(dto, usuarioId);
            return Ok(solicitud);
        }

        /// <summary>
        /// Exportar solicitud a PDF (Administradores y SuperAdministrador)
        /// </summary>
        [HttpGet("{id}/export/pdf")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<IActionResult> ExportarPdf(int id)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                _logger.LogInformation("Usuario {UsuarioId} solicitando PDF de solicitud {SolicitudId}", usuarioId, id);

                // Obtener la solicitud completa
                var solicitud = await _solicitudService.ObtenerPorIdAsync(id, usuarioId);

                if (solicitud == null)
                {
                    _logger.LogWarning("Solicitud {SolicitudId} no encontrada para exportar", id);
                    return NotFound(new { message = "Solicitud no encontrada" });
                }

                // Generar el PDF
                var pdfBytes = _pdfExportService.GenerarPdfSolicitud(solicitud);
                var fileName = $"Solicitud_{solicitud.Numero}_{DateTime.Now:yyyyMMdd}.pdf";

                _logger.LogInformation("PDF generado exitosamente para solicitud {SolicitudId}", id);

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar PDF de solicitud {SolicitudId}", id);
                return StatusCode(500, new { message = "Error al generar el PDF", error = ex.Message });
            }
        }

        /// <summary>
        /// Exportar solicitud a PNG (Administradores y SuperAdministrador)
        /// </summary>
        [HttpGet("{id}/export/png")]
        [Authorize(Roles = "Administrador,SuperAdministrador")]
        public async Task<IActionResult> ExportarPng(int id)
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                _logger.LogInformation("Usuario {UsuarioId} solicitando PNG de solicitud {SolicitudId}", usuarioId, id);

                // Obtener la solicitud completa
                var solicitud = await _solicitudService.ObtenerPorIdAsync(id, usuarioId);

                if (solicitud == null)
                {
                    _logger.LogWarning("Solicitud {SolicitudId} no encontrada para exportar", id);
                    return NotFound(new { message = "Solicitud no encontrada" });
                }

                // Generar el PNG
                var pngBytes = _pdfExportService.GenerarPngSolicitud(solicitud);
                var fileName = $"Solicitud_{solicitud.Numero}_{DateTime.Now:yyyyMMdd}.png";

                _logger.LogInformation("PNG generado exitosamente para solicitud {SolicitudId}", id);

                return File(pngBytes, "image/png", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar PNG de solicitud {SolicitudId}", id);
                return StatusCode(500, new { message = "Error al generar la imagen PNG", error = ex.Message });
            }
        }

        /// <summary>
        /// Agregar comentario a una solicitud (Todos los usuarios autenticados)
        /// </summary>
        [HttpPost("{id}/comentarios")]
        [Authorize]
        public async Task<IActionResult> AgregarComentario(int id, [FromBody] AgregarComentarioDto dto)
        {
            try
            {
                _logger.LogInformation("Agregando comentario a solicitud {SolicitudId}", id);

                var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out int usuarioId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                // Validar que la solicitud existe
                var solicitud = await _context.Solicitudes.FindAsync(id);
                if (solicitud == null)
                {
                    _logger.LogWarning("Solicitud {SolicitudId} no encontrada", id);
                    return NotFound(new { message = $"Solicitud {id} no encontrada" });
                }

                // Validar contenido del comentario
                if (string.IsNullOrWhiteSpace(dto.Contenido))
                {
                    return BadRequest(new { message = "El contenido del comentario es requerido" });
                }

                if (dto.Contenido.Length > 1000)
                {
                    return BadRequest(new { message = "El comentario no puede exceder 1000 caracteres" });
                }

                // Crear el comentario
                var comentario = new Comentario
                {
                    SolicitudId = id,
                    UsuarioId = usuarioId,
                    Texto = dto.Contenido,
                    FechaCreacion = DateTime.Now
                };

                _context.Comentarios.Add(comentario);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Comentario {ComentarioId} creado exitosamente para solicitud {SolicitudId}", 
                    comentario.Id, id);

                // Cargar el usuario para devolver en la respuesta
                await _context.Entry(comentario)
                    .Reference(c => c.Usuario)
                    .LoadAsync();

                var comentarioDto = new ComentarioDto
                {
                    Id = comentario.Id,
                    Contenido = comentario.Texto,
                    FechaCreacion = comentario.FechaCreacion,
                    UsuarioId = comentario.UsuarioId,
                    NombreUsuario = comentario.Usuario.Nombre
                };

                return Ok(comentarioDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar comentario a solicitud {SolicitudId}", id);
                return StatusCode(500, new { message = "Error al agregar comentario", error = ex.Message });
            }
        }
    }
}
