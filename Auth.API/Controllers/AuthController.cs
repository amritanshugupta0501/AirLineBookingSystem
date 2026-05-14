using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        // In-memory user store (replace with real DB in production)
        private static readonly List<RegisteredUser> _users = new();

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            // Check hardcoded role-based credentials first
            if (model.Username == "admin" && model.Password == "password")
                return Ok(new { token = GenerateJwtToken(model.Username, "Admin"), role = "Admin" });

            if (model.Username == "staff" && model.Password == "password")
                return Ok(new { token = GenerateJwtToken(model.Username, "AirlineStaff"), role = "AirlineStaff" });

            if (model.Username == "user" && model.Password == "password")
                return Ok(new { token = GenerateJwtToken(model.Username, "User"), role = "User" });

            // Check registered users
            var registeredUser = _users.FirstOrDefault(u =>
                u.Username.Equals(model.Username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == model.Password);

            if (registeredUser != null)
                return Ok(new { token = GenerateJwtToken(registeredUser.Username, registeredUser.Role), role = registeredUser.Role });

            return Unauthorized(new { message = "Invalid username or password" });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { message = "Username and password are required" });

            // Check duplicates (hardcoded + registered)
            var existsHardcoded = model.Username.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
                                  model.Username.Equals("staff", StringComparison.OrdinalIgnoreCase) ||
                                  model.Username.Equals("user", StringComparison.OrdinalIgnoreCase);

            var existsRegistered = _users.Any(u => u.Username.Equals(model.Username, StringComparison.OrdinalIgnoreCase));

            if (existsHardcoded || existsRegistered)
                return Conflict(new { message = "Username already exists" });

            var newUser = new RegisteredUser
            {
                Username = model.Username,
                Password = model.Password, // hash in production!
                Email = model.Email ?? string.Empty,
                Role = "User"
            };

            _users.Add(newUser);

            var token = GenerateJwtToken(newUser.Username, newUser.Role);
            return Ok(new { token, role = newUser.Role, message = "Registration successful" });
        }

        [HttpPost("staff/login")]
        public IActionResult StaffLogin([FromBody] LoginModel model)
        {
            // Check hardcoded staff accounts
            if (model.Username == "staff" && model.Password == "password")
                return Ok(new { token = GenerateJwtToken(model.Username, "AirlineStaff"), role = "AirlineStaff" });

            if (model.Username == "admin" && model.Password == "password")
                return Ok(new { token = GenerateJwtToken(model.Username, "Admin"), role = "Admin" });

            // Check registered staff accounts
            var registeredStaff = _users.FirstOrDefault(u =>
                u.Username.Equals(model.Username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == model.Password &&
                (u.Role == "AirlineStaff" || u.Role == "Admin"));

            if (registeredStaff != null)
                return Ok(new { token = GenerateJwtToken(registeredStaff.Username, registeredStaff.Role), role = registeredStaff.Role });

            return Unauthorized(new { message = "Invalid staff credentials" });
        }

        [HttpPost("staff/register")]
        public IActionResult StaffRegister([FromBody] StaffRegisterModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { message = "Username and password are required" });

            var existsHardcoded = model.Username.Equals("admin", StringComparison.OrdinalIgnoreCase) ||
                                  model.Username.Equals("staff", StringComparison.OrdinalIgnoreCase) ||
                                  model.Username.Equals("user", StringComparison.OrdinalIgnoreCase);

            var existsRegistered = _users.Any(u => u.Username.Equals(model.Username, StringComparison.OrdinalIgnoreCase));

            if (existsHardcoded || existsRegistered)
                return Conflict(new { message = "Username already taken" });

            var newStaff = new RegisteredUser
            {
                Username = model.Username,
                Password = model.Password,
                Email = model.Email ?? string.Empty,
                Role = "AirlineStaff"
            };

            _users.Add(newStaff);

            var token = GenerateJwtToken(newStaff.Username, newStaff.Role);
            return Ok(new { token, role = newStaff.Role, message = "Staff account created successfully" });
        }

        [HttpGet("login-google")]
        public IActionResult LoginWithGoogle([FromQuery] string returnUrl = "http://localhost:4200")
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse", new { returnUrl }) };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse([FromQuery] string returnUrl = "http://localhost:4200")
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (result?.Principal != null)
            {
                var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
                var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
                            ?? result.Principal.Identity?.Name ?? "Google User";

                var token = GenerateJwtToken(email, "User");

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Redirect back to Angular frontend with token in URL
                return Redirect($"{returnUrl}/auth/callback?token={Uri.EscapeDataString(token)}&role=User&email={Uri.EscapeDataString(email)}");
            }

            return Redirect($"{returnUrl}/login?error=google_failed");
        }

        private string GenerateJwtToken(string username, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var keyBytes = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "superSecretKey@345ThisShouldBeLonger");
            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
    }

    public class RegisteredUser
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }

    public class StaffRegisterModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string StaffCode { get; set; } = string.Empty;
    }
}
