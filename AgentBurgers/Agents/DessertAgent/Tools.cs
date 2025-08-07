using ModelContextProtocol.Server;
using System.ComponentModel;

namespace DessertAgent;

[McpServerToolType]
public static class ShakeTool
{
    [McpServerTool, Description("Make a milkshake.")]
    public static string MakeShake(MakeShakeRequest request) => $"🥤 Making {request.Size} {request.Flavor} shake with {request.Toppings}... Creamy shake ready!";
}

[McpServerToolType]
public static class SundaeTool
{
    [McpServerTool, Description("Make a sundae.")]
    public static string MakeShake(MakeSundaeRequest request) => $"🍨 Making {request.Size} sundae with {request.Flavor} ice cream and {request.Toppings}... Delicious sundae ready!";
}

[McpServerToolType]
public static class WhippedCreamTool
{
    [McpServerTool, Description("Add whipped cream to a dessert.")]
    public static string MakeWhippedCream(AddWhippedCreamRequest request) => $"🍦 Adding {request.Amount} whipped cream... Perfect fluffy topping added!";
}
