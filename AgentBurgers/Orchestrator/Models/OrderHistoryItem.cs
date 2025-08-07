namespace Orchestrator;

/// <summary>
/// Represents a historical order item with tracking information
/// </summary>
public class OrderHistoryItem
{
  public string Id { get; set; } = string.Empty;
  public DateTime Timestamp { get; set; }
  public string OrderText { get; set; } = string.Empty;
  public OrderStatus Status { get; set; }
  public DateTime? CompletedAt { get; set; }
  public string Response { get; set; } = string.Empty;
  public List<ProgressStep> ProgressSteps { get; set; } = new();
}
