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
    }
}

public record RegisterDto(string UserName, string PhoneNumber, string Email, string Password);