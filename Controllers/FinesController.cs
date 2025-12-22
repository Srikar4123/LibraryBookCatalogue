
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MiniProject.Data;
using MiniProject.Model;

namespace MiniProject.Controllers
{
    [ApiController]
    [Route("api/fines")]
    public class FinesController : ControllerBase
    {
        private readonly FinesAppDbContext _db;

        public FinesController(FinesAppDbContext db)
        {
            _db = db;
        }

        // ---------- DTOs (inline with validation) ----------
        public record FineCreateDto(
            [Required] int UserId,
            [Required] int BookId,
            [Range(0.01, double.MaxValue, ErrorMessage = "fineAmount must be greater than zero.")] decimal fineAmount,
            bool paymentStatus,
            [Required] DateTime IssueDate,
            DateTime? ReturnDate
        );

        public record FineUpdateDto(
            [Range(0, double.MaxValue, ErrorMessage = "fineAmount cannot be negative.")] decimal? fineAmount,
            bool? paymentStatus,
            DateTime? ReturnDate
        );

        // ---------- Endpoints ----------

        // POST: /api/fines/create
        // Create a fine
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] FineCreateDto dto)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            // Basic guards
            if (dto.UserId <= 0)
                return BadRequest(new { message = "UserId must be greater than zero." });

            if (dto.BookId <= 0)
                return BadRequest(new { message = "BookId must be greater than zero." });

            // Date sanity: ReturnDate should not be before IssueDate
            if (dto.ReturnDate.HasValue && dto.ReturnDate.Value < dto.IssueDate)
                return BadRequest(new { message = "ReturnDate cannot be earlier than IssueDate." });

            // Ensure principal records exist to avoid FK violations
            var userExists = await _db.AppUsers.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
                return NotFound(new { message = $"User with id {dto.UserId} not found." });

            var bookExists = await _db.Books.AnyAsync(b => b.Id == dto.BookId);
            if (!bookExists)
                return NotFound(new { message = $"Book with id {dto.BookId} not found." });

            var fine = new Fines
            {
                UserId = dto.UserId,
                BookId = dto.BookId,
                fineAmount = dto.fineAmount,
                paymentStatus = dto.paymentStatus,
                IssueDate = dto.IssueDate,
                ReturnDate = dto.ReturnDate
            };

            _db.Fines.Add(fine);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return Conflict(new
                {
                    message = "Could not create fine due to a database constraint.",
                    hint = "Ensure UserId and BookId reference existing records, and mappings (AdUsers/Books) are correct.",
                    details = ex.InnerException?.Message ?? ex.Message
                });
            }

            return CreatedAtAction(nameof(GetById), new { id = fine.Id }, fine);
        }

        // GET: /api/fines/{id}
        // Retrieve a single fine by id
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var fine = await _db.Fines.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
            return fine is null ? NotFound() : Ok(fine);
        }

        // GET: /api/fines
        // List fines with optional filters: userId, onlyUnpaid
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? userId = null, [FromQuery] bool? onlyUnpaid = null)
        {
            IQueryable<Fines> q = _db.Fines.AsNoTracking();

            if (userId.HasValue) q = q.Where(f => f.UserId == userId.Value);
            if (onlyUnpaid == true) q = q.Where(f => !f.paymentStatus);

            var list = await q.OrderByDescending(f => f.IssueDate).ToListAsync();
            return Ok(list);
        }

        // GET: /api/fines/user/{userId}
        // Shortcut to list a user's fines
        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetByUser(int userId, [FromQuery] bool onlyUnpaid = false)
        {
            var q = _db.Fines.AsNoTracking().Where(f => f.UserId == userId);
            if (onlyUnpaid) q = q.Where(f => !f.paymentStatus);

            var list = await q.OrderByDescending(f => f.IssueDate).ToListAsync();
            return Ok(list);
        }

        // PUT: /api/fines/{id}
        // Update amount, paymentStatus, returnDate
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] FineUpdateDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var fine = await _db.Fines.FindAsync(id);
            if (fine is null) return NotFound();

            if (dto.fineAmount.HasValue)
            {
                if (dto.fineAmount.Value < 0) return BadRequest(new { message = "fineAmount cannot be negative." });
                fine.fineAmount = dto.fineAmount.Value;
            }

            if (dto.paymentStatus.HasValue)
                fine.paymentStatus = dto.paymentStatus.Value;

            if (dto.ReturnDate.HasValue)
            {
                if (dto.ReturnDate.Value < fine.IssueDate)
                    return BadRequest(new { message = "ReturnDate cannot be earlier than IssueDate." });
                fine.ReturnDate = dto.ReturnDate.Value;
            }

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return Conflict(new
                {
                    message = "Could not update fine due to a database constraint.",
                    details = ex.InnerException?.Message ?? ex.Message
                });
            }

            return Ok(fine);
        }

        // PUT: /api/fines/{id}/pay
        // Mark a fine as paid
        [HttpPut("{id:int}/pay")]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var fine = await _db.Fines.FindAsync(id);
            if (fine is null) return NotFound();

            if (fine.paymentStatus == true)
            {
                // Already paid; return current state
                return Ok(fine);
            }

            fine.paymentStatus = true;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return Conflict(new
                {
                    message = "Could not mark fine as paid due to a database constraint.",
                    details = ex.InnerException?.Message ?? ex.Message
                });
            }

            return Ok(fine);
        }

        // DELETE: /api/fines/{id}
        // Delete a fine
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var fine = await _db.Fines.FindAsync(id);
            if (fine is null) return NotFound();

            _db.Fines.Remove(fine);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return Conflict(new
                {
                    message = "Could not delete fine due to a database constraint.",
                    details = ex.InnerException?.Message ?? ex.Message
                });
            }

            return NoContent(); // 204
        }
    }
}
