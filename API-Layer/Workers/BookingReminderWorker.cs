using Application_Layer.Commands.BookingCommands.SendDueBookingReminders;
using MediatR;

namespace API_Layer.Workers
{
    /// <summary>
    /// Periodiskt bakgrundsjobb som skickar bokningspåminnelser. All logik ligger i
    /// <see cref="SendDueBookingRemindersCommand"/> (testbar, loggas av MediatR-pipelinen);
    /// den här klassen är bara timern. Kommandots beroenden är scoped
    /// (INotificationService, DbContext), så varje tick kör i en egen DI-scope.
    ///
    /// Config (sektion "Reminders"): Enabled (default true), LeadTimeHours (default 24),
    /// PollIntervalMinutes (default 5).
    /// </summary>
    public class BookingReminderWorker : BackgroundService
    {
        private static readonly TimeSpan StartupDelay = TimeSpan.FromSeconds(30);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BookingReminderWorker> _logger;

        public BookingReminderWorker(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<BookingReminderWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_configuration.GetValue("Reminders:Enabled", true))
            {
                _logger.LogInformation("Booking reminders are disabled (Reminders:Enabled=false)");
                return;
            }

            var leadTimeHours = _configuration.GetValue("Reminders:LeadTimeHours", 24);
            var pollInterval = TimeSpan.FromMinutes(_configuration.GetValue("Reminders:PollIntervalMinutes", 5));

            _logger.LogInformation(
                "Booking reminder worker started (lead time {LeadTimeHours}h, poll interval {PollInterval})",
                leadTimeHours, pollInterval);

            try
            {
                // Låt migrering/seedning bli klar innan första körningen.
                await Task.Delay(StartupDelay, stoppingToken);

                using var timer = new PeriodicTimer(pollInterval);
                do
                {
                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        await mediator.Send(new SendDueBookingRemindersCommand(leadTimeHours), stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        // Loopen får aldrig dö — nästa tick försöker igen.
                        _logger.LogError(ex, "Booking reminder run failed");
                    }
                } while (await timer.WaitForNextTickAsync(stoppingToken));
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown.
            }
        }
    }
}
