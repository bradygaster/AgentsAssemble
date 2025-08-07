namespace Orchestrator.Models;

public class ToolDefinition
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string Station { get; set; }
    public object? Parameters { get; set; }
}
