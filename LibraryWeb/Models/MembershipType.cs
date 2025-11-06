using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryWeb.Models
{
    public class MembershipType
    {
        [Key]
        public int MembershipTypeID { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(1, 36)]
        public double DurationMonths { get; set; }

        [Required]
        [Range(0, 10000)]
        public decimal Price { get; set; }
    }
}
