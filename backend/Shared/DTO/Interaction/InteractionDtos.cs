using System;

namespace Shared.DTO.Interactions;

public class RatingRequestDto 
{
    public int ContentId { get; set; }
    public int Score { get; set; }
}

public class FavoriteRequestDto 
{
    public int ContentId { get; set; }
}

public class InteractionStatusDto
{
    public int ContentId { get; set; }
    public bool IsFavorite { get; set; }
    public int? UserScore { get; set; }
}

public class UserRatingDto
{
    public int ContentId { get; set; }
    public int Score { get; set; }
    public DateTime DateRated { get; set; }
}

public class UserFavoriteDto
{
    public int ContentId { get; set; }
    public DateTime DateAdded { get; set; }
}

public class ContentAverageDto
{
    public double Average { get; set; }
    public int Count { get; set; }
}
