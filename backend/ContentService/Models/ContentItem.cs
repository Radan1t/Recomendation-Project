using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContentService.Models
{
    [Table("Contents")]
    public abstract class ContentItem
    {
        [Key]
        public int ContentId { get; set; }

        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public string Genre { get; set; }
        public DateTime ReleaseDate { get; set; }
        public double AverageRating { get; set; }
        public string PosterUrl { get; set; }
        
        // Службове поле для EF Core
        public string ContentType { get; set; } 
    }
}