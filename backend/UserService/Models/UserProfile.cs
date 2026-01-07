using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models
{
    public class UserProfile
    {
        [Key]
        public int ProfileId { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public string PreferredGenres { get; set; }     // JSON або рядок через кому
        public string PreferredLanguages { get; set; }
        public string Country { get; set; }

        public User User { get; set; }
    }
}