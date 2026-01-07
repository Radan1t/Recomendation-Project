using System.ComponentModel.DataAnnotations.Schema;

namespace ContentService.Models
{
    [Table("Films")]
    public class Film : ContentItem
    {
        public string Director { get; set; }
        public string Cast { get; set; }
        public int DurationMinutes { get; set; }
        public string Country { get; set; }
        public string AgeRating { get; set; }
    }
}