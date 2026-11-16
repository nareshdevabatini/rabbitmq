using Carter;
using EmployeeService.EndPoints.Employee.GetEmployee;
using Mapster;
using MediatR;


public sealed record GetEmployeeResponse(Employee Employee);

public sealed class GetEmployeeEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/employee/{id:int}", async (
            int id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new GetEmployeeQuery(id), cancellationToken);
            var response = result.Adapt<GetEmployeeResponse>();

            return Results.Ok(response);
        });
    }
}
