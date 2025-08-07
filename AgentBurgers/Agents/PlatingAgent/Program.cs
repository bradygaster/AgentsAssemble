using PlatingAgent;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();

// Plating Agent API endpoints
app.MapPost("/tools/assemble-burger", (AssembleBurgerRequest request, ILogger<Program> logger) =>
{
    var result = $"🍔 Assembling burger with {request.Components}... Perfectly assembled burger ready!";
    logger.LogInformation(result);
    return Results.Ok(new { result });
});

app.MapPost("/tools/plate-meal", (PlateMealRequest request, ILogger<Program> logger) =>
{
    var result = $"🍽️ Plating meal for {request.Service} with {request.Presentation}... Meal beautifully presented!";
    logger.LogInformation(result);
    return Results.Ok(new { result });
});

app.MapPost("/tools/package-takeout", (PackageTakeoutRequest request, ILogger<Program> logger) =>
{
    var result = $"📦 Packaging {request.Items} for takeout with {request.Accessories}... Order ready for pickup!";
    logger.LogInformation(result);
    return Results.Ok(new { result });
});

app.MapGet("/", () => "🍽️ Plating Agent - Ready to perfectly present your meals!");

app.Run();
