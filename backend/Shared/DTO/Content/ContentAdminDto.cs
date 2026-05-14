namespace Shared.DTO.Content
{
    
    public class ContentAdminListDto
    {
        public int Id { get; set; } 
        public string Title { get; set; }
        public string Type { get; set; } 
    }

    
    public class ContentUpdateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
