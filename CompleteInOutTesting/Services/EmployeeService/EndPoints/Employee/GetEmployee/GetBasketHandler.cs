
using BuildingBlocks.CQRS;
using EmployeeService.Respository;

namespace EmployeeService.EndPoints.Employee.GetEmployee;

public record GetEmployeeQuery(int Id) : IQuery<GetEmployeeResult>;
public record GetEmployeeResult(Models.Employee Employee);
public class GetEmployeeQueryHandler(IEmployeeRepository employeeRepository) : IQueryHandler<GetEmployeeQuery, GetEmployeeResult>
{
    public async Task<GetEmployeeResult> Handle(GetEmployeeQuery query, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetEmployeeByIdAsync(query.Id, cancellationToken);

        return new GetEmployeeResult(employee);
    }
}
