using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LibraryWeb.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Login { get; set; } = string.Empty;

        [Required]
        public string UserPassword { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        // Навігаційні властивості
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
