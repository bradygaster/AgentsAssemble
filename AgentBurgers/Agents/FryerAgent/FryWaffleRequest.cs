namespace FryerAgent;

/// <summary>
/// Request to fry waffle-cut french fries
/// </summary>
/// <param name="Portion">The portion size to fry</param>
/// <param name="Duration">The frying duration in minutes</param>
public record FryWaffleRequest(string Portion, int Duration);
