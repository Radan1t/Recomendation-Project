using System.ComponentModel.DataAnnotations;

namespace InteractionService.Models
{
    public class Recommendation
    {
        [Key]
        public int RecommendationId { get; set; }

        public Guid UserId { get; set; }
        public DateTime DateGenerated { get; set; }
        public string AlgorithmType { get; set; }

        // Список вкладених елементів
        public List<RecommendContent> Items { get; set; }
    }
}