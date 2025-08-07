namespace FryerAgent;

/// <summary>
/// Request to fry sweet potato fries
/// </summary>
/// <param name="Portion">The portion size to fry</param>
/// <param name="Duration">The frying duration in minutes</param>
public record FrySweetPotatoRequest(string Portion, int Duration);
