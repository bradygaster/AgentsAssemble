using DessertAgent;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();

// Dessert Agent API endpoints
app.MapPost("/tools/make-shake", (MakeShakeRequest request, ILogger<Program> logger) =>
{
    var result = $"🥤 Making {request.Size} {request.Flavor} shake with {request.Toppings}... Creamy shake ready!";
    logger.LogInformation(result);
    return Results.Ok(new { result });
});

app.MapPost("/tools/make-sundae", (MakeSundaeRequest request, ILogger<Program> logger) =>
{
    var result = $"🍨 Making {request.Size} sundae with {request.Flavor} ice cream and {request.Toppings}... Delicious sundae ready!";
    logger.LogInformation(result);
    return Results.Ok(new { result });
});

app.MapPost("/tools/add-whipped-cream", (AddWhippedCreamRequest request, ILogger<Program> logger) =>
{
    var result = $"🍦 Adding {request.Amount} whipped cream... Perfect fluffy topping added!";
    logger.LogInformation(result);
    return Results.Ok(new { result });
});

app.MapGet("/", () => "🍦 Dessert Agent - Ready to make your sweet treats!");

app.Run();
