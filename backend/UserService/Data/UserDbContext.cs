using Microsoft.EntityFrameworkCore;
using UserService.Entities;

namespace UserService.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ProfileGenre> ProfileGenres { get; set; }
        public DbSet<ProfileLanguage> ProfileLanguages { get; set; }
        public DbSet<ProfileTag> ProfileTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<UserProfile>().HasKey(p => p.UserID);
    
    modelBuilder.Entity<User>()
        .HasOne(u => u.UserProfile)
        .WithOne(p => p.User)
        .HasForeignKey<UserProfile>(p => p.UserID);

    
    modelBuilder.Entity<ProfileGenre>().HasKey(pg => new { pg.UserID, pg.GenreID });
    modelBuilder.Entity<ProfileGenre>().HasOne<UserProfile>().WithMany().HasForeignKey(pg => pg.UserID);

    modelBuilder.Entity<ProfileLanguage>().HasKey(pl => new { pl.UserID, pl.LanguageID });
    modelBuilder.Entity<ProfileLanguage>().HasOne<UserProfile>().WithMany().HasForeignKey(pl => pl.UserID);

    modelBuilder.Entity<ProfileTag>().HasKey(pt => new { pt.UserID, pt.TagID });
    modelBuilder.Entity<ProfileTag>().HasOne<UserProfile>().WithMany().HasForeignKey(pt => pt.UserID);
}
}}
