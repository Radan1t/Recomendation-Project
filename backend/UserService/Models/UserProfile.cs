using System;
using System.Collections.Generic;

namespace UserService.Entities
{
    public class UserProfile
    {
        public int UserID { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Country { get; set; }
        public List<string> ContentPriorities { get; set; }
        public User User { get; set; }

        public ICollection<ProfileGenre> ProfileGenres { get; set; } = new List<ProfileGenre>();
        public ICollection<ProfileLanguage> ProfileLanguages { get; set; } = new List<ProfileLanguage>();
        public ICollection<ProfileTag> ProfileTags { get; set; } = new List<ProfileTag>();
    }
}
