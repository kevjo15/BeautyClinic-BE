using MediatR;
using Application_Layer.DTOs;

namespace Application_Layer.Queries.UserQueries.GetUserName
{
    public class GetUserNameQuery : IRequest<UserNameDTO>
    {
        public string UserId { get; }

        public GetUserNameQuery(string userId)
        {
            UserId = userId;
        }
    }
}
