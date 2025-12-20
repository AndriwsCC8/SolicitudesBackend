using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Area)
                .FirstOrDefaultAsync(u => u.NombreUsuario == request.NombreUsuario && u.Activo);

            if (usuario == null)
                return null;

            // Verificar contraseña (implementar BCrypt en producción)
            if (!VerifyPassword(request.Password, usuario.PasswordHash))
                return null;

            var token = GenerateJwtToken(usuario.Id, usuario.NombreUsuario, usuario.Rol.ToString(), usuario.AreaId);

            return new LoginResponseDto
            {
                Token = token,
                UsuarioId = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Rol = usuario.Rol,
                AreaId = usuario.AreaId,
                AreaNombre = usuario.Area?.Nombre
            };
        }

        public string GenerateJwtToken(int userId, string nombreUsuario, string rol, int? areaId)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured")));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, nombreUsuario),
                new Claim(ClaimTypes.Role, rol)
            };

            if (areaId.HasValue)
            {
                claims.Add(new Claim("AreaId", areaId.Value.ToString()));
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpirationMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            // TEMPORAL: comparación simple
            // EN PRODUCCIÓN: usar BCrypt.Net-Next
            // return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            
            // Para desarrollo, aceptamos contraseñas simples
            return password == "Admin123!" || password == "Gestor123!" || password == "User123!";
        }
    }
}