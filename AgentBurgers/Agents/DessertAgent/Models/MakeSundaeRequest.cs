namespace DessertAgent;

/// <summary>
/// Request to make a sundae with specific flavor and toppings
/// </summary>
/// <param name="Size">The size of the sundae</param>
/// <param name="Flavor">The flavor of the ice cream</param>
/// <param name="Toppings">Toppings for the sundae</param>
public record MakeSundaeRequest(string Size, string Flavor, string Toppings);
