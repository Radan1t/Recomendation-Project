using ContentService.Entities;
using ContentService.Repositories;
using Shared.DTO.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContentService.Services
{
    public class SeriesImportService
    {
        private readonly ISeriesRepository _seriesRepository;
        private readonly IGenreRepository _genreRepository;
        private readonly ITagRepository _tagRepository;
        private readonly ILanguageRepository _languageRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDictionarySyncService _syncService;

        private readonly Dictionary<string, Genre> _genreCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Tag> _tagCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Language> _languageCache = new(StringComparer.OrdinalIgnoreCase);

        public SeriesImportService(
            ISeriesRepository seriesRepository,
            IGenreRepository genreRepository,
            ITagRepository tagRepository,
            ILanguageRepository languageRepository,
            IUnitOfWork unitOfWork,
            IDictionarySyncService syncService)
        {
            _seriesRepository = seriesRepository;
            _genreRepository = genreRepository;
            _tagRepository = tagRepository;
            _languageRepository = languageRepository;
            _unitOfWork = unitOfWork;
            _syncService = syncService;
        }

        public async Task ImportSeriesAsync(SeriesImportDto dto)
        {
            if (await _seriesRepository.ExistsAsync(dto.ExternalID, dto.ExternalSource)) return;

            var newSeries = new Series
            {
                ExternalID = dto.ExternalID,
                ExternalSource = dto.ExternalSource,
                Title = dto.Title,
                Description = dto.Description,
                ReleaseDate = dto.ReleaseDate.ToUniversalTime(),
                AverageRating = dto.AverageRating,
                PosterURL = dto.PosterURL,
                SeasonCount = dto.SeasonCount,
                EpisodesCount = dto.EpisodesCount,
                Status = dto.Status,
                Network = dto.Network
            };

            var uniqueGenres = dto.Genres.Where(g => !string.IsNullOrWhiteSpace(g)).Select(g => g.Trim()).Distinct();
            foreach (var genreName in uniqueGenres)
            {
                if (!_genreCache.TryGetValue(genreName, out var genre))
                {
                    genre = await _genreRepository.GetByNameAsync(genreName);
                    if (genre == null)
                    {
                        genre = new Genre { Name = genreName };
                        await _genreRepository.AddAsync(genre);
                        await _unitOfWork.SaveChangesAsync();
                        await _syncService.SyncGenreAsync(genreName);
                    }
                    _genreCache[genreName] = genre;
                }
                newSeries.ContentGenres.Add(new ContentGenre { Genre = genre });
            }

            var uniqueTags = dto.Tags.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim().ToLower()).Distinct();
            foreach (var tagName in uniqueTags)
            {
                if (!_tagCache.TryGetValue(tagName, out var tag))
                {
                    tag = await _tagRepository.GetByNameAsync(tagName);
                    if (tag == null)
                    {
                        tag = new Tag { Name = tagName };
                        await _tagRepository.AddAsync(tag);
                        await _unitOfWork.SaveChangesAsync();
                        await _syncService.SyncTagAsync(tagName);
                    }
                    _tagCache[tagName] = tag;
                }
                newSeries.ContentTags.Add(new ContentTag { Tag = tag });
            }

            var uniqueLangs = dto.Languages.Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l.Trim().ToLower()).Distinct();
            foreach (var langCode in uniqueLangs)
            {
                if (!_languageCache.TryGetValue(langCode, out var language))
                {
                    language = await _languageRepository.GetByCodeAsync(langCode);
                    if (language == null)
                    {
                        language = new Language { Code = langCode, Name = langCode };
                        await _languageRepository.AddAsync(language);
                        await _unitOfWork.SaveChangesAsync();
                        await _syncService.SyncLanguageAsync(langCode);
                    }
                    _languageCache[langCode] = language;
                }
                newSeries.ContentLanguages.Add(new ContentLanguage { Language = language });
            }

            await _seriesRepository.AddAsync(newSeries);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
