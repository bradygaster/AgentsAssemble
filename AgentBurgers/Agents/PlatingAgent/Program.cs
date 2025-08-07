using PlatingAgent;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();

// Plating Agent API endpoints
app.MapPost("/tools/assemble-burger", (AssembleBurgerRequest request) =>
{
    var result = $"🍔 Assembling burger with {request.Components}... Perfectly assembled burger ready!";
    return Results.Ok(new { result });
});

app.MapPost("/tools/plate-meal", (PlateMealRequest request) =>
{
    var result = $"🍽️ Plating meal for {request.Service} with {request.Presentation}... Meal beautifully presented!";
    return Results.Ok(new { result });
});

app.MapPost("/tools/package-takeout", (PackageTakeoutRequest request) =>
{
    var result = $"📦 Packaging {request.Items} for takeout with {request.Accessories}... Order ready for pickup!";
    return Results.Ok(new { result });
});

app.MapGet("/", () => "🍽️ Plating Agent - Ready to perfectly present your meals!");

app.Run();
