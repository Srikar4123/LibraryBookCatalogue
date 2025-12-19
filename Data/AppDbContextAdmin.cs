using Microsoft.EntityFrameworkCore;
using MiniProject.Model;

namespace MiniProject.Data
{
    public class AppDbContextAdmin : DbContext
    {
        public AppDbContextAdmin(DbContextOptions<AppDbContextAdmin> options) : base(options) { }

        public DbSet<AdminModel> AdminModel { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AdminModel>()
               .HasIndex(u => u.email)
               .IsUnique();

            modelBuilder.Entity<AdminModel>()
                .HasIndex(u => u.phoneNumber)
                .IsUnique();
        }

    }
}
