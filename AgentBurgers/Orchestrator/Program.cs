using Orchestrator;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add Blazor services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Configure HTTP clients for MCP services using service discovery
builder.Services.AddHttpClient("grillagent", client =>
{
    client.BaseAddress = new Uri("http://grillagent");
});

builder.Services.AddHttpClient("fryeragent", client =>
{
    client.BaseAddress = new Uri("http://fryeragent");
});

builder.Services.AddHttpClient("dessertagent", client =>
{
    client.BaseAddress = new Uri("http://dessertagent");
});

builder.Services.AddHttpClient("platingagent", client =>
{
    client.BaseAddress = new Uri("http://platingagent");
});

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
