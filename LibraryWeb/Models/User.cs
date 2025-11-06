using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibraryWeb.Models
{
    public enum UserRole
    {
        User,
        Reader,
        Employee,
        Admin
    }

    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required, MaxLength(150)]
        public required string Name { get; set; }

        [Required, MaxLength(50)]
        public required string Login { get; set; }

        [Required, MaxLength(100)]
        public required string UserPassword { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.Reader;

        // Навігаційні властивості
        public virtual Membership? Membership { get; set; }
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
