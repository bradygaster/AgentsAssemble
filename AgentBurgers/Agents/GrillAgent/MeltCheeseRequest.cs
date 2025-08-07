namespace GrillAgent;

/// <summary>
/// Request to melt cheese on a patty
/// </summary>
/// <param name="CheeseType">The type of cheese to melt</param>
public record MeltCheeseRequest(string CheeseType);
