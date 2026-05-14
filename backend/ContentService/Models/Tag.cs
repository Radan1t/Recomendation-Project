using System.Collections.Generic;

namespace ContentService.Entities
{
    public class Tag
    {
        public int TagID { get; set; }
        public string Name { get; set; }
        public ICollection<ContentTag> ContentTags { get; set; }
    }
}
