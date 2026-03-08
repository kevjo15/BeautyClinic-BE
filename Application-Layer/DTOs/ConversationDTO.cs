namespace Application_Layer.DTOs
{
    public class ConversationDTO
    {
        public Guid Id { get; set; }
        public List<Guid> ParticipantIds { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
    }
}
