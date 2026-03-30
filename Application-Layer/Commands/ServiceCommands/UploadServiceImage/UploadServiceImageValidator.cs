using FluentValidation;

namespace Application_Layer.Commands.ServiceCommands.UploadServiceImage
{
    public sealed class UploadServiceImageValidator : AbstractValidator<UploadServiceImageCommand>
    {
        public UploadServiceImageValidator()
        {
            RuleFor(x => x.ServiceId).NotEmpty();
            RuleFor(x => x.File).NotNull();
            RuleFor(x => x.File.Content).NotNull();
            RuleFor(x => x.File.Content.Length).GreaterThan(0);
        }
    }
}
