using LibraryWeb.Models;
using LibraryWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryWeb
{
    public class TestLibrary
    {
        public static void RunTests(LibraryContext context)
        {
            // Очищаємо та створюємо базу
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            Console.WriteLine("===== Початок тестування LibraryWeb =====");

            var book = new Book
            {
                Title = "Test Book",
                Author = "Author One",
                ISBN = "123456",
                Language = "English",
                PublishingHouse = "Test Pub",
                PublicationYear = 2025,
                Type = "Novel",
                DateAdded = DateTime.Now
            };
            context.Books.Add(book);
            context.SaveChanges();

            var copy = new Copy
            {
                BookID = book.BookID,
                Condition = "New",
                Status = "Available",
                ReturnDate = null
            };
            context.Copies.Add(copy);
            context.SaveChanges();

            var reader = new User { Name = "Reader One", Login = "reader1", UserPassword = "pass1", Email = "reader1@test.com", Role = UserRole.Reader };
            var employee = new User { Name = "Employee One", Login = "emp1", UserPassword = "emp1pass", Email = "emp1@test.com", Role = UserRole.Employee };
            context.Users.AddRange(reader, employee);
            context.SaveChanges();

            var membership = new Membership
            {
                ReaderID = reader.UserID,
                Type = "Standard",
                Price = 10m,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(1),
                Status = "Active"
            };
            context.Memberships.Add(membership);
            context.SaveChanges();

            var payment = new Payment
            {
                UserID = reader.UserID,
                EmployeeID = employee.UserID,
                MembershipID = membership.MembershipID,
                Amount = 10m,
                Date = DateTime.Now,
                Type = "Membership",
                Status = "Paid"
            };
            context.Payments.Add(payment);

            var loan = new Loan
            {
                ReaderID = reader.UserID,
                EmployeeID = employee.UserID,
                InventoryNum = copy.InventoryNum,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(14),
                Status = "Active"
            };
            var reservation = new Reservation
            {
                ReaderID = reader.UserID,
                EmployeeID = employee.UserID,
                InventoryNum = copy.InventoryNum,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7),
                Status = "Active",
                Amount = 0m,
            };
            context.Loans.Add(loan);
            context.Reservations.Add(reservation);

            context.SaveChanges();

            var loadedReader = context.Users
                .Include(u => u.Membership)
                .Include(u => u.Loans)
                    .ThenInclude(l => l.Copy)
                        .ThenInclude(c => c.Book)
                .Include(u => u.Reservations)
                    .ThenInclude(r => r.Copy)
                        .ThenInclude(c => c.Book)
                .Include(u => u.Payments)
                    .ThenInclude(p => p.Employee)
                .FirstOrDefault(u => u.UserID == reader.UserID);

            Console.WriteLine($"User: {loadedReader!.Name} ({loadedReader.Role})");
            Console.WriteLine($"  • Membership: {loadedReader.Membership?.Type}");
            Console.WriteLine($"  • Loans: {loadedReader.Loans.Count}");
            Console.WriteLine($"     - Loaned Book: {loadedReader.Loans.FirstOrDefault()?.Copy.Book.Title}");
            Console.WriteLine($"  • Reservations: {loadedReader.Reservations.Count}");
            Console.WriteLine($"     - Reserved Book: {loadedReader.Reservations.FirstOrDefault()?.Copy.Book.Title}");
            Console.WriteLine($"  • Payments: {loadedReader.Payments.Count}");
            Console.WriteLine($"     - Payment processed by: {loadedReader.Payments.FirstOrDefault()?.Employee?.Name}");

            Console.WriteLine("===== Кінець тестування =====");
        }
    }
}
