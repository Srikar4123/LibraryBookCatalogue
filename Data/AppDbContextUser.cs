using Microsoft.EntityFrameworkCore;
using MiniProject.Model;

namespace MiniProject.Data
{
    public class AppDbContextUser : DbContext
    {

        public AppDbContextUser(DbContextOptions<AppDbContextUser> options)
                    : base(options) { }

        public DbSet<AppUser> AdUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Map AppUser entity to existing table name
            modelBuilder.Entity<AppUser>().ToTable("AdUsers");

            // You can add other configurations here if needed
        }


    }
}
