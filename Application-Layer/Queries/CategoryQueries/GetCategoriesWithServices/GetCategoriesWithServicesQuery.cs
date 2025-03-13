using MediatR;
using System.Collections.Generic;
using Application_Layer.DTOs;

namespace Application_Layer.Queries.CategoryQueries.GetCategoriesWithServices
{
    public class GetCategoriesWithServicesQuery : IRequest<IEnumerable<CategoryWithServicesDTO>>
    {
        // No parameters needed as we're getting all categories with their services
    }
}
