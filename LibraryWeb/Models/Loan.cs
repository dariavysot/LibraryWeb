using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryWeb.Models
{
    public class Loan
    {
        [Key]
        public int LoanID { get; private set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public decimal? Fine { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        // Foreign key → (User subtype)
        [ForeignKey("User")]
        public int? UserID { get; set; }
        public virtual User? User { get; set; } = null!; 

        // Foreign key → Copy
        [ForeignKey("Copy")]
        public int InventoryNum { get; set; }
        public virtual Copy Copy { get; set; } = null!; 

    }
}
