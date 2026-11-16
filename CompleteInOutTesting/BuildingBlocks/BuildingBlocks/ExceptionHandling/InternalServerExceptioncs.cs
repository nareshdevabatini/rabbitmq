namespace BuildingBlocks.ExceptionHandling;

public class InternalServerExceptioncs : Exception
{
    public InternalServerExceptioncs(string message) : base(message)
    {
    }
    public InternalServerExceptioncs(string message, string details) : base(message)
    {
        Details = details;
    }
    public string? Details { get; set; }
}
