using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryWeb.Models
{
    public class Reservation
    {
        [Key]
        public int ReservationID { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        public decimal Amount { get; set; } = 0m;
        public string? UserName { get; set; }

        // Foreign key → User
        [ForeignKey("User")]
        public int? UserID { get; set; }
        public virtual User? User { get; set; } = null!;

        // Foreign key → Copy
        [ForeignKey("Copy")]
        public int InventoryNum { get; set; }
        public virtual Copy Copy { get; set; } = null!;

        //[ForeignKey("Payment")]
        //public int? PaymentID { get; set; }
        public virtual Payment? Payment { get; set; }
    }
}
