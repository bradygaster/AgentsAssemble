using ModelContextProtocol.Server;
using System.ComponentModel;

namespace GrillAgent;

[McpServerToolType]
public static class CookPattyTool
{
    [McpServerTool, Description("Cook a burger patty to specified doneness.")]
    public static string CookPatty(CookPattyRequest request) => $"🥩 Cooking {request.PattyType} patty to {request.Doneness} doneness... Done! Perfectly cooked patty ready.";
}

[McpServerToolType]
public static class MeltCheeseTool
{
    [McpServerTool, Description("Melt cheese on a burger patty.")]
    public static string MeltCheese(MeltCheeseRequest request) => $"🧀 Melting {request.CheeseType} cheese on the patty... Perfect melt achieved!";
}

[McpServerToolType]
public static class AddBaconTool
{
    [McpServerTool, Description("Add crispy bacon strips to a burger.")]
    public static string AddBacon(AddBaconRequest request) => $"🥓 Adding {request.BaconStrips} strips of crispy bacon... Bacon perfectly placed!";
}

[McpServerToolType]
public static class ToastBunTool
{
    [McpServerTool, Description("Toast burger buns to specified level.")]
    public static string ToastBun(ToastBunRequest request) => $"🍞 Toasting {request.BunType} bun to {request.ToastLevel}... Golden brown perfection!";
}
