using Application_Layer.DTOs;
using MediatR;

namespace Application_Layer.Queries.ServiceQueries.GetAllServices
{
    public class GetAllServicesQuery : IRequest<IEnumerable<ServiceDTO>>
    {
    }
}
