using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryWeb.Models
{
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }

        public decimal Amount { get; set; } = 0m;
        public DateTime Date { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        //[ForeignKey("Membership")]
        public int? MembershipID { get; set; }
        public virtual Membership? Membership { get; set; }

        // Оплата за резервацію
        //[ForeignKey("Reservation")]
        public int? ReservationID { get; set; }
        public virtual Reservation? Reservation { get; set; }

        [ForeignKey("Loan")]
        public int? LoanID { get; set; }
        public virtual Loan? Loan { get; set; }

        // Хто оплатив
        [ForeignKey("User")]
        public int? UserID { get; set; }
        public virtual User? User { get; set; } = null!;

        // Хто обробляв (опціонально)
        [ForeignKey("Employee")]
        public int? EmployeeID { get; set; }
        public virtual User? Employee { get; set; }
    }
}
