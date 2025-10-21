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

        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        // Foreign key → Reader (User subtype)
        [ForeignKey("Reader")]
        public int ReaderID { get; set; }
        public virtual User Reader { get; set; } = null!; 

        // Foreign key → Copy
        [ForeignKey("Copy")]
        public int InventoryNum { get; set; }
        public virtual Copy Copy { get; set; } = null!; 

        // Можна також додати, хто обслуговував (Employee)
        [ForeignKey("Employee")]
        public int? EmployeeID { get; set; }
        public virtual User? Employee { get; set; } 
    }
}
