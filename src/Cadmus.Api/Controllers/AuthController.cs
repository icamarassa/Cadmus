using Microsoft.AspNetCore.Mvc;
using Cadmus.Api.Models;
using Cadmus.Api.Security;
using Cadmus.Api.Services;

namespace Cadmus.Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly LdapAuthService _ldapAuth;
        private readonly JwtService _jwtService;

        public AuthController(LdapAuthService ldapAuth, JwtService jwtService)
        {
            _ldapAuth = ldapAuth;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (!_ldapAuth.ValidateCredentials(request.Username, request.Password, out var groups))
                return Unauthorized(new { message = "Credenciais inválidas" });

            var token = _jwtService.GenerateToken(request.Username, groups);
            return Ok(new { token, username = request.Username, groups });
        }
    }
}