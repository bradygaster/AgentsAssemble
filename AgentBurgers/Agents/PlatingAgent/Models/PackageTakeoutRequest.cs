namespace PlatingAgent;

/// <summary>
/// Request to package items for takeout
/// </summary>
/// <param name="Items">The items to package</param>
/// <param name="Accessories">Additional accessories to include</param>
public record PackageTakeoutRequest(string Items, string Accessories);
