namespace Orchestrator;

/// <summary>
/// Represents a step in the order processing progress
/// </summary>
public class ProgressStep
{
  public DateTime Timestamp { get; set; }
  public string Message { get; set; } = string.Empty;
}
