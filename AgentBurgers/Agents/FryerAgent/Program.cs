var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();

// Fryer Agent API endpoints
app.MapPost("/tools/fry-standard", (FryStandardRequest request) =>
{
    var result = $"🍟 Frying {request.Portion} portion of standard fries for {request.Duration} minutes... Crispy golden fries ready!";
    return Results.Ok(new { result });
});

app.MapPost("/tools/fry-waffle", (FryWaffleRequest request) =>
{
    var result = $"🧇 Frying {request.Portion} portion of waffle fries for {request.Duration} minutes... Crispy waffle-cut fries ready!";
    return Results.Ok(new { result });
});

app.MapPost("/tools/fry-sweet-potato", (FrySweetPotatoRequest request) =>
{
    var result = $"🍠 Frying {request.Portion} portion of sweet potato fries for {request.Duration} minutes... Delicious sweet potato fries ready!";
    return Results.Ok(new { result });
});

app.MapGet("/", () => "🍟 Fryer Agent - Ready to fry all your favorites!");

app.Run();

// Request models
public record FryStandardRequest(string Portion, int Duration);
public record FryWaffleRequest(string Portion, int Duration);
public record FrySweetPotatoRequest(string Portion, int Duration);
