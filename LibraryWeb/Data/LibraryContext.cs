
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
        public DbSet<MembershipType> MembershipTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Обмеження для decimal
            modelBuilder.Entity<Membership>()
                .Property(m => m.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Loan>()
                .Property(l => l.Fine)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Reservation>()
                .Property(r => r.Amount)
                .HasPrecision(18, 2);

            // User → Loans та Reservations (Reader)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Loans)
                .WithOne(l => l.User)
                .HasForeignKey(l => l.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Reservations)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment → User (платник) та Employee (той, хто обробляв)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Employee)
                .WithMany()
                .HasForeignKey(p => p.EmployeeID)
                .OnDelete(DeleteBehavior.NoAction);

            // Membership → User (Reader)
            modelBuilder.Entity<Membership>()
                .HasOne(m => m.User)
                .WithOne(u => u.Membership)
                .HasForeignKey<Membership>(m => m.UserID)
                .OnDelete(DeleteBehavior.NoAction);

            // Book → Copies
            modelBuilder.Entity<Book>()
                .HasMany(b => b.Copies)
                .WithOne(c => c.Book)
                .HasForeignKey(c => c.BookID)
                .OnDelete(DeleteBehavior.Cascade);

            // Copy → Loans та Reservations
            modelBuilder.Entity<Copy>()
                .HasMany(c => c.Loans)
                .WithOne(l => l.Copy)
                .HasForeignKey(l => l.InventoryNum)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Copy>()
                .HasMany(c => c.Reservations)
                .WithOne(r => r.Copy)
                .HasForeignKey(r => r.InventoryNum)
                .OnDelete(DeleteBehavior.Restrict);

            // Membership → MembershipType
            modelBuilder.Entity<Membership>()
                .HasOne(m => m.MembershipType)
                .WithMany()
                .HasForeignKey(m => m.MembershipTypeID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
