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

    public KitchenWorkflowService(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<string> ProcessOrderAsync(CustomerOrder order)
    {
        // Create kitchen agents using Agent Framework CreateAIAgent pattern
        var grillAgent = _chatClient.CreateAIAgent(
            name: "GrillMaster",
            instructions: "You are the grill station chef. Cook patties, melt cheese, and toast buns using your tools.",
            tools: [
                AIFunctionFactory.Create(GrillTools.CookPatty),
                AIFunctionFactory.Create(GrillTools.MeltCheese),
                AIFunctionFactory.Create(GrillTools.ToastBun)
            ]
        );

        var fryerAgent = _chatClient.CreateAIAgent(
            name: "FryerChef", 
            instructions: "You handle all frying operations. Fry fries and season them using your tools.",
            tools: [
                AIFunctionFactory.Create(FryerTools.FryFries),
                AIFunctionFactory.Create(FryerTools.SeasonFries)
            ]
        );

        var dessertAgent = _chatClient.CreateAIAgent(
            name: "DessertChef",
            instructions: "You make desserts and drinks. Create shakes and add toppings using your tools.",
            tools: [
                AIFunctionFactory.Create(DessertTools.MakeShake),
                AIFunctionFactory.Create(DessertTools.AddWhippedCream)
            ]
        );

        var platingAgent = _chatClient.CreateAIAgent(
            name: "PlatingChef",
            instructions: "You assemble and package the final meals using your tools.",
            tools: [
                AIFunctionFactory.Create(PlatingTools.AssembleBurger),
                AIFunctionFactory.Create(PlatingTools.PackageMeal)
            ]
        );

        // Build the workflow using Agent Framework WorkflowBuilder
        var workflow = new WorkflowBuilder(grillAgent)
            .AddEdge(grillAgent, fryerAgent)
            .AddEdge(fryerAgent, dessertAgent)
            .AddEdge(dessertAgent, platingAgent)
            .Build();

        // Create the order message
        var orderMessage = $"Process this order: Burger: {order.BurgerType ?? "none"}, Fries: {order.FriesType ?? "none"}, Drink: {order.DrinkType ?? "none"}";
        
        // Execute the workflow with streaming
        await using var run = await InProcessExecution.StreamAsync(workflow, new ChatMessage(ChatRole.User, orderMessage));
        
        // Send the turn token to trigger the agents
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

        var results = new List<string>();
        
        // Watch for workflow events
        await foreach (var evt in run.WatchStreamAsync())
        {
            if (evt is AgentRunUpdateEvent agentUpdate)
            {
                results.Add($"{agentUpdate.ExecutorId}: {agentUpdate.Data}");
            }
        }

        return string.Join("\n", results);
    }
}