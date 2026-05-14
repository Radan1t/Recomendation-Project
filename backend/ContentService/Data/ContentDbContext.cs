using ContentService.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContentService.Data
{
    public class ContentDbContext : DbContext
    {
        public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options) { }

        public DbSet<Content> Contents { get; set; }
        public DbSet<Film> Films { get; set; }
        public DbSet<Series> Series { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Game> Games { get; set; }

        public DbSet<Genre> Genres { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Tag> Tags { get; set; }

        public DbSet<ContentGenre> ContentGenres { get; set; }
        public DbSet<ContentLanguage> ContentLanguages { get; set; }
        public DbSet<ContentTag> ContentTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    
    modelBuilder.Entity<Content>(entity =>
    {
        entity.ToTable("Contents");
        entity.Property(e => e.ContentID).HasColumnName("ContentID");
        entity.Property(e => e.Title).HasColumnName("Title");
        entity.Property(e => e.PosterURL).HasColumnName("PosterURL");
        entity.Property(e => e.AverageRating).HasColumnName("AverageRating");
    });

    
    modelBuilder.Entity<Film>().ToTable("Films").Property(e => e.ContentID).HasColumnName("ContentID");
    modelBuilder.Entity<Game>().ToTable("Games").Property(e => e.ContentID).HasColumnName("ContentID");
    modelBuilder.Entity<Series>().ToTable("Series").Property(e => e.ContentID).HasColumnName("ContentID");
    modelBuilder.Entity<Book>().ToTable("Books").Property(e => e.ContentID).HasColumnName("ContentID");

    
    modelBuilder.Entity<Genre>().ToTable("Genres").Property(g => g.GenreID).HasColumnName("GenreID");
    modelBuilder.Entity<Language>().ToTable("Languages").Property(l => l.LanguageID).HasColumnName("LanguageID");
    modelBuilder.Entity<Tag>().ToTable("Tags").Property(t => t.TagID).HasColumnName("TagID");

    
    
    modelBuilder.Entity<ContentGenre>()
        .ToTable("ContentGenres")
        .HasKey(cg => new { cg.ContentID, cg.GenreID });

    modelBuilder.Entity<ContentLanguage>()
        .ToTable("ContentLanguages")
        .HasKey(cl => new { cl.ContentID, cl.LanguageID });

    modelBuilder.Entity<ContentTag>()
        .ToTable("ContentTags")
        .HasKey(ct => new { ct.ContentID, ct.TagID });
}
}
}
