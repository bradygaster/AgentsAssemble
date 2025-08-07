using Microsoft.Extensions.AI;
using Orchestrator;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure Foundry LLM services
builder.AddAzureChatCompletionsClient("chat")
       .AddChatClient();

// Add Blazor services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Configure MCP clients for agent services using service discovery
builder.Services.AddMcpClients();

// Register order processing services
builder.Services.AddSingleton<IOrderProcessor, McpOrderProcessor>();

// Register the Kitchen Service
builder.Services.AddSingleton<KitchenManager>();

var app = builder.Build();

app.MapPost("/test", async (IChatClient chatClient, TestRequest prompt) =>
{
    var messages = new List<ChatMessage>
    {
        new(ChatRole.System, "You are a helpful assistant."),
        new(ChatRole.User, prompt.Prompt)
    };

    var response = await chatClient.GetResponseAsync(messages);
    return Results.Ok(new { Response = response.Text });
});

app.MapDefaultEndpoints();

// API Endpoints
app.MapPost("/api/order", async (OrderRequest request, KitchenManager kitchen) =>
{
    var response = await kitchen.ProcessOrderAsync(request.Order);
    return Results.Ok(new { response });
});

app.MapGet("/api/order-stream/{orderId}", (string orderId, KitchenManager kitchen) =>
{
    var stream = kitchen.GetOrderProgressStream(orderId);
    return Results.Ok(stream);
});

// Blazor UI
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.UseStaticFiles();

app.Run();

public record TestRequest(string Prompt);