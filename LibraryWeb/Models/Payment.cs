using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryWeb.Models
{
    public class Payment
    {
        [Key]
        public int PaymentID { get; private set; }

        public decimal Amount { get; set; } = 0m;
        public DateTime Date { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        // Payment прив’язаний до Membership
        [ForeignKey("Membership")]
        public int MembershipID { get; set; }
        public virtual Membership Membership { get; set; } = null!;

        // Хто оплатив (будь-який користувач)
        [ForeignKey("User")]
        public int UserID { get; set; }
        public virtual User User { get; set; } = null!;

        // Хто обробляв (якщо потрібно)
        [ForeignKey("Employee")]
        public int? EmployeeID { get; set; }
        public virtual Employee? Employee { get; set; }
    }
}
