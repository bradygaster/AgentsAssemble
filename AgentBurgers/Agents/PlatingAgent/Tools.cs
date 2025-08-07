using ModelContextProtocol.Server;
using System.ComponentModel;

namespace PlatingAgent;

[McpServerToolType]
public static class AssembleBurgerTool
{
    [McpServerTool, Description("Assemble a burger with specified components.")]
    public static string AssembleBurger(AssembleBurgerRequest request) => $"🍔 Assembling burger with {request.Components}... Perfectly assembled burger ready!";
}

[McpServerToolType]
public static class PlateMealTool
{
    [McpServerTool, Description("Plate a meal with proper presentation.")]
    public static string PlateMeal(PlateMealRequest request) => $"🍽️ Plating meal for {request.Service} with {request.Presentation}... Meal beautifully presented!";
}

[McpServerToolType]
public static class PackageTakeoutTool
{
    [McpServerTool, Description("Package food items for takeout.")]
    public static string PackageTakeout(PackageTakeoutRequest request) => $"📦 Packaging {request.Items} for takeout with {request.Accessories}... Order ready for pickup!";
}
