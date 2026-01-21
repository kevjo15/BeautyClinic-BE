using System.Threading;
using System.Threading.Tasks;
using Application_Layer.Interfaces;

namespace Infrastructure_Layer.Services
{
    public class NullSmsSender : ISmsSender
    {
        public Task SendAsync(string toPhoneNumber, string message, CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }
}
