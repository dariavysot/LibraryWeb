using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.PortableExecutable;

namespace LibraryWeb.Models
{
    public class Membership
    {
        [Key]
        public int MembershipID { get; private set; }

        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        public decimal Price { get; set; } = 0m;

        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        // Foreign key → Reader (User subtype)
        [ForeignKey("Reader")]
        public int ReaderID { get; set; }
        public virtual Reader Reader { get; set; } = null!;
    }
}
