using System;
using System.Collections.Generic;

namespace InteractionService.Entities
{
    public class RecommendationSession
    {
        public int SessionID { get; set; }
        public DateTime DateGenerated { get; set; }
        public string AlgorithmType { get; set; }
        public int UserID { get; set; }

        public ICollection<RecommendedContent> RecommendedContents { get; set; } = new List<RecommendedContent>();
    }
}
