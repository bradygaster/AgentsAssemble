using AgentBurgers.Web.Components;
using AgentBurgers.Web.Services;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

builder.Services.AddSingleton<IChatClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var azureClient = new AzureOpenAIClient(
        new Uri(config["AZURE_OPENAI_ENDPOINT"] ?? "https://your-endpoint.openai.azure.com"),
        new DefaultAzureCredential());
    
    var chatClient = azureClient.GetChatClient(config["AZURE_OPENAI_MODEL"] ?? "gpt-4o-mini");
    return chatClient.AsIChatClient();
});

builder.Services.AddScoped<KitchenWorkflowService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseOutputCache();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/api/orders", async (AgentBurgers.Web.Models.CustomerOrder order, KitchenWorkflowService kitchen) =>
{
    try
    {
        var result = await kitchen.ProcessOrderAsync(order);
        return Results.Ok(new AgentBurgers.Web.Models.OrderResult 
        { 
            OrderId = order.OrderId, 
            Result = result, 
            IsComplete = true 
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new AgentBurgers.Web.Models.OrderResult 
        { 
            OrderId = order.OrderId, 
            Result = $"Error: {ex.Message}", 
            IsComplete = false 
        });
    }
});

app.MapDefaultEndpoints();
app.Run();
