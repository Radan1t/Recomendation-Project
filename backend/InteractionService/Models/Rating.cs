using System.ComponentModel.DataAnnotations;

namespace InteractionService.Models
{
    public class Rating
    {
        [Key]
        public int RatingId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int ContentId { get; set; }

        [Range(1, 10)]
        public int Score { get; set; }

        public DateTime DateRated { get; set; } = DateTime.UtcNow;
    }
}