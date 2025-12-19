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
    public class AdminModelController : ControllerBase
    {
        private readonly AppDbContextAdmin _db;
        private readonly IPasswordHasher<AdminModel> _hasher;
        private readonly IConfiguration _config;

        public AdminModelController(AppDbContextAdmin db, IPasswordHasher<AdminModel> hasher, IConfiguration config)
        {
            _db = db;
            _hasher = hasher;
            _config = config;
        }

        [HttpPost("admin-reg")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminDto dto)
        {
            if (dto == null) return BadRequest("Invalid data.");

            if (await _db.AdminModel.AnyAsync(u => u.email == dto.Email))
                return BadRequest("Email already exists.");

            if (await _db.AdminModel.AnyAsync(u => u.phoneNumber == dto.PhoneNumber))
                return BadRequest("Phone number already exists.");

            if (dto.Password != dto.ConfirmPassword) return BadRequest("The password doesn't match");

            var admin = new AdminModel
            {
                userName = dto.UserName,
                email = dto.Email,
                phoneNumber = dto.PhoneNumber
            };

            admin.password = _hasher.HashPassword(admin, dto.Password);

            _db.AdminModel.Add(admin);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Admin registered successfully" });
        }

        [HttpPost("admin-log")]
        public async Task<IActionResult> Login([FromBody] LoginAdminDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid login data.");

            // Find user by username
            var admin = await _db.AdminModel.FirstOrDefaultAsync(u => u.userName == dto.UserName);
            if (admin == null)
                return Unauthorized("Invalid username or password.");

            // Verify password
            var result = _hasher.VerifyHashedPassword(admin, admin.password, dto.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid username or password.");
            return Ok(new
            {
                message = "Login successful"
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

public record RegisterAdminDto(string UserName, string PhoneNumber, string Email, string Password, string ConfirmPassword);
public record LoginAdminDto(string UserName, string Password);