namespace PlatingAgent;

/// <summary>
/// Request to assemble a burger with specific components
/// </summary>
/// <param name="Components">The components to assemble the burger with</param>
public record AssembleBurgerRequest(string Components);
