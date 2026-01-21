using System.Threading;
using System.Threading.Tasks;

namespace Application_Layer.Interfaces
{
    public interface ISmsSender
    {
        Task SendAsync(string toPhoneNumber, string message, CancellationToken ct);
    }
}
