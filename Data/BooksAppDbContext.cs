using Microsoft.EntityFrameworkCore;
using MiniProject.Model;

namespace MiniProject.Data
{
    public class BooksAppDbContext : DbContext
        {
            public BooksAppDbContext(DbContextOptions<BooksAppDbContext> options) : base(options) { }
            public DbSet<Books> Books { get; set; }
    }
}



