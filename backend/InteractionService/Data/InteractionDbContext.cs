using Microsoft.EntityFrameworkCore;
using InteractionService.Entities;

namespace InteractionService.Data;

public class InteractionDbContext : DbContext
{
    public InteractionDbContext(DbContextOptions<InteractionDbContext> options)
        : base(options)
    {
    }

    
    public DbSet<UserRating> UserRatings { get; set; }
    public DbSet<UserFavorite> UserFavorites { get; set; }
    public DbSet<InteractionLog> InteractionLogs { get; set; }

    
    public DbSet<RecommendationSession> RecommendationSessions { get; set; }
    public DbSet<RecommendedContent> RecommendedContents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    
    modelBuilder.Entity<RecommendationSession>()
        .HasKey(s => s.SessionID);

    
    modelBuilder.Entity<InteractionLog>()
        .HasKey(l => l.LogID);

    
    modelBuilder.Entity<UserRating>()
        .HasKey(r => new { r.UserID, r.ContentID });

    modelBuilder.Entity<UserFavorite>()
        .HasKey(f => new { f.UserID, f.ContentID });

    modelBuilder.Entity<RecommendedContent>()
        .HasKey(rc => new { rc.SessionID, rc.ContentID });

    
    modelBuilder.Entity<RecommendedContent>()
        .HasOne(rc => rc.Session)
        .WithMany(s => s.RecommendedContents)
        .HasForeignKey(rc => rc.SessionID);
}
}
