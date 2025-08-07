namespace PlatingAgent;

/// <summary>
/// Request to plate a meal for specific service style
/// </summary>
/// <param name="Service">The type of service (dine-in, takeout, etc.)</param>
/// <param name="Presentation">The presentation style</param>
public record PlateMealRequest(string Service, string Presentation);
