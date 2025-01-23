using Application_Layer.DTO_s;
using MediatR;

namespace Application_Layer.Queries.ServiceQueries.GetAllServices
{
    public class GetAllServicesQuery : IRequest<IEnumerable<ServiceDTO>>
    {
    }
}