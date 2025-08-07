using Orchestrator;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

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
