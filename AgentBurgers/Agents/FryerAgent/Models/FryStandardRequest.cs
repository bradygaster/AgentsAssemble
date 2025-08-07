namespace FryerAgent;

/// <summary>
/// Request to fry standard french fries
/// </summary>
/// <param name="Portion">The portion size to fry</param>
/// <param name="Duration">The frying duration in minutes</param>
public record FryStandardRequest(string Portion, int Duration);
