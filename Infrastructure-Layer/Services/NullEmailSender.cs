using System.Threading;
using System.Threading.Tasks;
using Application_Layer.Interfaces;

namespace Infrastructure_Layer.Services
{
    public class NullEmailSender : IEmailSender
    {
        public Task SendAsync(string to, string subject, string plainText, string html, CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }
}
