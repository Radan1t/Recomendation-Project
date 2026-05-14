using ContentService.Data;

using ContentService.Services;
using ContentService.Repositories; 
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);




builder.Services.AddDbContext<ContentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));





builder.Services.AddScoped<IContentService, ContentService.Services.ContentService>();





builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IGenreRepository, GenreRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IFilmRepository, FilmRepository>();
builder.Services.AddScoped<ISeriesRepository, SeriesRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<ILanguageRepository, LanguageRepository>();


builder.Services.AddScoped<FilmImportService>();
builder.Services.AddScoped<SeriesImportService>();
builder.Services.AddScoped<GameImportService>();
builder.Services.AddScoped<BookImportService>();


builder.Services.AddHttpClient<TmdbIntegrationService>();
builder.Services.AddHttpClient<GoogleBooksIntegrationService>();
builder.Services.AddHttpClient<IDictionarySyncService, DictionarySyncService>(); 
builder.Services.AddHttpClient<RawgIntegrationService>(); 






builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });
    
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();




using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var maxRetries = 5;

    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            var context = services.GetRequiredService<ContentDbContext>();
            context.Database.Migrate();
            logger.LogInformation("Міграції ContentDb успішно застосовано!");
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning($"Помилка підключення до БД (спроба {i + 1} з {maxRetries}). Чекаємо 3 секунди...");
            if (i == maxRetries - 1)
            {
                logger.LogError(ex, "Критична помилка: не вдалося підключитися до бази даних після всіх спроб.");
            }
            else
            {
                System.Threading.Thread.Sleep(3000);
            }
        }
    }
}




if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();







app.Run();
