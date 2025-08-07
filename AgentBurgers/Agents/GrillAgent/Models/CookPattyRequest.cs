namespace GrillAgent;

/// <summary>
/// Request to cook a patty to specific doneness
/// </summary>
/// <param name="PattyType">The type of patty to cook</param>
/// <param name="Doneness">The desired doneness level</param>
public record CookPattyRequest(string PattyType, string Doneness);
