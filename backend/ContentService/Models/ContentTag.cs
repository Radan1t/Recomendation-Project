namespace ContentService.Entities
{
    public class ContentTag
    {
        public int ContentID { get; set; }
        public Content Content { get; set; }
        public int TagID { get; set; }
        public Tag Tag { get; set; }
    }
}
