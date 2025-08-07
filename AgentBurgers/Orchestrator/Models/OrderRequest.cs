namespace Orchestrator;

/// <summary>
/// Represents a request to process an order
/// </summary>
/// <param name="Order">The order text from the customer</param>
public record OrderRequest(string Order);
