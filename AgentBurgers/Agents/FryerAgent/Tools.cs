using ModelContextProtocol.Server;
using System.ComponentModel;

namespace FryerAgent;

[McpServerToolType]
public static class FryStandardTool
{
    [McpServerTool, Description("Fry standard French fries.")]
    public static string FryStandard(FryStandardRequest request) => $"🍟 Frying {request.Portion} portion of standard fries for {request.Duration} minutes... Crispy golden fries ready!";
}

[McpServerToolType]
public static class FryWaffleTool
{
    [McpServerTool, Description("Fry waffle-cut French fries.")]
    public static string FryWaffle(FryWaffleRequest request) => $"🧇 Frying {request.Portion} portion of waffle fries for {request.Duration} minutes... Crispy waffle-cut fries ready!";
}

[McpServerToolType]
public static class FrySweetPotatoTool
{
    [McpServerTool, Description("Fry sweet potato fries.")]
    public static string FrySweetPotato(FrySweetPotatoRequest request) => $"🍠 Frying {request.Portion} portion of sweet potato fries for {request.Duration} minutes... Delicious sweet potato fries ready!";
}
