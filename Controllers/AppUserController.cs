
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using MiniProject.Data;
using MiniProject.Model;

namespace MiniProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // => api/AppUser
    public class AppUserController : ControllerBase
    {
        private readonly AppDbContextUser _db;
        public AppUserController(AppDbContextUser db) { _db = db; }

        // GET: api/AppUser
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetAll()
        {
            var users = await _db.AdUsers.AsNoTracking().ToListAsync();
            return Ok(users);
        }

        // GET: api/AppUser/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AppUser>> GetById(int id)
        {
            var user = await _db.AdUsers.FindAsync(id);
            return user is null ? NotFound() : Ok(user);
        }

        // POST: api/AppUser
        [HttpPost]
        public async Task<ActionResult<AppUser>> Create([FromBody] UserCreateDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            if (await _db.AdUsers.AnyAsync(u => u.Email == dto.Email))
                return Conflict("Email already exists.");

            var u = new AppUser
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Role = dto.Role,
                IsActive = dto.IsActive
            };

            _db.AdUsers.Add(u);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = u.Id }, u);
        }

        // PUT: api/AppUser/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var u = await _db.AdUsers.FindAsync(id);
            if (u is null) return NotFound();

            if (!string.Equals(u.Email, dto.Email, StringComparison.OrdinalIgnoreCase) &&
                await _db.AdUsers.AnyAsync(x => x.Email == dto.Email))
            {
                return Conflict("Email already exists.");
            }

            u.Name = dto.Name;
            u.Email = dto.Email;
            u.Phone = dto.Phone;
            u.Role = dto.Role;
            u.IsActive = dto.IsActive;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/AppUser/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var u = await _db.AdUsers.FindAsync(id);
            if (u is null) return NotFound();

            _db.AdUsers.Remove(u);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/AppUser/search?query=kavya&role=Admin&active=true&page=1&pageSize=10&sort=Name&dir=asc
        [HttpGet("search")]
        public async Task<ActionResult<object>> Search(
            [FromQuery] string? query,
            [FromQuery] string? role,
            [FromQuery] bool? active,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sort = "Name",
            [FromQuery] string dir = "asc")
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0 || pageSize > 200) pageSize = 10;

            var q = _db.AdUsers.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var term = query.Trim().ToLower();
                q = q.Where(u =>
                    (u.Name != null && u.Name.ToLower().Contains(term)) ||
                    (u.Email != null && u.Email.ToLower().Contains(term)) ||
                    (u.Phone != null && u.Phone.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                q = q.Where(u => u.Role == role);
            }

            if (active.HasValue)
            {
                q = q.Where(u => u.IsActive == active.Value);
            }

            bool asc = string.Equals(dir, "asc", StringComparison.OrdinalIgnoreCase);
            q = sort switch
            {
                "Email" => asc ? q.OrderBy(u => u.Email) : q.OrderByDescending(u => u.Email),
                "Phone" => asc ? q.OrderBy(u => u.Phone) : q.OrderByDescending(u => u.Phone),
                "Role" => asc ? q.OrderBy(u => u.Role) : q.OrderByDescending(u => u.Role),
                "IsActive" => asc ? q.OrderBy(u => u.IsActive) : q.OrderByDescending(u => u.IsActive),
                _ => asc ? q.OrderBy(u => u.Name) : q.OrderByDescending(u => u.Name)
            };

            var total = await q.CountAsync();
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new { page, pageSize, total, items });
        }
    }

    // === DTOs declared in the same file ===
    public class UserCreateDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required, MaxLength(50)]
        public string Role { get; set; } = "Member";

        public bool IsActive { get; set; } = true;
    }

    public class UserUpdateDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required, MaxLength(50)]
        public string Role { get; set; } = "Member";

        public bool IsActive { get; set; } = true;
    }
}

