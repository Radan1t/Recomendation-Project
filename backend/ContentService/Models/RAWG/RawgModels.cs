using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ContentService.Models.RAWG
{
    public class RawgResponse
    {
        [JsonPropertyName("results")] public List<RawgGame> Results { get; set; }
    }

    public class RawgGame
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("released")] public DateTime? Released { get; set; }
        [JsonPropertyName("background_image")] public string BackgroundImage { get; set; }
        [JsonPropertyName("rating")] public double Rating { get; set; }
        [JsonPropertyName("esrb_rating")] public RawgEsrbRating EsrbRating { get; set; }
        [JsonPropertyName("genres")] public List<RawgGenre> Genres { get; set; }
        [JsonPropertyName("tags")] public List<RawgTag> Tags { get; set; }
    }

    public class RawgEsrbRating { [JsonPropertyName("name")] public string Name { get; set; } }
    public class RawgGenre { [JsonPropertyName("name")] public string Name { get; set; } }
    public class RawgTag { [JsonPropertyName("name")] public string Name { get; set; } }
    public class RawgGameDetails
    {
        [JsonPropertyName("id")] 
        public int Id { get; set; }

        [JsonPropertyName("description_raw")] 
        public string DescriptionRaw { get; set; }

        [JsonPropertyName("developers")] 
        public List<RawgCompany> Developers { get; set; }

        [JsonPropertyName("publishers")] 
        public List<RawgCompany> Publishers { get; set; }
    }

    public class RawgCompany
    {
        [JsonPropertyName("name")] 
        public string Name { get; set; }
    }
}
