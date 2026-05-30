using System.Collections.Generic;

namespace Shared.DTO.User
{
    public class FrontendUserProfileDto
    {
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Bio { get; set; }
        public List<int> SelectedGenreIds { get; set; } = new List<int>();
        public List<string> Tags { get; set; } = new List<string>();
    }
}
