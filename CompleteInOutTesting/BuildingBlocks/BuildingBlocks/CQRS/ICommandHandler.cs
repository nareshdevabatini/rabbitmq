using MediatR;

namespace BuildingBlocks.CQRS;

/// <summary>
/// IcommandHandler and there is no reponse
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public interface ICommandHandler<in TCommand>: ICommandHandler<TCommand, Unit>
    where TCommand : ICommand<Unit>
{
    
}
//summary>
/// IcommandHandler is an interface that defines a handler for a command of type TCommand that returns a response of type TResponse. It extends the IRequestHandler interface from MediatR, which provides the basic functionality for handling requests and returning responses.
// TcommandHandler is an interface that defines a handler for a command of type TCommand that returns a response of type TResponse. It extends the IRequestHandler interface from MediatR, which provides the basic functionality for handling requests and returning responses.
/// <summary>
/// Tcommand is an interface that defines a command of type TCommand that returns a response of type TResponse. It extends the IRequest interface from MediatR, which provides the basic functionality for sending requests and receiving responses.
/// TResponse is a type parameter that represents the response type that the command handler will return. It is constrained to be a non-nullable type using the notnull constraint.
/// IRequestHandler is an interface from MediatR that defines a handler for a request of type TRequest that returns a response of type TResponse. It provides the basic functionality for handling requests and returning responses.
/// </summary>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
    where TResponse : notnull
{
}

