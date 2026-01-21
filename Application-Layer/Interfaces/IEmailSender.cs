using System.Threading;
using System.Threading.Tasks;

namespace Application_Layer.Interfaces
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string plainText, string html, CancellationToken ct);
    }
}
