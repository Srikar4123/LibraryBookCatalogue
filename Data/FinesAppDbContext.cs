using Microsoft.EntityFrameworkCore;
using MiniProject.Model;

namespace MiniProject.Data
{
    public class FinesAppDbContext : DbContext
    {
        public FinesAppDbContext(DbContextOptions<FinesAppDbContext> options) : base(options) { }

        public DbSet<Fines> Fines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Fines>(entity =>
            {
                entity.ToTable("Fines");
                entity.HasKey(f => f.Id);

                entity.Property(f => f.fineAmount)
                      .HasColumnType("decimal(10,2)")
                      .IsRequired();

                // Foreign Key to AppUser
                entity.HasOne<AppUser>()
                      .WithMany()
                      .HasForeignKey(f => f.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Dates
                entity.Property(f => f.IssueDate)
                      .HasColumnType("datetime2")
                      .IsRequired();

                entity.Property(f => f.ReturnDate)
                      .HasColumnType("datetime2");

                // Foreign Key to Book
                entity.HasOne<Books>()
                      .WithMany()
                      .HasForeignKey(f => f.BookId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
