using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryWeb.Models
{
    public class Reservation
    {
        [Key]
        public int ReservationID { get; private set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        public decimal Amount { get; set; } = 0m;

        // Foreign key → Reader (User subtype)
        [ForeignKey("Reader")]
        public int ReaderID { get; set; }
        public virtual Reader Reader { get; set; } = null!;

        // Foreign key → Copy
        [ForeignKey("Copy")]
        public int InventoryNum { get; set; }
        public virtual Copy Copy { get; set; } = null!;

        // Хто обробляв (опціонально)
        [ForeignKey("Employee")]
        public int? EmployeeID { get; set; }
        public virtual Employee? Employee { get; set; }
    }
}
