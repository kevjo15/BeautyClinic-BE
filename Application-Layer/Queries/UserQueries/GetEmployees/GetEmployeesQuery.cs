using Application_Layer.DTOs;
using MediatR;
using System.Collections.Generic;

namespace Application_Layer.Queries.UserQueries.GetEmployees
{
    public class GetEmployeesQuery : IRequest<List<EmployeeDTO>>
    {
    }
}
