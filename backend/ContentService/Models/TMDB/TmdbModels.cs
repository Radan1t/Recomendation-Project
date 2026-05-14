using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ContentService.Models.TMDB
{
    public class TmdbPagedResponse<T>
    {
        [JsonPropertyName("results")] public List<T> Results { get; set; }
    }

    
    public class TmdbKeywordsResponse { [JsonPropertyName("keywords")] public List<TmdbKeyword> Keywords { get; set; } }
    public class TmdbSeriesKeywordsResponse { [JsonPropertyName("results")] public List<TmdbKeyword> Results { get; set; } }
    public class TmdbKeyword { [JsonPropertyName("name")] public string Name { get; set; } }
    

    public class TmdbMovieDetails
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("title")] public string Title { get; set; }
        [JsonPropertyName("overview")] public string Overview { get; set; }
        [JsonPropertyName("release_date")] public string ReleaseDate { get; set; }
        [JsonPropertyName("vote_average")] public double VoteAverage { get; set; }
        [JsonPropertyName("poster_path")] public string PosterPath { get; set; }
        [JsonPropertyName("runtime")] public int? Runtime { get; set; }
        [JsonPropertyName("revenue")] public decimal? Revenue { get; set; }
        [JsonPropertyName("genres")] public List<TmdbGenre> Genres { get; set; }
        [JsonPropertyName("credits")] public TmdbCredits Credits { get; set; }

        [JsonPropertyName("original_language")] public string OriginalLanguage { get; set; } 
        [JsonPropertyName("keywords")] public TmdbKeywordsResponse KeywordsData { get; set; } 
    }

    public class TmdbSeriesDetails
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("overview")] public string Overview { get; set; }
        [JsonPropertyName("first_air_date")] public string FirstAirDate { get; set; }
        [JsonPropertyName("vote_average")] public double VoteAverage { get; set; }
        [JsonPropertyName("poster_path")] public string PosterPath { get; set; }
        [JsonPropertyName("number_of_seasons")] public int? NumberOfSeasons { get; set; }
        [JsonPropertyName("number_of_episodes")] public int? NumberOfEpisodes { get; set; }
        [JsonPropertyName("status")] public string Status { get; set; }
        [JsonPropertyName("networks")] public List<TmdbNetwork> Networks { get; set; }
        [JsonPropertyName("genres")] public List<TmdbGenre> Genres { get; set; }

        [JsonPropertyName("original_language")] public string OriginalLanguage { get; set; } 
        [JsonPropertyName("keywords")] public TmdbSeriesKeywordsResponse KeywordsData { get; set; } 
    }

    public class TmdbGenre { [JsonPropertyName("name")] public string Name { get; set; } }
    public class TmdbNetwork { [JsonPropertyName("name")] public string Name { get; set; } }
    public class TmdbCredits { [JsonPropertyName("crew")] public List<TmdbCrew> Crew { get; set; } }
    public class TmdbCrew
    {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("job")] public string Job { get; set; }
    }
    public class TmdbItem { [JsonPropertyName("id")] public int Id { get; set; } }
}
