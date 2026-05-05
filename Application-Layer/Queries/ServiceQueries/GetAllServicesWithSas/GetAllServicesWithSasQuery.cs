using Application_Layer.DTOs;
using MediatR;
using System.Collections.Generic;

namespace Application_Layer.Queries.ServiceQueries.GetAllServicesWithSas
{
    public record GetAllServicesWithSasQuery : IRequest<IReadOnlyList<ServiceDTO>>;
}
