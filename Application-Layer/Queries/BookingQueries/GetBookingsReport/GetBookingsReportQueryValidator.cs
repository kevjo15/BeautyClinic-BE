using FluentValidation;

namespace Application_Layer.Queries.BookingQueries.GetBookingsReport
{
    public class GetBookingsReportQueryValidator : AbstractValidator<GetBookingsReportQuery>
    {
        public GetBookingsReportQueryValidator()
        {
            RuleFor(x => x.To)
                .GreaterThanOrEqualTo(x => x.From)
                .WithMessage("End date must be on or after start date.");

            RuleFor(x => x)
                .Must(x => (x.To.Date - x.From.Date).TotalDays <= 366)
                .WithMessage("Report range cannot exceed one year.");
        }
    }
}
