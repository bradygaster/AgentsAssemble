namespace GrillAgent;

/// <summary>
/// Request to toast a bun to specific level
/// </summary>
/// <param name="BunType">The type of bun to toast</param>
/// <param name="ToastLevel">The desired toast level</param>
public record ToastBunRequest(string BunType, string ToastLevel);
