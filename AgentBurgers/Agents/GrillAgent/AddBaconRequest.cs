namespace GrillAgent;

/// <summary>
/// Request to add bacon strips to a burger
/// </summary>
/// <param name="BaconStrips">The number of bacon strips to add</param>
public record AddBaconRequest(int BaconStrips);
