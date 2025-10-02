using FluentValidation;

namespace Application_Layer.Features.Services.Commands.UploadServiceImage
{
    public sealed class UploadServiceImageValidator : AbstractValidator<UploadServiceImageCommand>
    {
        public UploadServiceImageValidator()
        {
            RuleFor(x => x.ServiceId).NotEmpty();
            RuleFor(x => x.File).NotNull();
        }
    }
}
