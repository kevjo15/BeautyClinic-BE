namespace Domain_Layer.Models
{
    public class ConversationModel
    {
        public Guid Id { get; set; }
        public List<Guid> ParticipantIds { get; set; } = new();
        public List<MessageModel> Messages { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
    }
}
