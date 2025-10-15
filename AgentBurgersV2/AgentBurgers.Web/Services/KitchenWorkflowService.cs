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
            
            // Create kitchen agents
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
                instructions: "You handle all frying operations.",
                tools: [
                    AIFunctionFactory.Create(FryerTools.FryFries),
                    AIFunctionFactory.Create(FryerTools.SeasonFries)
                ]
            );

            var dessertAgent = _chatClient.CreateAIAgent(
                name: "DessertChef",
                instructions: "You make desserts and drinks.",
                tools: [
                    AIFunctionFactory.Create(DessertTools.MakeShake),
                    AIFunctionFactory.Create(DessertTools.AddWhippedCream)
                ]
            );

            var platingAgent = _chatClient.CreateAIAgent(
                name: "PlatingChef",
                instructions: "You assemble and package meals.",
                tools: [
                    AIFunctionFactory.Create(PlatingTools.AssembleBurger),
                    AIFunctionFactory.Create(PlatingTools.PackageMeal)
                ]
            );

            // Build workflow
            var workflow = new WorkflowBuilder(grillAgent)
                .AddEdge(grillAgent, fryerAgent)
                .AddEdge(fryerAgent, dessertAgent)
                .AddEdge(dessertAgent, platingAgent)
                .Build();

            var orderMessage = $"Process this order: {order.BurgerType ?? "none"}, {order.FriesType ?? "none"}, {order.DrinkType ?? "none"}";
            
            results.Add("🚀 Starting kitchen workflow...");
            
            // Execute workflow following the official pattern
            await using var run = await InProcessExecution.StreamAsync(workflow, new ChatMessage(ChatRole.User, orderMessage));
            await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

            // Follow the official sample pattern for event handling
            await foreach (var workflowEvent in run.WatchStreamAsync())
            {
                switch (workflowEvent)
                {
                    case ExecutorInvokedEvent executorInvoked:
                        results.Add($"🚀 Agent {executorInvoked.ExecutorId[..8]}... started");
                        break;
                        
                    case ExecutorCompletedEvent executorCompleted:
                        results.Add($"✅ Agent {executorCompleted.ExecutorId[..8]}... completed");
                        break;
                        
                    case AgentRunUpdateEvent streamEvent:
                        // This is where the actual agent output text comes from
                        if (!string.IsNullOrEmpty(streamEvent.Update.Text))
                        {
                            results.Add($"� {streamEvent.Update.Text.Trim()}");
                        }
                        break;
                        
                    case AgentRunResponseEvent messageEvent:
                        // Final response from agent
                        results.Add($"📝 Agent completed task");
                        break;
                        
                    case WorkflowErrorEvent workflowError:
                        results.Add($"❌ Workflow error: {workflowError.Data?.ToString() ?? "Unknown error"}");
                        break;
                }
                
                // Safety limit
                if (results.Count > 50) break;
            }

            results.Add("🎉 Order completed successfully!");
            return string.Join("\n", results);
        }
        catch (Exception ex)
        {
            return $"❌ Error: {ex.Message}";
        }
    }
}