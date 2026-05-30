using System.Collections.Generic;

namespace Shared.DTO.Content
{
    public class ContentListResultDto
    {
        public List<ContentListItemDto> Items { get; set; } = new List<ContentListItemDto>();
        public int Total { get; set; }
    }
}
