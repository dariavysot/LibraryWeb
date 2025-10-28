using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryWeb.Models
{
    public class Membership
    {
        [Key]
        public int MembershipID { get; set; }

        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        public decimal Price { get; set; } = 0m;

        public DateTime StartDate { get; set; } = DateTime.Now;

        public DateTime EndDate { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        [ForeignKey("User")]
        public int UserID { get; set; }

        public virtual User User { get; set; } = null!;

        [ForeignKey("MembershipType")]
        public int MembershipTypeID { get; set; }
        public virtual MembershipType MembershipType { get; set; } = null!;

        public int? PaymentID { get; set; } // nullable, бо платіж ще може не бути
        public Payment? Payment { get; set; }
    }
}
