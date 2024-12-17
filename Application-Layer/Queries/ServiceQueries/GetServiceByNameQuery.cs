using MediatR;
using System.Collections.Generic;
using Application_Layer.DTO_s;

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