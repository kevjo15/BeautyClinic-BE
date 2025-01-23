using Application_Layer.Interfaces;
using Domain_Layer.Models;
using Infrastructure_Layer.Database;

namespace Infrastructure_Layer.Repositories.Message
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ElsaBeautyDbContext _context;

        public MessageRepository(ElsaBeautyDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(MessageModel message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
        }
    }
}
