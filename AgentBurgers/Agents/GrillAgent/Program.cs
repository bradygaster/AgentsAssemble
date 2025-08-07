using GrillAgent;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();

// Grill Agent API endpoints
app.MapPost("/tools/cook-patty", (CookPattyRequest request) =>
{
    var result = $"🥩 Cooking {request.PattyType} patty to {request.Doneness} doneness... Done! Perfectly cooked patty ready.";
    return Results.Ok(new { result });
});

app.MapPost("/tools/melt-cheese", (MeltCheeseRequest request) =>
{
    var result = $"🧀 Melting {request.CheeseType} cheese on the patty... Perfect melt achieved!";
    return Results.Ok(new { result });
});

app.MapPost("/tools/add-bacon", (AddBaconRequest request) =>
{
    var result = $"🥓 Adding {request.BaconStrips} strips of crispy bacon... Bacon perfectly placed!";
    return Results.Ok(new { result });
});

app.MapPost("/tools/toast-bun", (ToastBunRequest request) =>
{
    var result = $"🍞 Toasting {request.BunType} bun to {request.ToastLevel}... Golden brown perfection!";
    return Results.Ok(new { result });
});

app.MapGet("/", () => "🔥 Grill Agent - Ready to handle all your grilling needs!");

app.Run();
