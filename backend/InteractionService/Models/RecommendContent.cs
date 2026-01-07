using System.ComponentModel.DataAnnotations;

namespace InteractionService.Models
{
    public class RecommendContent
    {
        [Key]
        public int Id { get; set; }
        
        public int RecommendationId { get; set; }
        public Recommendation Recommendation { get; set; } // Навігація назад до Recommendation

        public int ContentId { get; set; } // ID рекомендованого контенту
        public double RelevanceScore { get; set; }
    }
}