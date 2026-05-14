
using System;
using ContentService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ContentService.Migrations
{
    [DbContext(typeof(ContentDbContext))]
    partial class ContentDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.24")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ContentService.Entities.Content", b =>
                {
                    b.Property<int>("ContentID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ContentID"));

                    b.Property<double>("AverageRating")
                        .HasColumnType("double precision");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ExternalID")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ExternalSource")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PosterURL")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("ReleaseDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ContentID");

                    b.ToTable("Contents");

                    b.UseTptMappingStrategy();
                });

            modelBuilder.Entity("ContentService.Entities.ContentGenre", b =>
                {
                    b.Property<int>("ContentID")
                        .HasColumnType("integer");

                    b.Property<int>("GenreID")
                        .HasColumnType("integer");

                    b.HasKey("ContentID", "GenreID");

                    b.HasIndex("GenreID");

                    b.ToTable("ContentGenres");
                });

            modelBuilder.Entity("ContentService.Entities.ContentLanguage", b =>
                {
                    b.Property<int>("ContentID")
                        .HasColumnType("integer");

                    b.Property<int>("LanguageID")
                        .HasColumnType("integer");

                    b.HasKey("ContentID", "LanguageID");

                    b.HasIndex("LanguageID");

                    b.ToTable("ContentLanguages");
                });

            modelBuilder.Entity("ContentService.Entities.ContentTag", b =>
                {
                    b.Property<int>("ContentID")
                        .HasColumnType("integer");

                    b.Property<int>("TagID")
                        .HasColumnType("integer");

                    b.HasKey("ContentID", "TagID");

                    b.HasIndex("TagID");

                    b.ToTable("ContentTags");
                });

            modelBuilder.Entity("ContentService.Entities.Genre", b =>
                {
                    b.Property<int>("GenreID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("GenreID"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("GenreID");

                    b.ToTable("Genres");
                });

            modelBuilder.Entity("ContentService.Entities.Language", b =>
                {
                    b.Property<int>("LanguageID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("LanguageID"));

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LanguageID");

                    b.ToTable("Languages");
                });

            modelBuilder.Entity("ContentService.Entities.Tag", b =>
                {
                    b.Property<int>("TagID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("TagID"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("TagID");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("ContentService.Entities.Book", b =>
                {
                    b.HasBaseType("ContentService.Entities.Content");

                    b.Property<string>("Author")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("BookSeries")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ISBN")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Pages")
                        .HasColumnType("integer");

                    b.Property<string>("Publisher")
                        .IsRequired()
                        .HasColumnType("text");

                    b.ToTable("Books", (string)null);
                });

            modelBuilder.Entity("ContentService.Entities.Film", b =>
                {
                    b.HasBaseType("ContentService.Entities.Content");

                    b.Property<decimal>("BoxOffice")
                        .HasColumnType("numeric");

                    b.Property<string>("Director")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("DurationMinutes")
                        .HasColumnType("integer");

                    b.ToTable("Films", (string)null);
                });

            modelBuilder.Entity("ContentService.Entities.Game", b =>
                {
                    b.HasBaseType("ContentService.Entities.Content");

                    b.Property<string>("Developer")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PEGIRating")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Publisher")
                        .IsRequired()
                        .HasColumnType("text");

                    b.ToTable("Games", (string)null);
                });

            modelBuilder.Entity("ContentService.Entities.Series", b =>
                {
                    b.HasBaseType("ContentService.Entities.Content");

                    b.Property<int>("EpisodesCount")
                        .HasColumnType("integer");

                    b.Property<string>("Network")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("SeasonCount")
                        .HasColumnType("integer");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.ToTable("Series", (string)null);
                });

            modelBuilder.Entity("ContentService.Entities.ContentGenre", b =>
                {
                    b.HasOne("ContentService.Entities.Content", "Content")
                        .WithMany("ContentGenres")
                        .HasForeignKey("ContentID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ContentService.Entities.Genre", "Genre")
                        .WithMany("ContentGenres")
                        .HasForeignKey("GenreID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Content");

                    b.Navigation("Genre");
                });

            modelBuilder.Entity("ContentService.Entities.ContentLanguage", b =>
                {
                    b.HasOne("ContentService.Entities.Content", "Content")
                        .WithMany("ContentLanguages")
                        .HasForeignKey("ContentID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ContentService.Entities.Language", "Language")
                        .WithMany("ContentLanguages")
                        .HasForeignKey("LanguageID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Content");

                    b.Navigation("Language");
                });

            modelBuilder.Entity("ContentService.Entities.ContentTag", b =>
                {
                    b.HasOne("ContentService.Entities.Content", "Content")
                        .WithMany("ContentTags")
                        .HasForeignKey("ContentID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ContentService.Entities.Tag", "Tag")
                        .WithMany("ContentTags")
                        .HasForeignKey("TagID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Content");

                    b.Navigation("Tag");
                });

            modelBuilder.Entity("ContentService.Entities.Book", b =>
                {
                    b.HasOne("ContentService.Entities.Content", null)
                        .WithOne()
                        .HasForeignKey("ContentService.Entities.Book", "ContentID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ContentService.Entities.Film", b =>
                {
                    b.HasOne("ContentService.Entities.Content", null)
                        .WithOne()
                        .HasForeignKey("ContentService.Entities.Film", "ContentID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ContentService.Entities.Game", b =>
                {
                    b.HasOne("ContentService.Entities.Content", null)
                        .WithOne()
                        .HasForeignKey("ContentService.Entities.Game", "ContentID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ContentService.Entities.Series", b =>
                {
                    b.HasOne("ContentService.Entities.Content", null)
                        .WithOne()
                        .HasForeignKey("ContentService.Entities.Series", "ContentID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ContentService.Entities.Content", b =>
                {
                    b.Navigation("ContentGenres");

                    b.Navigation("ContentLanguages");

                    b.Navigation("ContentTags");
                });

            modelBuilder.Entity("ContentService.Entities.Genre", b =>
                {
                    b.Navigation("ContentGenres");
                });

            modelBuilder.Entity("ContentService.Entities.Language", b =>
                {
                    b.Navigation("ContentLanguages");
                });

            modelBuilder.Entity("ContentService.Entities.Tag", b =>
                {
                    b.Navigation("ContentTags");
                });
#pragma warning restore 612, 618
        }
    }
}

