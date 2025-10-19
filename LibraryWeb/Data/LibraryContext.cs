using LibraryWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryWeb.Data
{
    public class LibraryContext : DbContext
    {
        public LibraryContext(DbContextOptions<LibraryContext> options) : base(options) { }

        // Користувачі (TPH)
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Reader> Readers { get; set; }

        // Інші сутності
        public DbSet<Book> Books { get; set; }
        public DbSet<Copy> Copies { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Наслідування: одна таблиця "Users"
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("Discriminator")
                .HasValue<User>("User")
                .HasValue<Admin>("Admin")
                .HasValue<Employee>("Employee")
                .HasValue<Reader>("Reader");

            // Reader → Membership (опційне один-на-один)
            modelBuilder.Entity<Membership>()
                .HasOne(m => m.Reader)
                .WithOne(r => r.Membership)
                .HasForeignKey<Membership>(m => m.ReaderID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction); // вимикаємо каскад

            // Payment → Membership (немає каскадного видалення)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Membership)
                .WithMany() // Membership не має колекції Payment
                .HasForeignKey(p => p.MembershipID)
                .OnDelete(DeleteBehavior.NoAction);

            // Payment → User (немає каскадного видалення)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserID)
                .OnDelete(DeleteBehavior.NoAction);

            // Copy → Book (можна каскадно видаляти копії книги)
            modelBuilder.Entity<Copy>()
                .HasOne(c => c.Book)
                .WithMany(b => b.Copies)
                .HasForeignKey(c => c.BookID)
                .OnDelete(DeleteBehavior.Cascade);

            // Loan → Reader
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Reader)
                .WithMany(r => r.Loans)
                .HasForeignKey(l => l.ReaderID)
                .OnDelete(DeleteBehavior.NoAction);

            // Loan → Copy
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Copy)
                .WithMany(c => c.Loans)
                .HasForeignKey(l => l.InventoryNum)
                .OnDelete(DeleteBehavior.NoAction);

            // Loan → Employee (опційно)
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Employee)
                .WithMany(e => e.Loans)
                .HasForeignKey(l => l.EmployeeID)
                .OnDelete(DeleteBehavior.NoAction);

            // Reservation → Reader
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Reader)
                .WithMany(rdr => rdr.Reservations)
                .HasForeignKey(r => r.ReaderID)
                .OnDelete(DeleteBehavior.NoAction);

            // Reservation → Copy
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Copy)
                .WithMany(c => c.Reservations)
                .HasForeignKey(r => r.InventoryNum)
                .OnDelete(DeleteBehavior.NoAction);

            // Reservation → Employee (опційно)
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Employee)
                .WithMany(e => e.Reservations)
                .HasForeignKey(r => r.EmployeeID)
                .OnDelete(DeleteBehavior.NoAction);

            // Ініціалізація колекцій для Employee та Reader
            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Loans)
                .WithOne(l => l.Employee)
                .HasForeignKey(l => l.EmployeeID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Reservations)
                .WithOne(r => r.Employee)
                .HasForeignKey(r => r.EmployeeID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Employee>()
                .HasMany(e => e.PaymentsHandled)
                .WithOne(p => p.Employee)
                .HasForeignKey(p => p.EmployeeID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Reader>()
                .HasMany(r => r.Loans)
                .WithOne(l => l.Reader)
                .HasForeignKey(l => l.ReaderID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Reader>()
                .HasMany(r => r.Reservations)
                .WithOne(rz => rz.Reader)
                .HasForeignKey(rz => rz.ReaderID)
                .OnDelete(DeleteBehavior.NoAction);

            base.OnModelCreating(modelBuilder);
        }
    }
}
