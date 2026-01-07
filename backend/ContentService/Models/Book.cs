using System.ComponentModel.DataAnnotations.Schema;

namespace ContentService.Models
{
    [Table("Books")]
    public class Book : ContentItem
    {
        public string Author { get; set; }
        public string Publisher { get; set; }
        public int PageCount { get; set; }
        public string ISBN { get; set; }
        public string Language { get; set; }
    }
}