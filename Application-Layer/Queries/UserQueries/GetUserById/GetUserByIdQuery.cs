using Application_Layer.DTOs;
using MediatR;

namespace Application_Layer.Queries.UserQueries.GetUserById
{
    public class GetUserByIdQuery : IRequest<GetUserByIdResponseDTO>
    {
        public string UserId { get; set; }

        public GetUserByIdQuery(string userId)
        {
            UserId = userId;
        }
    }
}
