using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KanbanRestService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        // POST api/<AuthController>
        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginRequest loginRequest)
        {
            // Dummy authentication:
            // Replace with real user check from secure DB and hashed passwords
            if (loginRequest.Username != "admin" || loginRequest.Password != "admin")
                return Unauthorized();

            string jwt = _createJwtToken(loginRequest.Username);

            return Ok(new { token = jwt });
        }

        private string _createJwtToken(string username)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = _createTokenDescriptor(username);

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            return jwt;
        }

        private SecurityTokenDescriptor _createTokenDescriptor(string username)
        {
            var secretKey = _config["SuperSecretJwtKey"];
            var issuer = _config["Issuer"];
            var audience = _config["Audience"];

            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature);
            var subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username), new Claim(ClaimTypes.Role, "Admin") });

            return new SecurityTokenDescriptor
            {
                Subject = subject,
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = signingCredentials
            };
        }

        public class LoginRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

    }
}
