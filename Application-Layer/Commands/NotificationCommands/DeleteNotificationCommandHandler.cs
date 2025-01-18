using Infrastructure_Layer.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Application_Layer.Commands.NotificationCommands
{
    public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, Unit>
    {
        private readonly ElsaBeautyDbContext _context;

        public DeleteNotificationCommandHandler(ElsaBeautyDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == request.UserId, cancellationToken);

            if (notification == null)
                throw new KeyNotFoundException("Notification not found");

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
