using Microsoft.EntityFrameworkCore;
using MiniProject.Model;

namespace MiniProject.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<UserRegistration> UserRegistration { get; set; }

        public DbSet<AdminModel> AdminModel { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRegistration>()
                .HasIndex(u => u.email)
                .IsUnique();

            modelBuilder.Entity<UserRegistration>()
                .HasIndex(u => u.phoneNumber)
                .IsUnique();

            modelBuilder.Entity<AdminModel>()
               .HasIndex(u => u.email)
               .IsUnique();

            modelBuilder.Entity<AdminModel>()
                .HasIndex(u => u.phoneNumber)
                .IsUnique();
        }

    }
}
