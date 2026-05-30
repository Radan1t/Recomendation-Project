using System;

namespace Shared.DTO.Content
{
    public class ImportResultDto
    {
        public string ExternalID { get; set; }
        public string Title { get; set; }
        public bool Success { get; set; }
        public int? ContentID { get; set; }
        public string ErrorMessage { get; set; }
    }
}
