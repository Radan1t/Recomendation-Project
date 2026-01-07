using System.ComponentModel.DataAnnotations.Schema;

namespace ContentService.Models
{
    [Table("Series")]
    public class Series : ContentItem
    {
        public string Director { get; set; }
        public int SeasonCount { get; set; }
        public int EpisodesCount { get; set; }
        public string Status { get; set; }
        public string Network { get; set; }
        public string AgeRating { get; set; }
    }
}