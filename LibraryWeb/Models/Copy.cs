using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryWeb.Models
{
    public class Copy
    {
        [Key]
        public int InventoryNum { get; set; }

        [MaxLength(50)]
        public string Condition { get; set; } = string.Empty; // ініціалізація

        [MaxLength(50)]
        public string Status { get; set; } = string.Empty; // ініціалізація

        public DateTime? ReturnDate { get; set; }

        // Foreign key → Book
        [ForeignKey("Book")]
        public int BookID { get; set; }

        public virtual Book Book { get; set; } = null!;

        public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }

}
