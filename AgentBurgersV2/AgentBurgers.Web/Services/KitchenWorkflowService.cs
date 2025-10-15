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
        try
        {
            var results = new List<string>();
            results.Add($"🎯 Starting order for {order.CustomerName}");
            results.Add($"📋 Order: {order.BurgerType ?? "No burger"}, {order.FriesType ?? "No fries"}, {order.DrinkType ?? "No drink"}");
            
            // Create kitchen agents using Agent Framework CreateAIAgent pattern
            var grillAgent = _chatClient.CreateAIAgent(
                name: "GrillMaster",
                instructions: "You are the grill station chef. Cook patties, melt cheese, and toast buns using your tools. Be brief and describe what you're doing.",
                tools: [
                    AIFunctionFactory.Create(GrillTools.CookPatty),
                    AIFunctionFactory.Create(GrillTools.MeltCheese),
                    AIFunctionFactory.Create(GrillTools.ToastBun)
                ]
            );

            var fryerAgent = _chatClient.CreateAIAgent(
                name: "FryerChef", 
                instructions: "You handle all frying operations. Fry fries and season them using your tools. Be brief and describe what you're doing.",
                tools: [
                    AIFunctionFactory.Create(FryerTools.FryFries),
                    AIFunctionFactory.Create(FryerTools.SeasonFries)
                ]
            );

            var dessertAgent = _chatClient.CreateAIAgent(
                name: "DessertChef",
                instructions: "You make desserts and drinks. Create shakes and add toppings using your tools. Be brief and describe what you're doing.",
                tools: [
                    AIFunctionFactory.Create(DessertTools.MakeShake),
                    AIFunctionFactory.Create(DessertTools.AddWhippedCream)
                ]
            );

            var platingAgent = _chatClient.CreateAIAgent(
                name: "PlatingChef",
                instructions: "You assemble and package the final meals using your tools. Be brief and describe what you're doing.",
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
            
            results.Add("🚀 Starting kitchen workflow...");
            
            // Execute the workflow with streaming
            await using var run = await InProcessExecution.StreamAsync(workflow, new ChatMessage(ChatRole.User, orderMessage));
            
            // Send the turn token to trigger the agents
            await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

            var eventCount = 0;
            
            // Watch for workflow events with timeout
            await foreach (var evt in run.WatchStreamAsync())
            {
                eventCount++;
                
                switch (evt)
                {
                    case AgentRunUpdateEvent agentUpdate:
                        results.Add($"� {agentUpdate.ExecutorId} is working...");
                        break;
                        
                    case ExecutorCompletedEvent executorComplete:
                        results.Add($"✅ {executorComplete.ExecutorId} completed their task");
                        break;
                        
                    default:
                        results.Add($"📡 Workflow event: {evt.GetType().Name}");
                        break;
                }
                
                // Prevent infinite loops
                if (eventCount > 20)
                {
                    results.Add("⚠️ Stopping after 20 events");
                    break;
                }
            }

            results.Add("� Order completed successfully!");
            return string.Join("\n", results);
        }
        catch (Exception ex)
        {
            return $"❌ Error processing order: {ex.Message}";
        }
    }
}