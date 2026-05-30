using ContentService.Entities;
using ContentService.Repositories;
using Shared.DTO.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ContentService.Services
{
    public class BookImportService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IGenreRepository _genreRepository;
        private readonly ITagRepository _tagRepository;
        private readonly ILanguageRepository _languageRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDictionarySyncService _syncService;
        private readonly ILogger<BookImportService> _logger;

        private readonly Dictionary<string, Genre> _genreCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Tag> _tagCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Language> _languageCache = new(StringComparer.OrdinalIgnoreCase);

        public BookImportService(
            IBookRepository bookRepo,
            IGenreRepository genreRepo,
            ITagRepository tagRepo,
            ILanguageRepository languageRepo,
            IUnitOfWork uow,
            IDictionarySyncService syncService,
            ILogger<BookImportService> logger)
        {
            _bookRepository = bookRepo;
            _genreRepository = genreRepo;
            _tagRepository = tagRepo;
            _languageRepository = languageRepo;
            _unitOfWork = uow;
            _syncService = syncService;
            _logger = logger;
        }

        public async Task ImportBookAsync(BookImportDto dto)
        {
            try
            {
                if (await _bookRepository.ExistsAsync(dto.ExternalID, dto.ExternalSource))
                {
                    _logger.LogDebug($"[BookImport] Книга вже існує: {dto.Title} ({dto.ExternalID})");
                    return;
                }

                var newBook = new Book
                {
                    ExternalID = dto.ExternalID,
                    ExternalSource = dto.ExternalSource,
                    Title = dto.Title,
                    Description = dto.Description,
                    ReleaseDate = dto.ReleaseDate.ToUniversalTime(),
                    AverageRating = dto.AverageRating,
                    PosterURL = dto.PosterURL,
                    Author = dto.Author,
                    Pages = dto.Pages,
                    ISBN = dto.ISBN,
                    Publisher = dto.Publisher,
                    BookSeries = dto.BookSeries
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
                    newBook.ContentGenres.Add(new ContentGenre { Genre = genre });
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
                    newBook.ContentTags.Add(new ContentTag { Tag = tag });
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
                    newBook.ContentLanguages.Add(new ContentLanguage { Language = language });
                }

                await _bookRepository.AddAsync(newBook);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"[BookImport] Успішно імпортовано: {dto.Title}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[BookImport] Помилка при імпорті книги: {dto.Title}");
                throw;
            }
        }

        // Wrapper that returns import result for debugging
        public async Task<Shared.DTO.Content.ImportResultDto> ImportBookWithResultAsync(BookImportDto dto)
        {
            try
            {
                if (await _bookRepository.ExistsAsync(dto.ExternalID, dto.ExternalSource))
                {
                    _logger.LogDebug($"[BookImport] Книга вже існує: {dto.Title} ({dto.ExternalID})");
                    return new Shared.DTO.Content.ImportResultDto { ExternalID = dto.ExternalID, Title = dto.Title, Success = false, ErrorMessage = "Already exists" };
                }

                await ImportBookAsync(dto); // uses existing method which performs add & save

                return new Shared.DTO.Content.ImportResultDto { ExternalID = dto.ExternalID, Title = dto.Title, Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[BookImport] ImportBookWithResultAsync failed for {dto.Title}");
                return new Shared.DTO.Content.ImportResultDto { ExternalID = dto.ExternalID, Title = dto.Title, Success = false, ErrorMessage = ex.Message };
            }
        }
    }
}

