using Application_Layer.DTO_s;
using MediatR;
using System.Collections.Generic;

namespace Application_Layer.Features.Services.Queries.GetAllServicesWithSas
{
    public record GetAllServicesWithSasQuery : IRequest<IReadOnlyList<ServiceDTO>>;
}
