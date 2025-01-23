using Domain_Layer.Models;

namespace Application_Layer.Interfaces
{
    public interface IMessageRepository
    {
        Task CreateAsync(MessageModel message);
    }
}
