using System.ComponentModel.DataAnnotations;

namespace LibraryWeb.Models
{
    public class Book
    {
        [Key]
        public int BookID { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Type { get; set; } = string.Empty;

        [MaxLength(20)]
        public string ISBN { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Language { get; set; } = string.Empty;

        [MaxLength(100)]
        public string PublishingHouse { get; set; } = string.Empty;

        public int? PublicationYear { get; set; }

        [MaxLength(100)]
        public string Author { get; set; } = string.Empty;

        public DateTime DateAdded { get; set; } = DateTime.Now;

        // Зв’язки
        public virtual ICollection<Copy> Copies { get; set; } = new List<Copy>(); // ініціалізація
    }
}
