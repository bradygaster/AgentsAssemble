namespace DessertAgent;

/// <summary>
/// Request to make a shake with specific flavor and toppings
/// </summary>
/// <param name="Size">The size of the shake</param>
/// <param name="Flavor">The flavor of the shake</param>
/// <param name="Toppings">Additional toppings for the shake</param>
public record MakeShakeRequest(string Size, string Flavor, string Toppings);
