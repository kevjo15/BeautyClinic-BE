using Domain_Layer.Models;

namespace Application_Layer.Interfaces
{
    public interface IConversationRepository
    {
        Task<ConversationModel?> GetByIdAsync(Guid id);
        Task CreateAsync(ConversationModel conversation);
        Task UpdateAsync(ConversationModel conversation);
        Task<List<MessageModel>> GetMessagesAsync(Guid conversationId);
        Task<List<ConversationModel>> GetAllAsync();
    }
}
