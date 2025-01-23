using Application_Layer.DTO_s;

namespace Application_Layer.Commands.UserCommands.Update
{
    public class UpdateUserProfileResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public UpdateUserProfileDTO UpdatedUserProfile { get; set; }

        public UpdateUserProfileResult(bool success, UpdateUserProfileDTO updatedUserProfile, List<string> errors = null)
        {
            Success = success;
            UpdatedUserProfile = updatedUserProfile;
            Errors = errors ?? new List<string>();
        }
    }
}
