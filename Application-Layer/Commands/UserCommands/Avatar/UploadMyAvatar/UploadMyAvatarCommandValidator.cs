using Application_Layer.Validators.ValidationExtensions;
using FluentValidation;

namespace Application_Layer.Commands.UserCommands.Avatar.UploadMyAvatar
{
    public class UploadMyAvatarCommandValidator : AbstractValidator<UploadMyAvatarCommand>
    {
        public UploadMyAvatarCommandValidator()
        {
            RuleFor(x => x.UserId)
                .MustBeValidGuidId();

            RuleFor(x => x.File.FileName)
                .MustBeValidImageFileName();

            RuleFor(x => x.File.ContentType)
                .MustBeValidImageContentType();

            RuleFor(x => x.File.Content)
                .MustBeValidImageSize();
        }
    }
}
