using Application_Layer.DTO_s;
using Domain_Layer.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
