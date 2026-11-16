namespace BuildingBlocks.ExceptionHandling;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
    public NotFoundException(string name, object key) : base($"Entity of type {name} with key {key} was not found.")
    {
    }
}
