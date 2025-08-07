using FryerAgent;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();

// Fryer Agent API endpoints
app.MapPost("/tools/fry-standard", (FryStandardRequest request, ILogger<Program> logger) =>
{
    var result = $"🍟 Frying {request.Portion} portion of standard fries for {request.Duration} minutes... Crispy golden fries ready!";
    logger.LogInformation(result);
    return Results.Ok(new { result });
});

app.MapPost("/tools/fry-waffle", (FryWaffleRequest request, ILogger<Program> logger) =>
{
    var result = $"🧇 Frying {request.Portion} portion of waffle fries for {request.Duration} minutes... Crispy waffle-cut fries ready!";
    logger.LogInformation(result);
    return Results.Ok(new { result });
});

app.MapPost("/tools/fry-sweet-potato", (FrySweetPotatoRequest request, ILogger<Program> logger) =>
{
    var result = $"🍠 Frying {request.Portion} portion of sweet potato fries for {request.Duration} minutes... Delicious sweet potato fries ready!";
    logger.LogInformation(result);
    return Results.Ok(new { result });
});

app.MapGet("/", () => "🍟 Fryer Agent - Ready to fry all your favorites!");

app.Run();
