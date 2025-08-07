using DessertAgent;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();

// Dessert Agent API endpoints
app.MapPost("/tools/make-shake", (MakeShakeRequest request) =>
{
    var result = $"🥤 Making {request.Size} {request.Flavor} shake with {request.Toppings}... Creamy shake ready!";
    return Results.Ok(new { result });
});

app.MapPost("/tools/make-sundae", (MakeSundaeRequest request) =>
{
    var result = $"🍨 Making {request.Size} sundae with {request.Flavor} ice cream and {request.Toppings}... Delicious sundae ready!";
    return Results.Ok(new { result });
});

app.MapPost("/tools/add-whipped-cream", (AddWhippedCreamRequest request) =>
{
    var result = $"🍦 Adding {request.Amount} whipped cream... Perfect fluffy topping added!";
    return Results.Ok(new { result });
});

app.MapGet("/", () => "🍦 Dessert Agent - Ready to make your sweet treats!");

app.Run();
