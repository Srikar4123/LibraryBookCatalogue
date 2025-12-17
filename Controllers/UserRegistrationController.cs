using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MiniProject.Data;
using MiniProject.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MiniProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRegistrationController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IPasswordHasher<UserRegistration> _hasher;
        private readonly IConfiguration _config;

        public UserRegistrationController(AppDbContext db, IPasswordHasher<UserRegistration> hasher, IConfiguration config)
        {
            _db = db;
            _hasher = hasher;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (dto == null) return BadRequest("Invalid data.");

            var user = new UserRegistration
            {
                userName = dto.UserName,
                email = dto.Email,
                phoneNumber = dto.PhoneNumber
            };

            user.password = _hasher.HashPassword(user, dto.Password);

            _db.UserRegistration.Add(user);
            await _db.SaveChangesAsync();

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]

        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid login data.");

            // Find user by username
            var user = await _db.UserRegistration
                                .FirstOrDefaultAsync(u => u.userName == dto.UserName);

            if (user == null)
                return Unauthorized("Invalid username or password.");

            // Verify password
            var result = _hasher.VerifyHashedPassword(user, user.password, dto.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid username or password.");

            return Ok(new
            {
                message = "Login successful",
                //UserName = user.userName,
                //email = user.email
            });
        }


        private string GenerateJwt(UserRegistration user)
        {
            var jwt = _config.GetSection("Jwt");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.userName),
                new Claim(JwtRegisteredClaimNames.Sub, user.userName),
                new Claim(JwtRegisteredClaimNames.Email, user.email)
            };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(jwt["ExpiresMinutes"] ?? "60")
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}

public record RegisterDto(string UserName, string PhoneNumber, string Email, string Password);
public record LoginDto(string UserName,string Password);
