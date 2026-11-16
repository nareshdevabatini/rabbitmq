
//public record GetEmployeeQuery(string UserName);
using Carter;
using EmployeeService.EndPoints.Employee.GetEmployee;
using Mapster;
using MediatR;

public record GetEmployeeResponse(Employee employee);
public class GetEmployeeEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/employee/{id:int}", async (int id, ISender sender) =>
        {
            var result = await sender.Send(new GetEmployeeQuery(id));

            var response = result.Adapt<GetEmployeeResult>();

            return Results.Ok(response);
        });
    }
}
