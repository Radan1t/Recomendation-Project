using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.DTO.User;
using UserService.Data;
using UserService.Entities;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/v1/user")]
    public class UserController : ControllerBase
    {
        private readonly UserDbContext _db;
        public UserController(UserDbContext db) { _db = db; }

        [HttpGet("profile/{id}")]
        public async Task<IActionResult> GetProfile(int id)
        {
            var user = await _db.Users
                .Include(u => u.UserProfile)
                    .ThenInclude(p => p.ProfileGenres)
                .Include(u => u.UserProfile)
                    .ThenInclude(p => p.ProfileTags)
                        .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null) return NotFound();

            var profile = user.UserProfile;

            var result = new FrontendUserProfileDto
            {
                DisplayName = string.IsNullOrEmpty(user.Name) && string.IsNullOrEmpty(user.Surname) ? "" : (user.Name + " " + user.Surname).Trim(),
                Email = user.Email,
                Bio = "",
                SelectedGenreIds = profile?.ProfileGenres?.Select(pg => pg.GenreID).ToList() ?? new List<int>(),
                Tags = profile?.ProfileTags?.Select(pt => pt.Tag?.Name ?? string.Empty).Where(t => !string.IsNullOrEmpty(t)).ToList() ?? new List<string>()
            };

            return Ok(result);
        }

        [HttpPut("profile/{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] FrontendUserProfileDto dto)
        {
            var user = await _db.Users.Include(u => u.UserProfile).FirstOrDefaultAsync(u => u.UserID == id);
            if (user == null) return NotFound();

            // update basic user fields
            if (!string.IsNullOrEmpty(dto.DisplayName))
            {
                var parts = dto.DisplayName.Split(' ', 2);
                user.Name = parts.Length > 0 ? parts[0] : dto.DisplayName;
                user.Surname = parts.Length > 1 ? parts[1] : user.Surname;
            }
            if (!string.IsNullOrEmpty(dto.Email)) user.Email = dto.Email;

            // ensure profile exists
            if (user.UserProfile == null)
            {
                user.UserProfile = new UserProfile { UserID = user.UserID, ContentPriorities = new List<string>() };
                _db.UserProfiles.Add(user.UserProfile);
                // Persist profile immediately so FK references (ProfileGenres/ProfileTags) are valid
                await _db.SaveChangesAsync();
            }

            var profile = user.UserProfile;

            // update tags: ensure Tag entities exist and update ProfileTags
            var existingTags = await _db.Tags.Where(t => dto.Tags.Contains(t.Name)).ToListAsync();
            var missingNames = dto.Tags.Except(existingTags.Select(t => t.Name)).Distinct();
            foreach (var name in missingNames)
            {
                var newTag = new Tag { Name = name };
                _db.Tags.Add(newTag);
                existingTags.Add(newTag);
            }
            // ensure new tags have IDs (and any newly created profile is persisted)
            await _db.SaveChangesAsync();

            // replace ProfileTags
            var currentProfileTags = _db.ProfileTags.Where(pt => pt.UserID == id);
            _db.ProfileTags.RemoveRange(currentProfileTags);
            foreach (var tag in existingTags)
            {
                _db.ProfileTags.Add(new ProfileTag { UserID = id, TagID = tag.TagID, UserProfile = profile });
            }

            // replace ProfileGenres
            var genreIds = dto.SelectedGenreIds?.Distinct().ToList() ?? new List<int>();
            if (genreIds.Any())
            {
                var existingGenreIds = await _db.Genres.Where(g => genreIds.Contains(g.GenreID)).Select(g => g.GenreID).ToListAsync();
                var missingGenreIds = genreIds.Except(existingGenreIds).ToList();
                if (missingGenreIds.Any())
                {
                    return BadRequest(new { message = "Some genre IDs are invalid", missing = missingGenreIds });
                }
            }

            var currentGenres = _db.ProfileGenres.Where(pg => pg.UserID == id);
            _db.ProfileGenres.RemoveRange(currentGenres);
            foreach (var gid in genreIds)
            {
                _db.ProfileGenres.Add(new ProfileGenre { UserID = id, GenreID = gid, UserProfile = profile });
            }

            await _db.SaveChangesAsync();

            return Ok(new { message = "Profile updated" });
        }
    }
}
