using Domain_Layer.Models;

namespace Application.Common.Interfaces
{
    public interface IMessageRepository
    {
        Task CreateAsync(MessageModel message);
    }
}
