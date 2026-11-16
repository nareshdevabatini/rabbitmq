using MediatR;

namespace BuildingBlocks.CQRS;

/// <summary>
/// Unit is a struct that represents a void return type for MediatR requests. It is used to indicate that a request does not return any value.
/// it is a singleton struct, meaning that there is only one instance of it, which can be accessed via the static property Unit.Value.
public interface ICommand : ICommand<Unit>
{
}
public interface ICommand<out TResponse>:IRequest<TResponse >
{
}
