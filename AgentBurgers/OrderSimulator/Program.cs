using OrderSimulator;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Configure HTTP client for Orchestrator using service discovery
builder.Services.AddHttpClient("orchestrator", client =>
{
  client.BaseAddress = new Uri("http://orchestrator");
});

// Register the Background Order Simulator
builder.Services.AddHostedService<OrderSimulatorService>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.Run();
