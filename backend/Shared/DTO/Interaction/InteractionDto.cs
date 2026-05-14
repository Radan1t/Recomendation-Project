namespace Shared.DTO.Interaction;
public class InteractionDto
{
    public int InteractionId { get; set; }
    public int ContentId { get; set; }
    public string ActionType { get; set; }
    public DateTime ActionDate { get; set; }
}

