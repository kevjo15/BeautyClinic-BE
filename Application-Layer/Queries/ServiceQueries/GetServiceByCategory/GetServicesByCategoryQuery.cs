using MediatR;
using Application_Layer.DTOs;

public class GetServicesByCategoryQuery : IRequest<IEnumerable<ServiceDTO>>
{
    public Guid CategoryId { get; }

    public GetServicesByCategoryQuery(Guid categoryId)
    {
        CategoryId = categoryId;
    }
}
