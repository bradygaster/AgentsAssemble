using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using AgentBurgers.Web.Models;
using AgentBurgers.Web.Tools;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace AgentBurgers.Web.Services;

public class KitchenWorkflowService
{
    private readonly IChatClient _chatClient;
    private readonly ILogger<KitchenWorkflowService> _logger;

    public KitchenWorkflowService(IChatClient chatClient, ILogger<KitchenWorkflowService> logger)
    {
        _chatClient = chatClient;
        _logger = logger;
    }

    public async Task<string> ProcessOrderAsync(CustomerOrder order)
    {
        try
        {
            _logger.LogInformation("🚀 WORKFLOW: Starting order processing for {CustomerName}", order.CustomerName);
            // Create kitchen agents using AITool factory methods
            var grillTools = new List<AITool>
            {
                GrillTools.CreateCookPattyTool(_logger),
                GrillTools.CreateMeltCheeseTool(_logger),
                GrillTools.CreateToastBunTool(_logger)
            };

            var grillAgent = _chatClient.CreateAIAgent(
                name: "GrillMaster",
                instructions: "You are the grill station chef. Cook patties, melt cheese, and toast buns using your tools.",
                tools: grillTools
            );

            var fryerTools = new List<AITool>
            {
                FryerTools.CreateFryFriesTool(_logger),
                FryerTools.CreateSeasonFriesTool(_logger)
            };

            var fryerAgent = _chatClient.CreateAIAgent(
                name: "FryerChef", 
                instructions: "You handle all frying operations.",
                tools: fryerTools
            );

            var dessertTools = new List<AITool>
            {
                DessertTools.CreateMakeShakeTool(_logger),
                DessertTools.CreateAddWhippedCreamTool(_logger)
            };

            var dessertAgent = _chatClient.CreateAIAgent(
                name: "DessertChef",
                instructions: "You make desserts and drinks.",
                tools: dessertTools
            );

            var platingTools = new List<AITool>
            {
                PlatingTools.CreateAssembleBurgerTool(_logger),
                PlatingTools.CreatePackageMealTool(_logger)
            };

            var platingAgent = _chatClient.CreateAIAgent(
                name: "PlatingChef",
                instructions: "You assemble and package meals.",
                tools: platingTools
            );

            // Build workflow
            var workflow = new WorkflowBuilder(grillAgent)
                .AddEdge(grillAgent, fryerAgent)
                .AddEdge(fryerAgent, dessertAgent)
                .AddEdge(dessertAgent, platingAgent)
                .Build();

            var orderMessage = $"Process this order: {order.BurgerType ?? "none"}, {order.FriesType ?? "none"}, {order.DrinkType ?? "none"}";
            
            _logger.LogInformation("🏗️ WORKFLOW: Built workflow with 4 agents, starting execution");
            
            // Execute workflow - let it run completely
            await using var run = await InProcessExecution.StreamAsync(workflow, new ChatMessage(ChatRole.User, orderMessage));
            await run.TrySendMessageAsync(new TurnToken(emitEvents: false));

            // Wait for completion
            await foreach (var _ in run.WatchStreamAsync())
            {
                // Just consume events, don't process them
            }

            _logger.LogInformation("✅ WORKFLOW: Order processing completed");

            // Build clean order summary
            var orderItems = new List<string>();
            if (!string.IsNullOrEmpty(order.BurgerType) && order.BurgerType != "none")
                orderItems.Add(order.BurgerType);
            if (!string.IsNullOrEmpty(order.FriesType) && order.FriesType != "none")
                orderItems.Add(order.FriesType);
            if (!string.IsNullOrEmpty(order.DrinkType) && order.DrinkType != "none")
                orderItems.Add(order.DrinkType);

            var itemsText = orderItems.Count switch
            {
                0 => "order",
                1 => orderItems[0],
                2 => $"{orderItems[0]} and {orderItems[1]}",
                _ => $"{string.Join(", ", orderItems.Take(orderItems.Count - 1))}, and {orderItems.Last()}"
            };

            return $"✅ Order Complete!\n\nOrder ID: {Guid.NewGuid().ToString()[..8]}\nCustomer: {order.CustomerName}\n\nYour {itemsText} {(orderItems.Count == 1 ? "is" : "are")} ready!";
        }
        catch (Exception ex)
        {
            return $"❌ Error processing order: {ex.Message}";
        }
    }
}