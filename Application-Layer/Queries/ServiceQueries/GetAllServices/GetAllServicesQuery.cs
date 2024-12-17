using Application_Layer.DTO_s;
using MediatR;
using System.Collections.Generic;

namespace Application_Layer.Queries.ServiceQueries.GetAllServices
{
    public class GetAllServicesQuery : IRequest<IEnumerable<ServiceDTO>>
    {
    }
} 