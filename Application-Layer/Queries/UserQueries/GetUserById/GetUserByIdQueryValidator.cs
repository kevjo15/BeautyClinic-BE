using Application_Layer.Validators.ValidationExtensions;
using FluentValidation;

namespace Application_Layer.Queries.UserQueries.GetUserById
{
    public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
    {
        public GetUserByIdQueryValidator()
        {
            RuleFor(x => x.UserId).MustBeValidGuidId();
        }
    }
}