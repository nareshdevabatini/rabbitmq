namespace EmployeeService.Messaging;

public class InboxMessage
{
    public Guid MessageId { get; set; }
    public DateTime ReceivedAtUtc { get; set; }
}
