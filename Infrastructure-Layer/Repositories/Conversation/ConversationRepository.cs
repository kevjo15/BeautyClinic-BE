using Application.Common.Interfaces;
using Domain_Layer.Models;
using Infrastructure_Layer.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure_Layer.Repositories.Conversation
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly ElsaBeautyDbContext _context;

        public ConversationRepository(ElsaBeautyDbContext context)
        {
            _context = context;
        }

        public async Task<ConversationModel?> GetByIdAsync(Guid id)
        {
            return await _context.Conversations.Include(c => c.Messages)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task CreateAsync(ConversationModel conversation)
        {
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ConversationModel conversation)
        {
            _context.Conversations.Update(conversation);
            await _context.SaveChangesAsync();
        }

        public async Task<List<MessageModel>> GetMessagesAsync(Guid conversationId)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .ToListAsync();
        }
        
        public async Task<List<ConversationModel>> GetAllAsync()
        {
            return await _context.Conversations.ToListAsync();
        }
    }
}
