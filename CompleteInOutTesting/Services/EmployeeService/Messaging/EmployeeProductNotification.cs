namespace EmployeeService.Messaging;

public class EmployeeProductNotification
{
    public long Id { get; set; }
    public int ProductId { get; set; }
    public Guid SourceMessageId { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
}
