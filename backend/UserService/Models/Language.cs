using System.Collections.Generic;

namespace UserService.Entities
{
    public class Language
    {
        public int LanguageID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public ICollection<ProfileLanguage> ProfileLanguages { get; set; }
    }
}
