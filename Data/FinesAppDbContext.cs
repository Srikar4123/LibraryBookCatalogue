
using Microsoft.EntityFrameworkCore;
using MiniProject.Model;

namespace MiniProject.Data
{
    public class FinesAppDbContext : DbContext
    {
        public FinesAppDbContext(DbContextOptions<FinesAppDbContext> options) : base(options) { }

        // DbSets needed for EF to resolve principal tables for FK constraints
        public DbSet<Fines> Fines { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }  // principal for UserId FK
        public DbSet<Books> Books { get; set; }       // principal for BookId FK

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Map AppUser to the actual table name in your DB
            // If your users table is AdUsers, keep this. If it's AppUser, change to "AppUser".
            modelBuilder.Entity<AppUser>().ToTable("AdUsers");

            // Map Books to the actual table name (change if different, e.g., "Books")
            modelBuilder.Entity<Books>().ToTable("Books");

            // Fines table configuration
            modelBuilder.Entity<Fines>(entity =>
            {
                entity.ToTable("Fines");
                entity.HasKey(f => f.Id);

                // Amount
                entity.Property(f => f.fineAmount)
                      .HasColumnType("decimal(10,2)")
                      .IsRequired();

                // Dates
                entity.Property(f => f.IssueDate)
                      .HasColumnType("datetime2")
                      .IsRequired();

                entity.Property(f => f.ReturnDate)
                      .HasColumnType("datetime2");

                // FK to AppUser (principal table: AdUsers)
                // If Fines has no navigation property, this still works.
                entity.HasOne<AppUser>()
                      .WithMany()
                      .HasForeignKey(f => f.UserId)
                      .OnDelete(DeleteBehavior.Restrict); // avoid deleting fines when user is deleted

                // FK to Books (principal table: Books)
                entity.HasOne<Books>()
                      .WithMany()
                      .HasForeignKey(f => f.BookId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
