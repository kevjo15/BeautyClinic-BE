using Application_Layer.Validators.ValidationExtensions;
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
            RuleFor(x => x.File.Content.Length).GreaterThan(0).WithMessage("File cannot be empty.");
            RuleFor(x => x.File.Content).MustBeValidImageSize();
            RuleFor(x => x.File.FileName).MustBeValidImageFileName();
            RuleFor(x => x.File.ContentType).MustBeValidImageContentType();
        }
    }
}
