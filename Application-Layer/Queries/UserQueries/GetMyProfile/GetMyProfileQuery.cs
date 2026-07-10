using Application_Layer.DTOs;
using MediatR;

namespace Application_Layer.Queries.UserQueries.GetMyProfile
{
    public record GetMyProfileQuery(string UserId) : IRequest<UserProfileDTO?>;
}
