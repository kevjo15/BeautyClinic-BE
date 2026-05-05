using MediatR;
using Application_Layer.DTOs;

namespace Application_Layer.Queries.ServiceQueries
{
    public class GetServiceByNameQuery : IRequest<IEnumerable<ServiceDTO>>
    {
        public string Name { get; }

        public GetServiceByNameQuery(string name)
        {
            Name = name;
        }
    }
}
