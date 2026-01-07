using System.ComponentModel.DataAnnotations.Schema;

namespace ContentService.Models
{
    [Table("Games")]
    public class Game : ContentItem
    {
        public string Developer { get; set; }
        public string Platform { get; set; }
        public string PEGIRating { get; set; }
        public bool MultiplayerSupport { get; set; }
    }
}