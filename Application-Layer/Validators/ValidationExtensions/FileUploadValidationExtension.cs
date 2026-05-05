using FluentValidation;

namespace Application_Layer.Validators.ValidationExtensions
{
    public static class FileUploadValidationExtension
    {
        private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
        private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];
        private const int MaxFileSizeBytes = 5 * 1024 * 1024;

        public static IRuleBuilderOptions<T, string> MustBeValidImageFileName<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .Must(name => AllowedExtensions.Contains(Path.GetExtension(name).ToLowerInvariant()))
                .WithMessage("Only .jpg, .jpeg, .png and .webp files are allowed.");
        }

        public static IRuleBuilderOptions<T, string> MustBeValidImageContentType<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .Must(ct => AllowedContentTypes.Contains(ct.ToLowerInvariant()))
                .WithMessage("Invalid content type. Only image files are allowed.");
        }

        public static IRuleBuilderOptions<T, byte[]> MustBeValidImageSize<T>(this IRuleBuilder<T, byte[]> ruleBuilder)
        {
            return ruleBuilder
                .Must(content => content.Length <= MaxFileSizeBytes)
                .WithMessage("File must not exceed 5 MB.");
        }
    }
}
