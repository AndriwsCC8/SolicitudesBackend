using Application.DTOs.Usuarios;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdministrador")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        /// <summary>
        /// Obtener todos los usuarios (Solo SuperAdministrador)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var usuarios = await _usuarioService.ObtenerTodosAsync();
            return Ok(usuarios);
        }

        /// <summary>
        /// Obtener usuario por ID (Solo SuperAdministrador)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var usuario = await _usuarioService.ObtenerPorIdAsync(id);
            return Ok(usuario);
        }

        /// <summary>
        /// Crear un nuevo usuario (Solo SuperAdministrador)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearUsuarioDto dto)
        {
            await _usuarioService.CrearAsync(dto);
            return Ok(new { message = "Usuario creado exitosamente" });
        }

        /// <summary>
        /// Actualizar un usuario existente (Solo SuperAdministrador)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarUsuarioDto dto)
        {
            await _usuarioService.ActualizarAsync(id, dto);
            return Ok(new { message = "Usuario actualizado exitosamente" });
        }

        /// <summary>
        /// Resetear contraseña de un usuario (Solo SuperAdministrador)
        /// </summary>
        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
        {
            await _usuarioService.ResetPasswordAsync(id, dto);
            return Ok(new { message = "Contraseña reseteada exitosamente" });
        }
    }
}
