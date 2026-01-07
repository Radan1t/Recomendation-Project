using System.ComponentModel.DataAnnotations;

namespace InteractionService.Models
{
    public class Interaction
    {
        [Key]
        public int InteractionId { get; set; }

        [Required]
        public Guid UserId { get; set; } // Просто ID, без об'єкта User

        [Required]
        public int ContentId { get; set; } // Просто ID, без об'єкта ContentItem

        [Required]
        public string ActionType { get; set; } // Like, View, Click
        
        public DateTime ActionDate { get; set; } = DateTime.UtcNow;
    }
}