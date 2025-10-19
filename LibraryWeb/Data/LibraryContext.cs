using LibraryWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryWeb.Data
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Copy> Copies { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // 🔹 Додаємо обмеження для decimal
            modelBuilder.Entity<Membership>()
                .Property(m => m.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Reservation>()
                .Property(r => r.Amount)
                .HasPrecision(18, 2);

            // 🔹 Встановлюємо зв’язки
            modelBuilder.Entity<User>()
                .HasMany(u => u.Loans)
                .WithOne()
                .HasForeignKey(l => l.ReaderID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Reservations)
                .WithOne()
                .HasForeignKey(r => r.ReaderID)
                .OnDelete(DeleteBehavior.Cascade);

            // Payment → User (платник)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserID)
                .OnDelete(DeleteBehavior.NoAction);

            // Payment → Employee (той, хто обробляв)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Employee)
                .WithMany()
                .HasForeignKey(p => p.EmployeeID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Loan>()
               .HasOne(l => l.Reader)
               .WithMany(u => u.Loans)
               .HasForeignKey(l => l.ReaderID)
               .OnDelete(DeleteBehavior.Restrict); // 🔹 не каскадно

            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Employee)
                .WithMany()
                .HasForeignKey(l => l.EmployeeID)
                .OnDelete(DeleteBehavior.Restrict); // 🔹 не каскадно

            modelBuilder.Entity<Reservation>()
              .HasOne(r => r.Reader)
              .WithMany(u => u.Reservations)
              .HasForeignKey(r => r.ReaderID)
              .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Employee)
                .WithMany()
                .HasForeignKey(r => r.EmployeeID)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
