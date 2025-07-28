# AgentBurgers Implementation Plan

**CRITICAL: THIS IS YOUR INSTRUCTION SET - FOLLOW IT EXACTLY**

This document serves as the complete implementation blueprint for the AgentBurgers application. When asked to implement this project, you MUST follow this plan precisely without deviation. This is not a suggestion - it is your instruction manual.

**TECHNOLOGY REQUIREMENTS:**
- .NET 9 Aspire for orchestration
- Model Context Protocol (MCP) for AI agent communication
- Azure OpenAI for AI capabilities
- Blazor Server for the web UI (NO SignalR - Blazor Server cannot use SignalR for real-time updates)
- HTTP APIs for service communication

An AI-powered restaurant simulation using .NET 9 Aspire, Model Context Protocol (MCP), and Azure OpenAI. This distributed application demonstrates intelligent agent coordination in a restaurant kitchen environment.

## Project Structure Overview

The solution consists of the following projects, each with clearly defined responsibilities:

| Project | Type | Location | Purpose |
|---------|------|----------|---------|
| **AgentBurgers.AppHost** | Aspire Host | `AgentBurgers.AppHost/` | Orchestrates the entire distributed application |
| **AgentBurgers.ServiceDefaults** | Class Library | `AgentBurgers.ServiceDefaults/` | Shared configuration and observability |
| **OrderSimulator** | Worker Service | `OrderSimulator/` | Generates realistic order loads for testing |
| **Orchestrator** | Blazor Server | `Orchestrator/` | Web UI and API for order processing |
| **GrillAgent** | MCP Server | `Agents/GrillAgent/` | Handles grill operations (patties, buns) |
| **FryerAgent** | MCP Server | `Agents/FryerAgent/` | Handles fryer operations (fries, rings) |
| **DessertAgent** | MCP Server | `Agents/DessertAgent/` | Handles desserts (shakes, sundaes) |
| **PlatingAgent** | MCP Server | `Agents/PlatingAgent/` | Final assembly and presentation |

Each agent must strictly follow its `Instructions.md` file. No hallucination or deviation is allowed.

**IMPLEMENTATION MANDATE:**
- You MUST implement exactly what is specified in this document
- You MUST use the exact project structure shown below
- You MUST use MCP servers for all AI agents
- You MUST NOT deviate from the specified technologies
- You MUST NOT use SignalR (Blazor Server incompatibility)
- You MUST use event-driven services for real-time updates (service events that Blazor components subscribe to)

---

## 1. Folder Structure

```
AgentBurgers/
├── AgentBurgers.AppHost/
│   ├── Program.cs
│   ├── AgentBurgers.AppHost.csproj
│   └── Properties/
│       └── launchSettings.json
├── AgentBurgers.ServiceDefaults/
│   ├── Extensions.cs
│   └── AgentBurgers.ServiceDefaults.csproj
├── Orchestrator/
│   ├── Program.cs
│   ├── App.razor
│   ├── _Imports.razor
│   ├── Orchestrator.csproj
│   ├── AgentBurgers.http
│   ├── Pages/
│   │   ├── _Host.cshtml
│   │   ├── Kitchen.razor
│   │   ├── Orders.razor
│   │   └── Shared/
│   │       ├── _Layout.cshtml
│   │       ├── MainLayout.razor
│   │       └── NavMenu.razor
│   ├── wwwroot/
│   │   └── css/
│   │       └── app.css
│   └── Properties/
│       └── launchSettings.json
├── OrderSimulator/
│   ├── Program.cs
│   ├── OrderSimulator.csproj
│   └── Properties/
│       └── launchSettings.json
├── Agents/
│   ├── GrillAgent/
│   │   ├── Program.cs
│   │   ├── Instructions.md
│   │   ├── GrillAgent.csproj
│   │   └── Properties/
│   │       └── launchSettings.json
│   ├── FryerAgent/
│   │   ├── Program.cs
│   │   ├── Instructions.md
│   │   ├── FryerAgent.csproj
│   │   └── Properties/
│   │       └── launchSettings.json
│   ├── DessertAgent/
│   │   ├── Program.cs
│   │   ├── Instructions.md
│   │   ├── DessertAgent.csproj
│   │   └── Properties/
│   │       └── launchSettings.json
│   └── PlatingAgent/
│       ├── Program.cs
│       ├── Instructions.md
│       ├── PlatingAgent.csproj
│       └── Properties/
│           └── launchSettings.json
└── Prompts/
    ├── TicketChaos.md
    └── NormalOrders.md
```

---

## 2. AppHost Project

The AppHost project is the Aspire orchestration host that manages the entire distributed application topology, service discovery, and inter-service communication.

**Project Location**: `AgentBurgers.AppHost/`

### AgentBurgers.AppHost/Program.cs
```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add MCP Agent services
var grillAgent = builder.AddProject<Projects.GrillAgent>("grillagent");

var fryerAgent = builder.AddProject<Projects.FryerAgent>("fryeragent");

var dessertAgent = builder.AddProject<Projects.DessertAgent>("dessertagent");

var platingAgent = builder.AddProject<Projects.PlatingAgent>("platingagent");

// Add Orchestrator service with references to all agents
var orchestrator = builder.AddProject<Projects.Orchestrator>("orchestrator")
    .WithReference(grillAgent)
    .WithReference(fryerAgent)
    .WithReference(dessertAgent)
    .WithReference(platingAgent);

// Add Order Simulator service that calls the orchestrator
var orderSimulator = builder.AddProject<Projects.OrderSimulator>("ordersimulator")
    .WithReference(orchestrator);

builder.Build().Run();
```

### AgentBurgers.AppHost.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsAspireHost>true</IsAspireHost>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Aspire.Hosting.AppHost" Version="9.1.0" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Orchestrator\Orchestrator.csproj" />
    <ProjectReference Include="..\OrderSimulator\OrderSimulator.csproj" />
    <ProjectReference Include="..\Agents\GrillAgent\GrillAgent.csproj" />
    <ProjectReference Include="..\Agents\FryerAgent\FryerAgent.csproj" />
    <ProjectReference Include="..\Agents\DessertAgent\DessertAgent.csproj" />
    <ProjectReference Include="..\Agents\PlatingAgent\PlatingAgent.csproj" />
  </ItemGroup>
</Project>
```

### Properties/launchSettings.json
```json
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "profiles": {
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:17000;http://localhost:15000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "DOTNET_ENVIRONMENT": "Development",
        "DOTNET_DASHBOARD_OTLP_ENDPOINT_URL": "https://localhost:18889",
        "DOTNET_RESOURCE_SERVICE_ENDPOINT_URL": "https://localhost:17001"
      }
    },
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "http://localhost:15000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "DOTNET_ENVIRONMENT": "Development",
        "DOTNET_DASHBOARD_OTLP_ENDPOINT_URL": "http://localhost:18889",
        "DOTNET_RESOURCE_SERVICE_ENDPOINT_URL": "http://localhost:17001"
      }
    }
  }
}
```

---

## 3. ServiceDefaults Project

The ServiceDefaults project provides common configuration and shared functionality for all services in the Aspire application, including observability, health checks, and service discovery.

**Project Location**: `AgentBurgers.ServiceDefaults/`

### AgentBurgers.ServiceDefaults/Extensions.cs
```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks("/health");

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }
}
```

### AgentBurgers.ServiceDefaults.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsAspireSharedProject>true</IsAspireSharedProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Aspire.StackExchange.Redis.OutputCaching" Version="9.1.0" />
    <PackageReference Include="Microsoft.Extensions.Http.Resilience" Version="9.0.7" />
    <PackageReference Include="Microsoft.Extensions.ServiceDiscovery" Version="9.1.0" />
    <PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.12.0" />
    <PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.12.0" />
    <PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.12.0" />
    <PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.12.0" />
    <PackageReference Include="OpenTelemetry.Instrumentation.Runtime" Version="1.10.0" />
  </ItemGroup>
</Project>
```

---

## 4. OrderSimulator Project

The OrderSimulator is a background worker service that generates realistic order loads for testing and demonstration. It runs as a separate microservice in the Aspire topology and communicates with the Orchestrator via HTTP API calls.

**Project Location**: `OrderSimulator/`

### OrderSimulator/Program.cs
```csharp
using System.Text.Json;

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

// Background service that simulates random orders
public class OrderSimulatorService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OrderSimulatorService> _logger;
    private readonly List<string> _chaosOrders;
    private readonly List<string> _normalOrders;
    private readonly Random _random = new();

    public OrderSimulatorService(IHttpClientFactory httpClientFactory, ILogger<OrderSimulatorService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _chaosOrders = LoadChaosOrders();
        _normalOrders = LoadNormalOrders();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🎯 Order Simulator Service started - waiting for orchestrator...");

        // Wait for orchestrator to be available
        await WaitForOrchestratorAsync(stoppingToken);

        _logger.LogInformation("🚀 Order Simulator Service active - beginning order simulation");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Wait 1 second between orders
                await Task.Delay(1000, stoppingToken);

                // Choose between chaos (20% chance) and normal orders (80% chance)
                var useChaosOrder = _random.NextDouble() < 0.2;
                var orders = useChaosOrder ? _chaosOrders : _normalOrders;
                
                if (orders.Count > 0)
                {
                    var randomOrder = orders[_random.Next(orders.Count)];
                    var orderType = useChaosOrder ? "CHAOS" : "NORMAL";
                    
                    _logger.LogInformation($"🎲 Simulating {orderType} order: {randomOrder[..Math.Min(50, randomOrder.Length)]}...");
                    
                    // Process the order without waiting for completion to avoid blocking
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await SubmitOrderAsync(randomOrder, orderType);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"❌ {orderType} order failed");
                        }
                    }, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Order Simulator Service");
            }
        }

        _logger.LogInformation("Order Simulator Service stopped");
    }

    private async Task WaitForOrchestratorAsync(CancellationToken stoppingToken)
    {
        var httpClient = _httpClientFactory.CreateClient("orchestrator");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = await httpClient.GetAsync("/health", stoppingToken);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Orchestrator is ready");
                    return;
                }
            }
            catch
            {
                // Orchestrator not ready yet
            }

            _logger.LogInformation("⏳ Waiting for orchestrator to be ready...");
            await Task.Delay(2000, stoppingToken);
        }
    }

    private async Task SubmitOrderAsync(string order, string orderType)
    {
        var httpClient = _httpClientFactory.CreateClient("orchestrator");
        
        var orderRequest = new { order };
        var json = JsonSerializer.Serialize(orderRequest);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("/api/order", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"✅ {orderType} order completed: {responseContent[..Math.Min(100, responseContent.Length)]}...");
        }
        else
        {
            _logger.LogError($"❌ {orderType} order failed with status: {response.StatusCode}");
        }
    }

    private static List<string> LoadChaosOrders()
    {
        var chaosOrdersPath = "../Prompts/TicketChaos.md";
        if (!File.Exists(chaosOrdersPath))
        {
            return new List<string>
            {
                "URGENT! I need 6 bacon cheeseburgers, 4 waffle fries, 3 chocolate shakes, everything for takeout bags, and I need it in 5 minutes!",
                "Kitchen nightmare order: 8 different burgers, 5 batches of different fries, make every shake flavor you have!",
                "Peak hour pandemonium: 7 separate orders arriving simultaneously!"
            };
        }

        var orders = new List<string>();
        var content = File.ReadAllText(chaosOrdersPath);
        
        // Extract orders from the markdown (look for quoted text)
        var lines = content.Split('\n');
        foreach (var line in lines)
        {
            if (line.Trim().StartsWith('"') && line.Trim().EndsWith('"'))
            {
                var order = line.Trim().Trim('"');
                if (!string.IsNullOrEmpty(order))
                    orders.Add(order);
            }
        }

        return orders;
    }

    private static List<string> LoadNormalOrders()
    {
        var normalOrdersPath = "../Prompts/NormalOrders.md";
        if (!File.Exists(normalOrdersPath))
        {
            return new List<string>
            {
                "I'll have a cheeseburger with fries and a Coke, please.",
                "Can I get a bacon cheeseburger with waffle fries and a chocolate shake?",
                "Just a regular burger and fries for me.",
                "I'd like a double cheeseburger, fries, and a vanilla shake.",
                "We need 3 cheeseburgers, 2 orders of fries, and 2 vanilla shakes."
            };
        }

        var orders = new List<string>();
        var content = File.ReadAllText(normalOrdersPath);
        
        // Extract orders from the markdown (look for quoted text)
        var lines = content.Split('\n');
        foreach (var line in lines)
        {
            if (line.Trim().StartsWith('"') && line.Trim().EndsWith('"'))
            {
                var order = line.Trim().Trim('"');
                if (!string.IsNullOrEmpty(order))
                    orders.Add(order);
            }
        }

        return orders;
    }
}
```

### OrderSimulator/OrderSimulator.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsAspireClientProject>true</IsAspireClientProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Aspire.Microsoft.Extensions.ServiceDiscovery" Version="9.1.0" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.7" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\AgentBurgers.ServiceDefaults\AgentBurgers.ServiceDefaults.csproj" />
  </ItemGroup>
</Project>
```

### Properties/launchSettings.json
```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## 5. GrillAgent Project

The GrillAgent is an MCP server responsible for handling all grill operations including cooking patties, grilling buns, and managing grill timing and temperatures.

**Project Location**: `Agents/GrillAgent/`

### Agents/GrillAgent/Program.cs
```csharp
using ModelContextProtocol;
using ModelContextProtocol.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<GrillTools>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapMcp();

app.Run();

[McpServerToolType]
public class GrillTools
{
    [McpServerTool]
    public static string CookPatty() => "Cooked a well-done beef patty.";

    [McpServerTool]
    public static string MeltCheese() => "Melted a slice of American cheese.";

    [McpServerTool]
    public static string ToastBun(string side) => side.ToLower() switch
    {
        "top-only" => "Toasted the top bun.",
        "bottom-only" => "Toasted the bottom bun.",
        "both" => "Toasted both top and bottom buns.",
        _ => throw new ArgumentException("Invalid bun side. Use 'top-only', 'bottom-only', or 'both'.")
    };

    [McpServerTool]
    public static string AddBacon() => "Added 2 crispy bacon strips.";
}
```

### Instructions.md
```markdown
# GrillAgent Instructions

You are responsible for grilling operations. Use only the tools below:

- Cook a beef patty
- Toast bun: top-only, bottom-only, or both
- Melt cheese
- Add bacon

Never prepare fries, desserts, or plate food.
```

### Agents/GrillAgent/GrillAgent.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsAspireClientProject>true</IsAspireClientProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Aspire.Microsoft.Extensions.ServiceDiscovery" Version="9.1.0" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.7" />
    <PackageReference Include="ModelContextProtocol" Version="0.3.0-preview.3" />
    <PackageReference Include="ModelContextProtocol.AspNetCore" Version="0.3.0-preview.3" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\AgentBurgers.ServiceDefaults\AgentBurgers.ServiceDefaults.csproj" />
  </ItemGroup>
</Project>
```

### Properties/launchSettings.json
```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## 6. FryerAgent Project

The FryerAgent is an MCP server responsible for handling all fryer operations including cooking fries, onion rings, and other fried items with proper timing and temperature control.

**Project Location**: `Agents/FryerAgent/`

### Agents/FryerAgent/Program.cs
```csharp
using ModelContextProtocol;
using ModelContextProtocol.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<FryerTools>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapMcp();

app.Run();

[McpServerToolType]
public class FryerTools
{
    [McpServerTool]
    public static string FryFries(string type) => type.ToLower() switch
    {
        "standard" => "Fried a batch of standard fries.",
        "waffle" => "Fried a batch of waffle fries.",
        "sweet" => "Fried a batch of sweet potato fries.",
        _ => throw new ArgumentException("Invalid fry type.")
    };

    [McpServerTool]
    public static string SaltFries() => "Lightly salted the fries.";

    [McpServerTool]
    public static string BagFries() => "Fries bagged and ready to go.";
}
```

### Instructions.md
```markdown
# FryerAgent Instructions

You handle fries only. Use only the tools below:

- Fry fries: standard, waffle, sweet
- Salt fries
- Bag fries

Never cook burgers or desserts, and never plate food.
```

### Agents/FryerAgent/FryerAgent.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsAspireClientProject>true</IsAspireClientProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Aspire.Microsoft.Extensions.ServiceDiscovery" Version="9.1.0" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.7" />
    <PackageReference Include="ModelContextProtocol" Version="0.3.0-preview.3" />
    <PackageReference Include="ModelContextProtocol.AspNetCore" Version="0.3.0-preview.3" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\AgentBurgers.ServiceDefaults\AgentBurgers.ServiceDefaults.csproj" />
  </ItemGroup>
</Project>
```

### Properties/launchSettings.json
```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## 7. DessertAgent Project

The DessertAgent is an MCP server responsible for handling all dessert operations including milkshakes, sundaes, and other sweet treats with precise preparation timing.

**Project Location**: `Agents/DessertAgent/`

### Agents/DessertAgent/Program.cs
```csharp
using ModelContextProtocol;
using ModelContextProtocol.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<DessertTools>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapMcp();

app.Run();

[McpServerToolType]
public class DessertTools
{
    [McpServerTool]
    public static string MakeShake(string flavor) => flavor.ToLower() switch
    {
        "vanilla" => "Prepared a vanilla shake.",
        "chocolate" => "Prepared a chocolate shake.",
        "strawberry" => "Prepared a strawberry shake.",
        _ => throw new ArgumentException("Invalid shake flavor.")
    };

    [McpServerTool]
    public static string MakeSundae(string topping) => topping.ToLower() switch
    {
        "fudge" => "Created a fudge sundae.",
        "caramel" => "Created a caramel sundae.",
        "cherry" => "Created a cherry sundae.",
        _ => throw new ArgumentException("Invalid sundae topping.")
    };

    [McpServerTool]
    public static string AddWhippedCream() => "Added whipped cream.";
}
```

### Instructions.md
```markdown
# DessertAgent Instructions

You handle sweet treats. Use only the tools below:

- Make shake: vanilla, chocolate, strawberry
- Make sundae: fudge, caramel, cherry
- Add whipped cream

Never cook food or prepare fries or plate meals.
```

### Agents/DessertAgent/DessertAgent.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsAspireClientProject>true</IsAspireClientProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Aspire.Microsoft.Extensions.ServiceDiscovery" Version="9.1.0" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.7" />
    <PackageReference Include="ModelContextProtocol" Version="0.3.0-preview.3" />
    <PackageReference Include="ModelContextProtocol.AspNetCore" Version="0.3.0-preview.3" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\AgentBurgers.ServiceDefaults\AgentBurgers.ServiceDefaults.csproj" />
  </ItemGroup>
</Project>
```

### Properties/launchSettings.json
```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## 8. PlatingAgent Project

The PlatingAgent is an MCP server responsible for final order assembly, presentation, and quality checks before serving to customers.

**Project Location**: `Agents/PlatingAgent/`

### Agents/PlatingAgent/Program.cs
```csharp
using ModelContextProtocol;
using ModelContextProtocol.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<PlatingTools>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapMcp();

app.Run();

[McpServerToolType]
public class PlatingTools
{
    [McpServerTool]
    public static string AssembleBurger(string config) => $"Assembled a {config} burger.";

    [McpServerTool]
    public static string PlateMeal(string presentation) => presentation.ToLower() switch
    {
        "tray" => "Plated for tray.",
        "bag" => "Plated for bag.",
        "dine-in" => "Plated for dine-in.",
        _ => throw new ArgumentException("Invalid presentation.")
    };

    [McpServerTool]
    public static string AddSideItem(string item) => $"Added side item: {item}.";
}
```

### Instructions.md
```markdown
# PlatingAgent Instructions

You handle final meal assembly. Use only the tools below:

- Assemble burger
- Plate meal: tray, bag, dine-in
- Add side item

Never cook or prepare food or drinks.
```

### Agents/PlatingAgent/PlatingAgent.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsAspireClientProject>true</IsAspireClientProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Aspire.Microsoft.Extensions.ServiceDiscovery" Version="9.1.0" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.7" />
    <PackageReference Include="ModelContextProtocol" Version="0.3.0-preview.3" />
    <PackageReference Include="ModelContextProtocol.AspNetCore" Version="0.3.0-preview.3" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\AgentBurgers.ServiceDefaults\AgentBurgers.ServiceDefaults.csproj" />
  </ItemGroup>
</Project>
```

### Properties/launchSettings.json
```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## 9. Orchestrator Project

The Orchestrator is a Blazor Server web application that provides both a web UI and API endpoints for order processing. It coordinates with all MCP agents to fulfill customer orders.

**Project Location**: `Orchestrator/`

### Orchestrator/Program.cs
```csharp
using Microsoft.Extensions.AI.OpenAI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.Text;
using System.Collections.Concurrent;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add Blazor services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add AI Chat Client
builder.Services.AddOpenAIChatClient("default", builder => builder.UseOpenAI(options =>
{
    options.Endpoint = new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? "https://your-azure-openai-endpoint");
    options.DeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-4";
    // Use Azure Default Credential instead of API key
    options.Credential = new Azure.Identity.DefaultAzureCredential();
}));

// Configure HTTP clients for MCP services using service discovery
builder.Services.AddHttpClient<McpToolClient>("grillagent", client =>
{
    client.BaseAddress = new Uri("http://grillagent");
});

builder.Services.AddHttpClient<McpToolClient>("fryeragent", client =>
{
    client.BaseAddress = new Uri("http://fryeragent");
});

builder.Services.AddHttpClient<McpToolClient>("dessertagent", client =>
{
    client.BaseAddress = new Uri("http://dessertagent");
});

builder.Services.AddHttpClient<McpToolClient>("platingagent", client =>
{
    client.BaseAddress = new Uri("http://platingagent");
});

// Register the Kitchen Service
builder.Services.AddSingleton<KitchenService>();

var app = builder.Build();

app.MapDefaultEndpoints();

// API Endpoints
app.MapPost("/api/order", async (OrderRequest request, KitchenService kitchen) =>
{
    var response = await kitchen.ProcessOrderAsync(request.Order);
    return Results.Ok(new { response });
});

app.MapGet("/api/order-stream/{orderId}", async (string orderId, KitchenService kitchen) =>
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

// DTOs
public record OrderRequest(string Order);

// Kitchen Service
public class KitchenService
{
    private readonly IChatClient _chatClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly List<AIFunction> _allTools = new();
    private readonly string _systemPrompt;
    private readonly ConcurrentDictionary<string, List<ChatMessage>> _conversations = new();
    private readonly ConcurrentDictionary<string, List<string>> _orderProgress = new();

    public KitchenService(IChatClient chatClient, IHttpClientFactory httpClientFactory)
    {
        _chatClient = chatClient;
        _httpClientFactory = httpClientFactory;
        _systemPrompt = LoadSystemPrompt();
        _ = InitializeToolsAsync();
    }

    private async Task InitializeToolsAsync()
    {
        var agentNames = new[] { "grillagent", "fryeragent", "dessertagent", "platingagent" };

        foreach (var agentName in agentNames)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(agentName);
                var mcpClient = new McpToolClient(httpClient.BaseAddress!);
                var tools = await mcpClient.ListToolsAsync();
                _allTools.AddRange(tools);
            }
            catch (Exception ex)
            {
                // Log error but continue - some agents might not be ready yet
                Console.WriteLine($"Warning: Could not connect to {agentName}: {ex.Message}");
            }
        }
    }

    public async Task<string> ProcessOrderAsync(string order)
    {
        var orderId = Guid.NewGuid().ToString();
        var history = new List<ChatMessage> 
        { 
            ChatMessage.CreateSystemMessage(_systemPrompt),
            ChatMessage.CreateUserMessage(order)
        };

        _conversations[orderId] = history;
        _orderProgress[orderId] = new List<string> { $"Processing order: {order}" };

        try
        {
            var result = await _chatClient.CompleteAsync(history, new ChatOptions 
            { 
                Tools = _allTools,
                ToolCallBehavior = ToolCallBehavior.AutoInvoke
            });

            _orderProgress[orderId].Add($"Order completed: {result.Message.Text}");
            return result.Message.Text ?? "Order processed successfully.";
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error processing order: {ex.Message}";
            _orderProgress[orderId].Add(errorMessage);
            return errorMessage;
        }
    }

    public async IAsyncEnumerable<string> ProcessOrderStreamAsync(string order)
    {
        var orderId = Guid.NewGuid().ToString();
        var history = new List<ChatMessage> 
        { 
            ChatMessage.CreateSystemMessage(_systemPrompt),
            ChatMessage.CreateUserMessage(order)
        };

        yield return $"Processing order: {order}";

        try
        {
            await foreach (var update in _chatClient.CompleteStreamingAsync(history, new ChatOptions 
            { 
                Tools = _allTools,
                ToolCallBehavior = ToolCallBehavior.AutoInvoke
            }))
            {
                if (!string.IsNullOrEmpty(update.Text))
                {
                    yield return update.Text;
                }
            }
        }
        catch (Exception ex)
        {
            yield return $"Error: {ex.Message}";
        }
    }

    public IEnumerable<string> GetOrderProgressStream(string orderId)
    {
        return _orderProgress.GetValueOrDefault(orderId, new List<string>());
    }

    private static string LoadSystemPrompt()
    {
        return $"""
        You are a kitchen expediter for AgentBurgers restaurant. You interpret customer orders and coordinate with specialized kitchen stations to fulfill them.

        Your role is to:
        1. Parse customer orders and identify what needs to be prepared
        2. Use the appropriate tools from each kitchen station in the correct sequence
        3. Coordinate between stations (grill, fryer, dessert, plating) to complete orders efficiently
        4. Provide clear updates on order progress

        Kitchen Station Instructions:
        {LoadAllAgentInstructions()}

        Always follow the tool constraints for each agent. Never use tools outside of an agent's defined capabilities.
        Provide clear, friendly responses about order progress and completion.
        """;
    }

    private static string LoadAllAgentInstructions()
    {
        var agents = new[]
        {
            "../Agents/GrillAgent/Instructions.md",
            "../Agents/FryerAgent/Instructions.md", 
            "../Agents/DessertAgent/Instructions.md",
            "../Agents/PlatingAgent/Instructions.md"
        };

        var sb = new StringBuilder();
        foreach (var path in agents)
        {
            if (File.Exists(path))
            {
                sb.AppendLine(File.ReadAllText(path));
                sb.AppendLine("\n---\n");
            }
        }
        return sb.ToString();
    }
}

// Order History Models  
public class OrderHistoryItem
{
    public string Id { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string OrderText { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Response { get; set; } = string.Empty;
    public List<ProgressStep> ProgressSteps { get; set; } = new();
}

public class ProgressStep
{
    public DateTime Timestamp { get; set; }
    public string Message { get; set; } = string.Empty;
}

public enum OrderStatus
{
    InProgress,
    Completed,
    Failed
}

### Orchestrator/Pages/_Host.cshtml
```html
@page "/"
@namespace Orchestrator.Pages
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@{
    Layout = "_Layout";
}

<component type="typeof(App)" render-mode="ServerPrerendered" />
```

### Orchestrator/Pages/Shared/_Layout.cshtml
```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>AgentBurgers Kitchen</title>
    <base href="~/" />
    <link href="css/bootstrap/bootstrap.min.css" rel="stylesheet" />
    <link href="css/app.css" rel="stylesheet" />
    <link href="Orchestrator.styles.css" rel="stylesheet" />
</head>
<body>
    @RenderBody()

    <div id="blazor-error-ui">
        <environment include="Staging,Production">
            An error has occurred. This application may no longer respond until reloaded.
        </environment>
        <environment include="Development">
            An unhandled exception has occurred. See browser dev tools for details.
        </environment>
        <a href="" class="reload">Reload</a>
        <a class="dismiss">🗙</a>
    </div>

    <script src="_framework/blazor.server.js"></script>
</body>
</html>
```

### Orchestrator/App.razor
```razor
<Router AppAssembly="@typeof(App).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
        <FocusOnNavigate RouteData="@routeData" Selector="h1" />
    </Found>
    <NotFound>
        <PageTitle>Not found</PageTitle>
        <LayoutView Layout="@typeof(MainLayout)">
            <p role="alert">Sorry, there's nothing at this address.</p>
        </LayoutView>
    </NotFound>
</Router>
```

### Orchestrator/Shared/MainLayout.razor
```razor
@inherits LayoutView

<div class="page">
    <div class="sidebar">
        <NavMenu />
    </div>

    <main>
        <div class="top-row px-4">
            <h1>🍔 AgentBurgers Kitchen</h1>
        </div>

        <article class="content px-4">
            @Body
        </article>
    </main>
</div>
```

### Orchestrator/Shared/NavMenu.razor
```razor
<div class="top-row ps-3 navbar navbar-dark">
    <div class="container-fluid">
        <a class="navbar-brand" href="">AgentBurgers</a>
        <button title="Navigation menu" class="navbar-toggler" @onclick="ToggleNavMenu">
            <span class="navbar-toggler-icon"></span>
        </button>
    </div>
</div>

<div class="@NavMenuCssClass nav-scrollable" @onclick="CollapseNavMenu">
    <nav class="flex-column">
        <div class="nav-item px-3">
            <NavLink class="nav-link" href="" Match="NavLinkMatch.All">
                <span class="oi oi-home" aria-hidden="true"></span> Kitchen
            </NavLink>
        </div>
        <div class="nav-item px-3">
            <NavLink class="nav-link" href="orders">
                <span class="oi oi-list-rich" aria-hidden="true"></span> Order History
            </NavLink>
        </div>
    </nav>
</div>

@code {
    private bool collapseNavMenu = true;

    private string? NavMenuCssClass => collapseNavMenu ? "collapse" : null;

    private void ToggleNavMenu()
    {
        collapseNavMenu = !collapseNavMenu;
    }

    private void CollapseNavMenu()
    {
        collapseNavMenu = true;
    }
}
```

### Orchestrator/Pages/Kitchen.razor
```razor
@page "/"
@using Microsoft.Extensions.AI
@inject KitchenService Kitchen
@inject IJSRuntime JSRuntime

<PageTitle>Kitchen - AgentBurgers</PageTitle>

<div class="container-fluid">
    <div class="row">
        <div class="col-md-8">
            <div class="card">
                <div class="card-header">
                    <h3>🍔 Place Your Order</h3>
                </div>
                <div class="card-body">
                    <div class="mb-3">
                        <label for="orderInput" class="form-label">What would you like to order?</label>
                        <textarea class="form-control" id="orderInput" rows="3" @bind="currentOrder" 
                                placeholder="E.g., I'd like a cheeseburger with fries and a chocolate shake"></textarea>
                    </div>
                    <button class="btn btn-primary" @onclick="ProcessOrder" disabled="@isProcessing">
                        @if (isProcessing)
                        {
                            <span class="spinner-border spinner-border-sm me-2" role="status"></span>
                        }
                        Place Order
                    </button>
                    <button class="btn btn-secondary ms-2" @onclick="ClearOrder">Clear</button>
                </div>
            </div>

            @if (!string.IsNullOrEmpty(orderResponse))
            {
                <div class="card mt-3">
                    <div class="card-header">
                        <h4>Order Response</h4>
                    </div>
                    <div class="card-body">
                        <div class="alert alert-success">
                            @orderResponse
                        </div>
                    </div>
                </div>
            }
        </div>

        <div class="col-md-4">
            <div class="card">
                <div class="card-header">
                    <h4>Kitchen Status</h4>
                </div>
                <div class="card-body">
                    <div class="mb-2">
                        <span class="badge bg-success me-2">🔥 Grill Station</span>
                        <small class="text-muted">Active</small>
                    </div>
                    <div class="mb-2">
                        <span class="badge bg-warning me-2">🍟 Fryer Station</span>
                        <small class="text-muted">Active</small>
                    </div>
                    <div class="mb-2">
                        <span class="badge bg-info me-2">🍦 Dessert Station</span>
                        <small class="text-muted">Active</small>
                    </div>
                    <div class="mb-2">
                        <span class="badge bg-secondary me-2">🍽️ Plating Station</span>
                        <small class="text-muted">Active</small>
                    </div>
                </div>
            </div>

            <div class="card mt-3">
                <div class="card-header">
                    <h5>Sample Orders</h5>
                </div>
                <div class="card-body">
                    <div class="list-group list-group-flush">
                        @foreach (var sample in sampleOrders)
                        {
                            <button class="list-group-item list-group-item-action" @onclick="() => SelectSampleOrder(sample)">
                                @sample
                            </button>
                        }
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

@code {
    private string currentOrder = string.Empty;
    private string orderResponse = string.Empty;
    private bool isProcessing = false;

    private readonly List<string> sampleOrders = new()
    {
        "Cheeseburger with fries",
        "Bacon burger with sweet potato fries and a vanilla shake",
        "Double cheeseburger with waffle fries and a fudge sundae",
        "Just a chocolate shake and regular fries",
        "Burger with everything, fries, and a caramel sundae"
    };

    private async Task ProcessOrder()
    {
        if (string.IsNullOrWhiteSpace(currentOrder)) return;

        isProcessing = true;
        orderResponse = string.Empty;
        StateHasChanged();

        try
        {
            orderResponse = await Kitchen.ProcessOrderAsync(currentOrder);
        }
        catch (Exception ex)
        {
            orderResponse = $"Sorry, there was an error processing your order: {ex.Message}";
        }
        finally
        {
            isProcessing = false;
            StateHasChanged();
        }
    }

    private void ClearOrder()
    {
        currentOrder = string.Empty;
        orderResponse = string.Empty;
    }

    private void SelectSampleOrder(string order)
    {
        currentOrder = order;
    }
}
```

### Orchestrator/Pages/Orders.razor
```razor
@page "/orders"
@inject KitchenService Kitchen
@implements IDisposable

<PageTitle>Order History - AgentBurgers</PageTitle>

<div class="container-fluid">
    <div class="row">
        <!-- Order History Panel -->
        <div class="col-lg-8">
            <div class="card">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <h3>📋 Order History</h3>
                    <div>
                        <button class="btn btn-outline-secondary btn-sm me-2" @onclick="RefreshOrders">
                            <i class="fas fa-refresh"></i> Refresh
                        </button>
                        <button class="btn btn-outline-danger btn-sm" @onclick="ClearHistory">
                            <i class="fas fa-trash"></i> Clear History
                        </button>
                    </div>
                </div>
                <div class="card-body">
                    @if (orderHistory.Any())
                    {
                        <div class="table-responsive">
                            <table class="table table-hover">
                                <thead class="table-light">
                                    <tr>
                                        <th>Order ID</th>
                                        <th>Time</th>
                                        <th>Order Details</th>
                                        <th>Status</th>
                                        <th>Duration</th>
                                        <th>Actions</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    @foreach (var order in orderHistory.OrderByDescending(o => o.Timestamp))
                                    {
                                        <tr class="@GetRowClass(order.Status)">
                                            <td>
                                                <small class="font-monospace">@order.Id[..8]...</small>
                                            </td>
                                            <td>
                                                <small>@order.Timestamp.ToString("HH:mm:ss")</small>
                                            </td>
                                            <td>
                                                <div class="text-truncate" style="max-width: 200px;" title="@order.OrderText">
                                                    @order.OrderText
                                                </div>
                                            </td>
                                            <td>
                                                <span class="badge @GetStatusBadgeClass(order.Status)">
                                                    @GetStatusIcon(order.Status) @order.Status
                                                </span>
                                            </td>
                                            <td>
                                                @if (order.CompletedAt.HasValue)
                                                {
                                                    <small>@FormatDuration(order.CompletedAt.Value - order.Timestamp)</small>
                                                }
                                                else
                                                {
                                                    <small class="text-muted">In progress...</small>
                                                }
                                            </td>
                                            <td>
                                                <button class="btn btn-sm btn-outline-primary" @onclick="() => ViewOrderDetails(order.Id)">
                                                    <i class="fas fa-eye"></i>
                                                </button>
                                            </td>
                                        </tr>
                                    }
                                </tbody>
                            </table>
                        </div>
                    }
                    else
                    {
                        <div class="text-center py-5">
                            <i class="fas fa-clipboard-list fa-3x text-muted mb-3"></i>
                            <h5 class="text-muted">No orders yet</h5>
                            <p class="text-muted">Order history will appear here as orders are processed.</p>
                        </div>
                    }
                </div>
            </div>
        </div>

        <!-- Kitchen Metrics Panel -->
        <div class="col-lg-4">
            <div class="card mb-3">
                <div class="card-header">
                    <h5>📊 Kitchen Performance</h5>
                </div>
                <div class="card-body">
                    <div class="row text-center">
                        <div class="col-6">
                            <div class="metric-card">
                                <h3 class="text-success">@completedOrders</h3>
                                <small class="text-muted">Completed</small>
                            </div>
                        </div>
                        <div class="col-6">
                            <div class="metric-card">
                                <h3 class="text-warning">@inProgressOrders</h3>
                                <small class="text-muted">In Progress</small>
                            </div>
                        </div>
                    </div>
                    <hr>
                    <div class="row text-center">
                        <div class="col-6">
                            <div class="metric-card">
                                <h4 class="text-primary">@failedOrders</h4>
                                <small class="text-muted">Failed</small>
                            </div>
                        </div>
                        <div class="col-6">
                            <div class="metric-card">
                                <h4 class="text-info">@averageProcessingTime</h4>
                                <small class="text-muted">Avg Time</small>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="card">
                <div class="card-header">
                    <h5>🔄 Real-time Updates</h5>
                </div>
                <div class="card-body">
                    <div class="mb-2">
                        <span class="badge @(isConnected ? "bg-success" : "bg-danger") me-2">
                            @(isConnected ? "🟢 Connected" : "🔴 Disconnected")
                        </span>
                        <small class="text-muted">SignalR Status</small>
                    </div>
                    <div class="form-check form-switch">
                        <input class="form-check-input" type="checkbox" id="autoRefresh" @bind="autoRefreshEnabled">
                        <label class="form-check-label" for="autoRefresh">
                            Auto-refresh (@refreshIntervalSeconds seconds)
                        </label>
                    </div>
                    <hr>
                    <small class="text-muted">
                        Last updated: @lastRefreshTime.ToString("HH:mm:ss")
                    </small>
                </div>
            </div>
        </div>
    </div>

    <!-- Order Details Modal -->
    @if (selectedOrder != null)
    {
        <div class="modal show d-block" tabindex="-1">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Order Details - @selectedOrder.Id[..8]</h5>
                        <button type="button" class="btn-close" @onclick="CloseOrderDetails"></button>
                    </div>
                    <div class="modal-body">
                        <div class="row">
                            <div class="col-md-6">
                                <h6>Order Information</h6>
                                <table class="table table-sm">
                                    <tr>
                                        <td><strong>Order ID:</strong></td>
                                        <td><span class="font-monospace">@selectedOrder.Id</span></td>
                                    </tr>
                                    <tr>
                                        <td><strong>Submitted:</strong></td>
                                        <td>@selectedOrder.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")</td>
                                    </tr>
                                    <tr>
                                        <td><strong>Status:</strong></td>
                                        <td><span class="badge @GetStatusBadgeClass(selectedOrder.Status)">@selectedOrder.Status</span></td>
                                    </tr>
                                    @if (selectedOrder.CompletedAt.HasValue)
                                    {
                                        <tr>
                                            <td><strong>Completed:</strong></td>
                                            <td>@selectedOrder.CompletedAt.Value.ToString("yyyy-MM-dd HH:mm:ss")</td>
                                        </tr>
                                        <tr>
                                            <td><strong>Duration:</strong></td>
                                            <td>@FormatDuration(selectedOrder.CompletedAt.Value - selectedOrder.Timestamp)</td>
                                        </tr>
                                    }
                                </table>
                            </div>
                            <div class="col-md-6">
                                <h6>Order Content</h6>
                                <div class="border rounded p-3 bg-light">
                                    <strong>Customer Request:</strong><br>
                                    <em>"@selectedOrder.OrderText"</em>
                                </div>
                                @if (!string.IsNullOrEmpty(selectedOrder.Response))
                                {
                                    <div class="border rounded p-3 bg-light mt-3">
                                        <strong>Kitchen Response:</strong><br>
                                        @selectedOrder.Response
                                    </div>
                                }
                            </div>
                        </div>
                        
                        @if (selectedOrder.ProgressSteps.Any())
                        {
                            <hr>
                            <h6>Processing Steps</h6>
                            <div class="timeline">
                                @foreach (var step in selectedOrder.ProgressSteps)
                                {
                                    <div class="timeline-item">
                                        <small class="text-muted">@step.Timestamp.ToString("HH:mm:ss")</small>
                                        <div class="ms-3">@step.Message</div>
                                    </div>
                                }
                            </div>
                        }
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" @onclick="CloseOrderDetails">Close</button>
                    </div>
                </div>
            </div>
        </div>
        <div class="modal-backdrop show"></div>
    }
</div>

<style>
.metric-card {
    padding: 0.5rem;
}

.timeline {
    position: relative;
    padding-left: 1rem;
}

.timeline-item {
    position: relative;
    padding-bottom: 1rem;
    border-left: 2px solid #dee2e6;
    padding-left: 1rem;
    margin-left: 0.5rem;
}

.timeline-item:before {
    content: '';
    position: absolute;
    left: -5px;
    top: 5px;
    width: 8px;
    height: 8px;
    background: #007bff;
    border-radius: 50%;
}

.timeline-item:last-child {
    border-left: none;
}
</style>

@code {
    private List<OrderHistoryItem> orderHistory = new();
    private OrderHistoryItem? selectedOrder = null;
    private Timer? refreshTimer;
    private bool autoRefreshEnabled = true;
    private int refreshIntervalSeconds = 5;
    private DateTime lastRefreshTime = DateTime.Now;
    private HubConnection? hubConnection;
    private bool isConnected = false;

    private int completedOrders => orderHistory.Count(o => o.Status == OrderStatus.Completed);
    private int inProgressOrders => orderHistory.Count(o => o.Status == OrderStatus.InProgress);
    private int failedOrders => orderHistory.Count(o => o.Status == OrderStatus.Failed);
    private string averageProcessingTime => GetAverageProcessingTime();

    protected override async Task OnInitializedAsync()
    {
        await RefreshOrders();
        await InitializeSignalR();
        StartAutoRefresh();
    }

    private async Task InitializeSignalR()
    {
        hubConnection = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri("/kitchenhub"))
            .Build();

        hubConnection.On<OrderHistoryItem>("OrderStarted", (order) =>
        {
            InvokeAsync(() =>
            {
                orderHistory.Insert(0, order);
                StateHasChanged();
            });
        });

        hubConnection.On<OrderHistoryItem>("OrderCompleted", (order) =>
        {
            InvokeAsync(() =>
            {
                var existingOrder = orderHistory.FirstOrDefault(o => o.Id == order.Id);
                if (existingOrder != null)
                {
                    var index = orderHistory.IndexOf(existingOrder);
                    orderHistory[index] = order;
                }
                StateHasChanged();
            });
        });

        hubConnection.On<OrderHistoryItem>("OrderFailed", (order) =>
        {
            InvokeAsync(() =>
            {
                var existingOrder = orderHistory.FirstOrDefault(o => o.Id == order.Id);
                if (existingOrder != null)
                {
                    var index = orderHistory.IndexOf(existingOrder);
                    orderHistory[index] = order;
                }
                StateHasChanged();
            });
        });

        hubConnection.On<string, string>("OrderProgress", (orderId, message) =>
        {
            InvokeAsync(() =>
            {
                var order = orderHistory.FirstOrDefault(o => o.Id == orderId);
                if (order != null)
                {
                    order.ProgressSteps.Add(new ProgressStep 
                    { 
                        Timestamp = DateTime.Now, 
                        Message = message 
                    });
                    StateHasChanged();
                }
            });
        });

        await hubConnection.StartAsync();
        await hubConnection.SendAsync("JoinKitchenGroup");
        isConnected = true;
    }

    private void StartAutoRefresh()
    {
        refreshTimer = new Timer(async _ => 
        {
            if (autoRefreshEnabled)
            {
                await InvokeAsync(async () =>
                {
                    await RefreshOrders();
                    StateHasChanged();
                });
            }
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(refreshIntervalSeconds));
    }

    private async Task RefreshOrders()
    {
        orderHistory = await Kitchen.GetOrderHistoryAsync();
        lastRefreshTime = DateTime.Now;
    }

    private void ClearHistory()
    {
        Kitchen.ClearOrderHistory();
        orderHistory.Clear();
    }

    private void ViewOrderDetails(string orderId)
    {
        selectedOrder = orderHistory.FirstOrDefault(o => o.Id == orderId);
    }

    private void CloseOrderDetails()
    {
        selectedOrder = null;
    }

    private string GetRowClass(OrderStatus status) => status switch
    {
        OrderStatus.Completed => "table-success",
        OrderStatus.Failed => "table-danger",
        OrderStatus.InProgress => "table-warning",
        _ => ""
    };

    private string GetStatusBadgeClass(OrderStatus status) => status switch
    {
        OrderStatus.Completed => "bg-success",
        OrderStatus.Failed => "bg-danger",
        OrderStatus.InProgress => "bg-warning text-dark",
        _ => "bg-secondary"
    };

    private string GetStatusIcon(OrderStatus status) => status switch
    {
        OrderStatus.Completed => "✅",
        OrderStatus.Failed => "❌",
        OrderStatus.InProgress => "⏳",
        _ => "❓"
    };

    private string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalMinutes < 1)
            return $"{duration.Seconds}s";
        else if (duration.TotalHours < 1)
            return $"{duration.Minutes}m {duration.Seconds}s";
        else
            return $"{duration.Hours}h {duration.Minutes}m";
    }

    private string GetAverageProcessingTime()
    {
        var completedOrdersWithTime = orderHistory
            .Where(o => o.Status == OrderStatus.Completed && o.CompletedAt.HasValue)
            .ToList();

        if (!completedOrdersWithTime.Any())
            return "N/A";

        var avgTicks = completedOrdersWithTime
            .Average(o => (o.CompletedAt!.Value - o.Timestamp).Ticks);

        return FormatDuration(new TimeSpan((long)avgTicks));
    }

    public void Dispose()
    {
        refreshTimer?.Dispose();
        
        if (hubConnection is not null)
        {
            _ = hubConnection.SendAsync("LeaveKitchenGroup");
            _ = hubConnection.DisposeAsync();
        }
    }
}
```

### Orchestrator/wwwroot/css/app.css
```css
html, body {
    font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif;
}

h1:focus {
    outline: none;
}

a, .btn-link {
    color: #0071c1;
}

.btn-primary {
    color: #fff;
    background-color: #1b6ec2;
    border-color: #1861ac;
}

.content {
    padding-top: 1.1rem;
}

.valid.modified:not([type=checkbox]) {
    outline: 1px solid #26b050;
}

.invalid {
    outline: 1px solid red;
}

.validation-message {
    color: red;
}

#blazor-error-ui {
    background: lightyellow;
    bottom: 0;
    box-shadow: 0 -1px 2px rgba(0, 0, 0, 0.2);
    display: none;
    left: 0;
    padding: 0.6rem 1.25rem 0.7rem 1.25rem;
    position: fixed;
    width: 100%;
    z-index: 1000;
}

#blazor-error-ui .dismiss {
    cursor: pointer;
    position: absolute;
    right: 0.75rem;
    top: 0.5rem;
}

.page {
    position: relative;
    display: flex;
    flex-direction: column;
}

.main {
    flex: 1;
}

.sidebar {
    background-image: linear-gradient(180deg, rgb(5, 39, 103) 0%, #3a0647 70%);
}

.top-row {
    background-color: #f7f7f7;
    border-bottom: 1px solid #d6d5d5;
    justify-content: flex-end;
    height: 3.5rem;
    display: flex;
    align-items: center;
}

.top-row ::deep a, .top-row ::deep .btn-link {
    white-space: nowrap;
    margin-left: 1.5rem;
    text-decoration: none;
}

.top-row ::deep a:hover, .top-row ::deep .btn-link:hover {
    text-decoration: underline;
}

.top-row ::deep a:first-child {
    overflow: hidden;
    text-overflow: ellipsis;
}

@media (max-width: 640.98px) {
    .top-row:not(.auth) {
        display: none;
    }

    .top-row.auth {
        justify-content: space-between;
    }

    .top-row ::deep a, .top-row ::deep .btn-link {
        margin-left: 0;
    }
}

@media (min-width: 641px) {
    .page {
        flex-direction: row;
    }

    .sidebar {
        width: 250px;
        height: 100vh;
        position: sticky;
        top: 0;
    }

    .top-row {
        position: sticky;
        top: 0;
        z-index: 1;
    }

    .top-row.auth ::deep a:first-child {
        flex: 1;
        text-align: right;
        width: 0;
    }

    .top-row, article {
        padding-left: 2rem !important;
        padding-right: 1.5rem !important;
    }
}
```

### Orchestrator/_Imports.razor
```razor
@using System.Net.Http
@using Microsoft.AspNetCore.Authorization
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.JSInterop
@using Orchestrator
@using Orchestrator.Shared
```

### Orchestrator/Orchestrator.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsAspireClientProject>true</IsAspireClientProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Aspire.Microsoft.Extensions.ServiceDiscovery" Version="9.1.0" />
    <PackageReference Include="Azure.Identity" Version="1.12.1" />
    <PackageReference Include="Microsoft.Extensions.AI" Version="9.0.0-preview.3.24174.3" />
    <PackageReference Include="Microsoft.Extensions.AI.OpenAI" Version="9.0.0-preview.3.24174.3" />
    <PackageReference Include="ModelContextProtocol.Client" Version="0.3.0-preview.3" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\AgentBurgers.ServiceDefaults\AgentBurgers.ServiceDefaults.csproj" />
  </ItemGroup>
</Project>
```

### Orchestrator/AgentBurgers.http
```http
### AgentBurgers API Test Scenarios
### This file contains comprehensive test scenarios for the AgentBurgers kitchen API
### Use with VS Code REST Client extension or similar HTTP testing tools

# Variables - Update these to match your environment
@baseUrl = https://localhost:7042
@contentType = application/json

### Health Check
GET {{baseUrl}}/health

### Alive Check  
GET {{baseUrl}}/alive

### ===== BASIC ORDERS =====

### Simple Cheeseburger
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "I'd like a cheeseburger, please"
}

### Cheeseburger with Fries
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "Can I get a cheeseburger with fries?"
}

### Basic Combo Meal
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "I want a burger, fries, and a vanilla shake"
}
```

### Orchestrator/Properties/launchSettings.json
```csharp
using Microsoft.Extensions.AI.OpenAI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.Text;
using System.Collections.Concurrent;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add Blazor services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add AI Chat Client
builder.Services.AddOpenAIChatClient("default", builder => builder.UseOpenAI(options =>
{
    options.Endpoint = new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? "https://your-azure-openai-endpoint");
    options.DeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-4";
    // Use Azure Default Credential instead of API key
    options.Credential = new Azure.Identity.DefaultAzureCredential();
}));

// Configure HTTP clients for MCP services using service discovery
builder.Services.AddHttpClient<McpToolClient>("grillagent", client =>
{
    client.BaseAddress = new Uri("http://grillagent");
});

builder.Services.AddHttpClient<McpToolClient>("fryeragent", client =>
{
    client.BaseAddress = new Uri("http://fryeragent");
});

builder.Services.AddHttpClient<McpToolClient>("dessertagent", client =>
{
    client.BaseAddress = new Uri("http://dessertagent");
});

builder.Services.AddHttpClient<McpToolClient>("platingagent", client =>
{
    client.BaseAddress = new Uri("http://platingagent");
});

// Register the Kitchen Service
builder.Services.AddSingleton<KitchenService>();

var app = builder.Build();

app.MapDefaultEndpoints();

// API Endpoints
app.MapPost("/api/order", async (OrderRequest request, KitchenService kitchen) =>
{
    var response = await kitchen.ProcessOrderAsync(request.Order);
    return Results.Ok(new { response });
});

app.MapGet("/api/order-stream/{orderId}", async (string orderId, KitchenService kitchen) =>
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

// DTOs
public record OrderRequest(string Order);

// Kitchen Service
public class KitchenService
{
    private readonly IChatClient _chatClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly List<AIFunction> _allTools = new();
    private readonly string _systemPrompt;
    private readonly ConcurrentDictionary<string, List<ChatMessage>> _conversations = new();
    private readonly ConcurrentDictionary<string, List<string>> _orderProgress = new();

    public KitchenService(IChatClient chatClient, IHttpClientFactory httpClientFactory)
    {
        _chatClient = chatClient;
        _httpClientFactory = httpClientFactory;
        _systemPrompt = LoadSystemPrompt();
        _ = InitializeToolsAsync();
    }

    private async Task InitializeToolsAsync()
    {
        var agentNames = new[] { "grillagent", "fryeragent", "dessertagent", "platingagent" };

        foreach (var agentName in agentNames)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(agentName);
                var mcpClient = new McpToolClient(httpClient.BaseAddress!);
                var tools = await mcpClient.ListToolsAsync();
                _allTools.AddRange(tools);
            }
            catch (Exception ex)
            {
                // Log error but continue - some agents might not be ready yet
                Console.WriteLine($"Warning: Could not connect to {agentName}: {ex.Message}");
            }
        }
    }

    public async Task<string> ProcessOrderAsync(string order)
    {
        var orderId = Guid.NewGuid().ToString();
        var history = new List<ChatMessage> 
        { 
            ChatMessage.CreateSystemMessage(_systemPrompt),
            ChatMessage.CreateUserMessage(order)
        };

        _conversations[orderId] = history;
        _orderProgress[orderId] = new List<string> { $"Processing order: {order}" };

        try
        {
            var result = await _chatClient.CompleteAsync(history, new ChatOptions 
            { 
                Tools = _allTools,
                ToolCallBehavior = ToolCallBehavior.AutoInvoke
            });

            _orderProgress[orderId].Add($"Order completed: {result.Message.Text}");
            return result.Message.Text ?? "Order processed successfully.";
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error processing order: {ex.Message}";
            _orderProgress[orderId].Add(errorMessage);
            return errorMessage;
        }
    }

    public async IAsyncEnumerable<string> ProcessOrderStreamAsync(string order)
    {
        var orderId = Guid.NewGuid().ToString();
        var history = new List<ChatMessage> 
        { 
            ChatMessage.CreateSystemMessage(_systemPrompt),
            ChatMessage.CreateUserMessage(order)
        };

        yield return $"Processing order: {order}";

        try
        {
            await foreach (var update in _chatClient.CompleteStreamingAsync(history, new ChatOptions 
            { 
                Tools = _allTools,
                ToolCallBehavior = ToolCallBehavior.AutoInvoke
            }))
            {
                if (!string.IsNullOrEmpty(update.Text))
                {
                    yield return update.Text;
                }
            }
        }
        catch (Exception ex)
        {
            yield return $"Error: {ex.Message}";
        }
    }

    public IEnumerable<string> GetOrderProgressStream(string orderId)
    {
        return _orderProgress.GetValueOrDefault(orderId, new List<string>());
    }

    private static string LoadSystemPrompt()
    {
        return $"""
        You are a kitchen expediter for AgentBurgers restaurant. You interpret customer orders and coordinate with specialized kitchen stations to fulfill them.

        Your role is to:
        1. Parse customer orders and identify what needs to be prepared
        2. Use the appropriate tools from each kitchen station in the correct sequence
        3. Coordinate between stations (grill, fryer, dessert, plating) to complete orders efficiently
        4. Provide clear updates on order progress

        Kitchen Station Instructions:
        {LoadAllAgentInstructions()}

        Always follow the tool constraints for each agent. Never use tools outside of an agent's defined capabilities.
        Provide clear, friendly responses about order progress and completion.
        """;
    }

    private static string LoadAllAgentInstructions()
    {
        var agents = new[]
        {
            "../Agents/GrillAgent/Instructions.md",
            "../Agents/FryerAgent/Instructions.md", 
            "../Agents/DessertAgent/Instructions.md",
            "../Agents/PlatingAgent/Instructions.md"
        };

        var sb = new StringBuilder();
        foreach (var path in agents)
        {
            if (File.Exists(path))
            {
                sb.AppendLine(File.ReadAllText(path));
                sb.AppendLine("\n---\n");
            }
        }
        return sb.ToString();
    }
}

// Background service that simulates random orders
public class OrderSimulatorService : BackgroundService
{
    private readonly KitchenService _kitchenService;
    private readonly ILogger<OrderSimulatorService> _logger;
    private readonly List<string> _chaosOrders;
    private readonly List<string> _normalOrders;
    private readonly Random _random = new();

    public OrderSimulatorService(KitchenService kitchenService, ILogger<OrderSimulatorService> logger)
    {
        _kitchenService = kitchenService;
        _logger = logger;
        _chaosOrders = LoadChaosOrders();
        _normalOrders = LoadNormalOrders();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Order Simulator Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Wait 1 second between orders
                await Task.Delay(1000, stoppingToken);

                // Choose between chaos (20% chance) and normal orders (80% chance)
                var useChaosOrder = _random.NextDouble() < 0.2;
                var orders = useChaosOrder ? _chaosOrders : _normalOrders;
                
                if (orders.Count > 0)
                {
                    var randomOrder = orders[_random.Next(orders.Count)];
                    var orderType = useChaosOrder ? "CHAOS" : "NORMAL";
                    
                    _logger.LogInformation($"🎲 Simulating {orderType} order: {randomOrder[..Math.Min(50, randomOrder.Length)]}...");
                    
                    // Process the order without waiting for completion to avoid blocking
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var response = await _kitchenService.ProcessOrderAsync(randomOrder);
                            _logger.LogInformation($"✅ {orderType} order completed: {response[..Math.Min(100, response.Length)]}...");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"❌ {orderType} order failed");
                        }
                    }, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Order Simulator Service");
            }
        }

        _logger.LogInformation("Order Simulator Service stopped");
    }

    private static List<string> LoadChaosOrders()
    {
        var chaosOrdersPath = "../Prompts/TicketChaos.md";
        if (!File.Exists(chaosOrdersPath))
            return new List<string>();

        var orders = new List<string>();
        var content = File.ReadAllText(chaosOrdersPath);
        
        // Extract orders from the markdown (look for numbered items)
        var lines = content.Split('\n');
        foreach (var line in lines)
        {
            if (line.Trim().StartsWith('"') && line.Trim().EndsWith('"'))
            {
                var order = line.Trim().Trim('"');
                if (!string.IsNullOrEmpty(order))
                    orders.Add(order);
            }
        }

        return orders;
    }

    private static List<string> LoadNormalOrders()
    {
        var normalOrdersPath = "../Prompts/NormalOrders.md";
        if (!File.Exists(normalOrdersPath))
            return new List<string>();

        var orders = new List<string>();
        var content = File.ReadAllText(normalOrdersPath);
        
        // Extract orders from the markdown (look for numbered items)
        var lines = content.Split('\n');
        foreach (var line in lines)
        {
            if (line.Trim().StartsWith('"') && line.Trim().EndsWith('"'))
            {
                var order = line.Trim().Trim('"');
                if (!string.IsNullOrEmpty(order))
                    orders.Add(order);
            }
        }

        return orders;
    }
}
```

### Pages/_Host.cshtml
```html
@page "/"
@namespace Orchestrator.Pages
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@{
    Layout = "_Layout";
}

<component type="typeof(App)" render-mode="ServerPrerendered" />
```

### Pages/Shared/_Layout.cshtml
```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>AgentBurgers Kitchen</title>
    <base href="~/" />
    <link href="css/bootstrap/bootstrap.min.css" rel="stylesheet" />
    <link href="css/app.css" rel="stylesheet" />
    <link href="Orchestrator.styles.css" rel="stylesheet" />
</head>
<body>
    @RenderBody()

    <div id="blazor-error-ui">
        <environment include="Staging,Production">
            An error has occurred. This application may no longer respond until reloaded.
        </environment>
        <environment include="Development">
            An unhandled exception has occurred. See browser dev tools for details.
        </environment>
        <a href="" class="reload">Reload</a>
        <a class="dismiss">🗙</a>
    </div>

    <script src="_framework/blazor.server.js"></script>
</body>
</html>
```

### App.razor
```razor
<Router AppAssembly="@typeof(App).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
        <FocusOnNavigate RouteData="@routeData" Selector="h1" />
    </Found>
    <NotFound>
        <PageTitle>Not found</PageTitle>
        <LayoutView Layout="@typeof(MainLayout)">
            <p role="alert">Sorry, there's nothing at this address.</p>
        </LayoutView>
    </NotFound>
</Router>
```

### Shared/MainLayout.razor
```razor
@inherits LayoutView

<div class="page">
    <div class="sidebar">
        <NavMenu />
    </div>

    <main>
        <div class="top-row px-4">
            <h1>🍔 AgentBurgers Kitchen</h1>
        </div>

        <article class="content px-4">
            @Body
        </article>
    </main>
</div>
```

### Shared/NavMenu.razor
```razor
<div class="top-row ps-3 navbar navbar-dark">
    <div class="container-fluid">
        <a class="navbar-brand" href="">AgentBurgers</a>
        <button title="Navigation menu" class="navbar-toggler" @onclick="ToggleNavMenu">
            <span class="navbar-toggler-icon"></span>
        </button>
    </div>
</div>

<div class="@NavMenuCssClass nav-scrollable" @onclick="CollapseNavMenu">
    <nav class="flex-column">
        <div class="nav-item px-3">
            <NavLink class="nav-link" href="" Match="NavLinkMatch.All">
                <span class="oi oi-home" aria-hidden="true"></span> Kitchen
            </NavLink>
        </div>
        <div class="nav-item px-3">
            <NavLink class="nav-link" href="orders">
                <span class="oi oi-list-rich" aria-hidden="true"></span> Order History
            </NavLink>
        </div>
    </nav>
</div>

@code {
    private bool collapseNavMenu = true;

    private string? NavMenuCssClass => collapseNavMenu ? "collapse" : null;

    private void ToggleNavMenu()
    {
        collapseNavMenu = !collapseNavMenu;
    }

    private void CollapseNavMenu()
    {
        collapseNavMenu = true;
    }
}
```

### Pages/Kitchen.razor
```razor
@page "/"
@using Microsoft.Extensions.AI
@inject KitchenService Kitchen
@inject IJSRuntime JSRuntime

<PageTitle>Kitchen - AgentBurgers</PageTitle>

<div class="container-fluid">
    <div class="row">
        <div class="col-md-8">
            <div class="card">
                <div class="card-header">
                    <h3>🍔 Place Your Order</h3>
                </div>
                <div class="card-body">
                    <div class="mb-3">
                        <label for="orderInput" class="form-label">What would you like to order?</label>
                        <textarea class="form-control" id="orderInput" rows="3" @bind="currentOrder" 
                                placeholder="E.g., I'd like a cheeseburger with fries and a chocolate shake"></textarea>
                    </div>
                    <button class="btn btn-primary" @onclick="ProcessOrder" disabled="@isProcessing">
                        @if (isProcessing)
                        {
                            <span class="spinner-border spinner-border-sm me-2" role="status"></span>
                        }
                        Place Order
                    </button>
                    <button class="btn btn-secondary ms-2" @onclick="ClearOrder">Clear</button>
                </div>
            </div>

            @if (!string.IsNullOrEmpty(orderResponse))
            {
                <div class="card mt-3">
                    <div class="card-header">
                        <h4>Order Response</h4>
                    </div>
                    <div class="card-body">
                        <div class="alert alert-success">
                            @orderResponse
                        </div>
                    </div>
                </div>
            }
        </div>

        <div class="col-md-4">
            <div class="card">
                <div class="card-header">
                    <h4>Kitchen Status</h4>
                </div>
                <div class="card-body">
                    <div class="mb-2">
                        <span class="badge bg-success me-2">🔥 Grill Station</span>
                        <small class="text-muted">Active</small>
                    </div>
                    <div class="mb-2">
                        <span class="badge bg-warning me-2">🍟 Fryer Station</span>
                        <small class="text-muted">Active</small>
                    </div>
                    <div class="mb-2">
                        <span class="badge bg-info me-2">🍦 Dessert Station</span>
                        <small class="text-muted">Active</small>
                    </div>
                    <div class="mb-2">
                        <span class="badge bg-secondary me-2">🍽️ Plating Station</span>
                        <small class="text-muted">Active</small>
                    </div>
                </div>
            </div>

            <div class="card mt-3">
                <div class="card-header">
                    <h5>Sample Orders</h5>
                </div>
                <div class="card-body">
                    <div class="list-group list-group-flush">
                        @foreach (var sample in sampleOrders)
                        {
                            <button class="list-group-item list-group-item-action" @onclick="() => SelectSampleOrder(sample)">
                                @sample
                            </button>
                        }
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

@code {
    private string currentOrder = string.Empty;
    private string orderResponse = string.Empty;
    private bool isProcessing = false;

    private readonly List<string> sampleOrders = new()
    {
        "Cheeseburger with fries",
        "Bacon burger with sweet potato fries and a vanilla shake",
        "Double cheeseburger with waffle fries and a fudge sundae",
        "Just a chocolate shake and regular fries",
        "Burger with everything, fries, and a caramel sundae"
    };

    private async Task ProcessOrder()
    {
        if (string.IsNullOrWhiteSpace(currentOrder)) return;

        isProcessing = true;
        orderResponse = string.Empty;
        StateHasChanged();

        try
        {
            orderResponse = await Kitchen.ProcessOrderAsync(currentOrder);
        }
        catch (Exception ex)
        {
            orderResponse = $"Sorry, there was an error processing your order: {ex.Message}";
        }
        finally
        {
            isProcessing = false;
            StateHasChanged();
        }
    }

    private void ClearOrder()
    {
        currentOrder = string.Empty;
        orderResponse = string.Empty;
    }

    private void SelectSampleOrder(string order)
    {
        currentOrder = order;
    }
}
```

### Pages/Orders.razor
```razor
@page "/orders"
@inject KitchenService Kitchen
@implements IDisposable

<PageTitle>Order History - AgentBurgers</PageTitle>

<div class="container-fluid">
    <div class="row">
        <!-- Order History Panel -->
        <div class="col-lg-8">
            <div class="card">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <h3>📋 Order History</h3>
                    <div>
                        <button class="btn btn-outline-secondary btn-sm me-2" @onclick="RefreshOrders">
                            <i class="fas fa-refresh"></i> Refresh
                        </button>
                        <button class="btn btn-outline-danger btn-sm" @onclick="ClearHistory">
                            <i class="fas fa-trash"></i> Clear History
                        </button>
                    </div>
                </div>
                <div class="card-body">
                    @if (orderHistory.Any())
                    {
                        <div class="table-responsive">
                            <table class="table table-hover">
                                <thead class="table-light">
                                    <tr>
                                        <th>Order ID</th>
                                        <th>Time</th>
                                        <th>Order Details</th>
                                        <th>Status</th>
                                        <th>Duration</th>
                                        <th>Actions</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    @foreach (var order in orderHistory.OrderByDescending(o => o.Timestamp))
                                    {
                                        <tr class="@GetRowClass(order.Status)">
                                            <td>
                                                <small class="font-monospace">@order.Id[..8]...</small>
                                            </td>
                                            <td>
                                                <small>@order.Timestamp.ToString("HH:mm:ss")</small>
                                            </td>
                                            <td>
                                                <div class="text-truncate" style="max-width: 200px;" title="@order.OrderText">
                                                    @order.OrderText
                                                </div>
                                            </td>
                                            <td>
                                                <span class="badge @GetStatusBadgeClass(order.Status)">
                                                    @GetStatusIcon(order.Status) @order.Status
                                                </span>
                                            </td>
                                            <td>
                                                @if (order.CompletedAt.HasValue)
                                                {
                                                    <small>@FormatDuration(order.CompletedAt.Value - order.Timestamp)</small>
                                                }
                                                else
                                                {
                                                    <small class="text-muted">In progress...</small>
                                                }
                                            </td>
                                            <td>
                                                <button class="btn btn-sm btn-outline-primary" @onclick="() => ViewOrderDetails(order.Id)">
                                                    <i class="fas fa-eye"></i>
                                                </button>
                                            </td>
                                        </tr>
                                    }
                                </tbody>
                            </table>
                        </div>
                    }
                    else
                    {
                        <div class="text-center py-5">
                            <i class="fas fa-clipboard-list fa-3x text-muted mb-3"></i>
                            <h5 class="text-muted">No orders yet</h5>
                            <p class="text-muted">Order history will appear here as orders are processed.</p>
                        </div>
                    }
                </div>
            </div>
        </div>

        <!-- Kitchen Metrics Panel -->
        <div class="col-lg-4">
            <div class="card mb-3">
                <div class="card-header">
                    <h5>📊 Kitchen Performance</h5>
                </div>
                <div class="card-body">
                    <div class="row text-center">
                        <div class="col-6">
                            <div class="metric-card">
                                <h3 class="text-success">@completedOrders</h3>
                                <small class="text-muted">Completed</small>
                            </div>
                        </div>
                        <div class="col-6">
                            <div class="metric-card">
                                <h3 class="text-warning">@inProgressOrders</h3>
                                <small class="text-muted">In Progress</small>
                            </div>
                        </div>
                    </div>
                    <hr>
                    <div class="row text-center">
                        <div class="col-6">
                            <div class="metric-card">
                                <h4 class="text-primary">@failedOrders</h4>
                                <small class="text-muted">Failed</small>
                            </div>
                        </div>
                        <div class="col-6">
                            <div class="metric-card">
                                <h4 class="text-info">@averageProcessingTime</h4>
                                <small class="text-muted">Avg Time</small>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="card">
                <div class="card-header">
                    <h5>🔄 Real-time Updates</h5>
                </div>
                <div class="card-body">
                    <div class="mb-2">
                        <span class="badge bg-success me-2">
                            🟢 Event-Driven Updates
                        </span>
                        <small class="text-muted">Service Events Active</small>
                    </div>
                    <hr>
                    <small class="text-muted">
                        Last updated: @lastRefreshTime.ToString("HH:mm:ss")
                    </small>
                </div>
            </div>
        </div>
    </div>

    <!-- Order Details Modal -->
    @if (selectedOrder != null)
    {
        <div class="modal show d-block" tabindex="-1">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Order Details - @selectedOrder.Id[..8]</h5>
                        <button type="button" class="btn-close" @onclick="CloseOrderDetails"></button>
                    </div>
                    <div class="modal-body">
                        <div class="row">
                            <div class="col-md-6">
                                <h6>Order Information</h6>
                                <table class="table table-sm">
                                    <tr>
                                        <td><strong>Order ID:</strong></td>
                                        <td><span class="font-monospace">@selectedOrder.Id</span></td>
                                    </tr>
                                    <tr>
                                        <td><strong>Submitted:</strong></td>
                                        <td>@selectedOrder.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")</td>
                                    </tr>
                                    <tr>
                                        <td><strong>Status:</strong></td>
                                        <td><span class="badge @GetStatusBadgeClass(selectedOrder.Status)">@selectedOrder.Status</span></td>
                                    </tr>
                                    @if (selectedOrder.CompletedAt.HasValue)
                                    {
                                        <tr>
                                            <td><strong>Completed:</strong></td>
                                            <td>@selectedOrder.CompletedAt.Value.ToString("yyyy-MM-dd HH:mm:ss")</td>
                                        </tr>
                                        <tr>
                                            <td><strong>Duration:</strong></td>
                                            <td>@FormatDuration(selectedOrder.CompletedAt.Value - selectedOrder.Timestamp)</td>
                                        </tr>
                                    }
                                </table>
                            </div>
                            <div class="col-md-6">
                                <h6>Order Content</h6>
                                <div class="border rounded p-3 bg-light">
                                    <strong>Customer Request:</strong><br>
                                    <em>"@selectedOrder.OrderText"</em>
                                </div>
                                @if (!string.IsNullOrEmpty(selectedOrder.Response))
                                {
                                    <div class="border rounded p-3 bg-light mt-3">
                                        <strong>Kitchen Response:</strong><br>
                                        @selectedOrder.Response
                                    </div>
                                }
                            </div>
                        </div>
                        
                        @if (selectedOrder.ProgressSteps.Any())
                        {
                            <hr>
                            <h6>Processing Steps</h6>
                            <div class="timeline">
                                @foreach (var step in selectedOrder.ProgressSteps)
                                {
                                    <div class="timeline-item">
                                        <small class="text-muted">@step.Timestamp.ToString("HH:mm:ss")</small>
                                        <div class="ms-3">@step.Message</div>
                                    </div>
                                }
                            </div>
                        }
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" @onclick="CloseOrderDetails">Close</button>
                    </div>
                </div>
            </div>
        </div>
        <div class="modal-backdrop show"></div>
    }
</div>

<style>
.metric-card {
    padding: 0.5rem;
}

.timeline {
    position: relative;
    padding-left: 1rem;
}

.timeline-item {
    position: relative;
    padding-bottom: 1rem;
    border-left: 2px solid #dee2e6;
    padding-left: 1rem;
    margin-left: 0.5rem;
}

.timeline-item:before {
    content: '';
    position: absolute;
    left: -5px;
    top: 5px;
    width: 8px;
    height: 8px;
    background: #007bff;
    border-radius: 50%;
}

.timeline-item:last-child {
    border-left: none;
}
</style>

@code {
    private List<OrderHistoryItem> orderHistory = new();
    private OrderHistoryItem? selectedOrder = null;
    private DateTime lastRefreshTime = DateTime.Now;

    private int completedOrders => orderHistory.Count(o => o.Status == OrderStatus.Completed);
    private int inProgressOrders => orderHistory.Count(o => o.Status == OrderStatus.InProgress);
    private int failedOrders => orderHistory.Count(o => o.Status == OrderStatus.Failed);
    private string averageProcessingTime => GetAverageProcessingTime();

    protected override Task OnInitializedAsync()
    {
        // Subscribe to kitchen service events
        Kitchen.OrderStarted += OnOrderStarted;
        Kitchen.OrderCompleted += OnOrderCompleted;
        Kitchen.OrderFailed += OnOrderFailed;
        Kitchen.OrderProgressUpdated += OnOrderProgressUpdated;

        RefreshOrders();
        return Task.CompletedTask;
    }

    private void OnOrderStarted(object? sender, OrderHistoryItem order)
    {
        InvokeAsync(() =>
        {
            // Add to the beginning of the list for newest first
            orderHistory.Insert(0, order);
            lastRefreshTime = DateTime.Now;
            StateHasChanged();
        });
    }

    private void OnOrderCompleted(object? sender, OrderHistoryItem order)
    {
        InvokeAsync(() =>
        {
            var existingOrder = orderHistory.FirstOrDefault(o => o.Id == order.Id);
            if (existingOrder != null)
            {
                var index = orderHistory.IndexOf(existingOrder);
                orderHistory[index] = order;
            }
            lastRefreshTime = DateTime.Now;
            StateHasChanged();
        });
    }

    private void OnOrderFailed(object? sender, OrderHistoryItem order)
    {
        InvokeAsync(() =>
        {
            var existingOrder = orderHistory.FirstOrDefault(o => o.Id == order.Id);
            if (existingOrder != null)
            {
                var index = orderHistory.IndexOf(existingOrder);
                orderHistory[index] = order;
            }
            lastRefreshTime = DateTime.Now;
            StateHasChanged();
        });
    }

    private void OnOrderProgressUpdated(object? sender, (string OrderId, string Message) update)
    {
        InvokeAsync(() =>
        {
            var order = orderHistory.FirstOrDefault(o => o.Id == update.OrderId);
            if (order != null)
            {
                order.ProgressSteps.Add(new ProgressStep 
                { 
                    Timestamp = DateTime.Now, 
                    Message = update.Message 
                });
                lastRefreshTime = DateTime.Now;
                StateHasChanged();
            }
        });
    }

    private void RefreshOrders()
    {
        orderHistory = Kitchen.GetOrderHistory();
        lastRefreshTime = DateTime.Now;
    }

    private void ClearHistory()
    {
        Kitchen.ClearOrderHistory();
        orderHistory.Clear();
    }

    private void ViewOrderDetails(string orderId)
    {
        selectedOrder = orderHistory.FirstOrDefault(o => o.Id == orderId);
    }

    private void CloseOrderDetails()
    {
        selectedOrder = null;
    }

    private string GetRowClass(OrderStatus status) => status switch
    {
        OrderStatus.Completed => "table-success",
        OrderStatus.Failed => "table-danger",
        OrderStatus.InProgress => "table-warning",
        _ => ""
    };

    private string GetStatusBadgeClass(OrderStatus status) => status switch
    {
        OrderStatus.Completed => "bg-success",
        OrderStatus.Failed => "bg-danger",
        OrderStatus.InProgress => "bg-warning text-dark",
        _ => "bg-secondary"
    };

    private string GetStatusIcon(OrderStatus status) => status switch
    {
        OrderStatus.Completed => "✅",
        OrderStatus.Failed => "❌",
        OrderStatus.InProgress => "⏳",
        _ => "❓"
    };

    private string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalMinutes < 1)
            return $"{duration.Seconds}s";
        else if (duration.TotalHours < 1)
            return $"{duration.Minutes}m {duration.Seconds}s";
        else
            return $"{duration.Hours}h {duration.Minutes}m";
    }

    private string GetAverageProcessingTime()
    {
        var completedOrdersWithTime = orderHistory
            .Where(o => o.Status == OrderStatus.Completed && o.CompletedAt.HasValue)
            .ToList();

        if (!completedOrdersWithTime.Any())
            return "N/A";

        var avgTicks = completedOrdersWithTime
            .Average(o => (o.CompletedAt!.Value - o.Timestamp).Ticks);

        return FormatDuration(new TimeSpan((long)avgTicks));
    }

    public void Dispose()
    {
        // Unsubscribe from events to prevent memory leaks
        Kitchen.OrderStarted -= OnOrderStarted;
        Kitchen.OrderCompleted -= OnOrderCompleted;
        Kitchen.OrderFailed -= OnOrderFailed;
        Kitchen.OrderProgressUpdated -= OnOrderProgressUpdated;
    }
}
```

### wwwroot/css/app.css
```css
html, body {
    font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif;
}

h1:focus {
    outline: none;
}

a, .btn-link {
    color: #0071c1;
}

.btn-primary {
    color: #fff;
    background-color: #1b6ec2;
    border-color: #1861ac;
}

.content {
    padding-top: 1.1rem;
}

.valid.modified:not([type=checkbox]) {
    outline: 1px solid #26b050;
}

.invalid {
    outline: 1px solid red;
}

.validation-message {
    color: red;
}

#blazor-error-ui {
    background: lightyellow;
    bottom: 0;
    box-shadow: 0 -1px 2px rgba(0, 0, 0, 0.2);
    display: none;
    left: 0;
    padding: 0.6rem 1.25rem 0.7rem 1.25rem;
    position: fixed;
    width: 100%;
    z-index: 1000;
}

#blazor-error-ui .dismiss {
    cursor: pointer;
    position: absolute;
    right: 0.75rem;
    top: 0.5rem;
}

.page {
    position: relative;
    display: flex;
    flex-direction: column;
}

.main {
    flex: 1;
}

.sidebar {
    background-image: linear-gradient(180deg, rgb(5, 39, 103) 0%, #3a0647 70%);
}

.top-row {
    background-color: #f7f7f7;
    border-bottom: 1px solid #d6d5d5;
    justify-content: flex-end;
    height: 3.5rem;
    display: flex;
    align-items: center;
}

.top-row ::deep a, .top-row ::deep .btn-link {
    white-space: nowrap;
    margin-left: 1.5rem;
    text-decoration: none;
}

.top-row ::deep a:hover, .top-row ::deep .btn-link:hover {
    text-decoration: underline;
}

.top-row ::deep a:first-child {
    overflow: hidden;
    text-overflow: ellipsis;
}

@media (max-width: 640.98px) {
    .top-row:not(.auth) {
        display: none;
    }

    .top-row.auth {
        justify-content: space-between;
    }

    .top-row ::deep a, .top-row ::deep .btn-link {
        margin-left: 0;
    }
}

@media (min-width: 641px) {
    .page {
        flex-direction: row;
    }

    .sidebar {
        width: 250px;
        height: 100vh;
        position: sticky;
        top: 0;
    }

    .top-row {
        position: sticky;
        top: 0;
        z-index: 1;
    }

    .top-row.auth ::deep a:first-child {
        flex: 1;
        text-align: right;
        width: 0;
    }

    .top-row, article {
        padding-left: 2rem !important;
        padding-right: 1.5rem !important;
    }
}
```

### _Imports.razor
```razor
@using System.Net.Http
@using Microsoft.AspNetCore.Authorization
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.JSInterop
@using Orchestrator
@using Orchestrator.Shared
```

### Orchestrator.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsAspireClientProject>true</IsAspireClientProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Aspire.Microsoft.Extensions.ServiceDiscovery" Version="9.1.0" />
    <PackageReference Include="Azure.Identity" Version="1.12.1" />
    <PackageReference Include="Microsoft.Extensions.AI" Version="9.0.0-preview.3.24174.3" />
    <PackageReference Include="Microsoft.Extensions.AI.OpenAI" Version="9.0.0-preview.3.24174.3" />
    <PackageReference Include="ModelContextProtocol.Client" Version="0.3.0-preview.3" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\AgentBurgers.ServiceDefaults\AgentBurgers.ServiceDefaults.csproj" />
  </ItemGroup>
</Project>
```

### Properties/launchSettings.json
```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## 11. Interface Options

The enhanced orchestrator provides multiple ways to interact with the kitchen:

### 1. Minimal API Endpoints

**POST `/api/order`** - Process a single order
```json
{
  "order": "I'd like a cheeseburger with fries and a chocolate shake"
}
```

**GET `/api/order-stream/{orderId}`** - Get order progress updates

### 2. Blazor Server UI

- **Kitchen Page (`/`)**: Interactive order placement with real-time processing
- **Order History (`/orders`)**: Track past orders and kitchen performance
- **Responsive Design**: Works on desktop and mobile devices
- **Real-time Updates**: Server-side Blazor provides instant feedback

### 3. API Testing with .http File

The `AgentBurgers.http` file provides comprehensive test scenarios including:

- **Basic Orders**: Simple burgers, fries, and shakes
- **Complex Orders**: Multi-item meals with specific instructions
- **Dessert-focused Orders**: Shakes and sundaes only
- **Fries Variations**: Different types and preparations
- **Large Orders**: Family meals and party orders
- **Edge Cases**: Empty orders, invalid JSON, very long requests
- **Ambiguous Orders**: Testing AI interpretation of vague requests
- **Stress Tests**: Kitchen chaos scenarios with multiple complex items
- **Concurrent Testing**: Multiple simultaneous orders

### 4. OrderSimulator Service (Separate Node)

The OrderSimulator is a dedicated microservice in the Aspire topology that:

- **Runs independently**: Separate service that calls the Orchestrator API
- **Randomly selects orders** from both normal and chaos prompt files every second
- **Weighted selection**: 80% normal orders, 20% chaos orders for realistic simulation
- **Service discovery integration**: Uses Aspire service discovery to find the Orchestrator
- **Health check awareness**: Waits for Orchestrator to be ready before starting simulation
- **Comprehensive logging**: All simulated orders and results are logged with emojis for easy identification
- **Realistic load testing**: Provides continuous kitchen activity for testing and demonstration
- **Fallback orders**: Includes hardcoded fallback orders if prompt files aren't available

The simulator automatically loads orders from:
- `Prompts/NormalOrders.md` - 50 typical everyday orders
- `Prompts/TicketChaos.md` - 30 complex stress-test scenarios

### 5. Usage Examples

**API Usage (cURL):**
```bash
curl -X POST "https://localhost:7042/api/order" \
  -H "Content-Type: application/json" \
  -d '{"order": "Double cheeseburger with waffle fries"}'
```

**VS Code REST Client:**
Use the provided `AgentBurgers.http` file with the REST Client extension to run all test scenarios directly from VS Code.

**Blazor UI Features:**
- Pre-filled sample orders for quick testing
- Kitchen station status indicators  
- Real-time order processing with loading states
- Clear, user-friendly interface

---

---

## 12. Prompts

### TicketChaos.md
Create `Prompts/TicketChaos.md` with 30 creative and chaotic food orders to stress test orchestration:

```markdown
# Ticket Chaos - Stress Test Orders

These are intentionally complex, chaotic, and demanding orders designed to stress test the AI orchestration system and agent coordination.

## Rush Orders

1. "URGENT! I need 6 bacon cheeseburgers, 3 with only top buns toasted, 2 with both buns toasted, 1 with no bun toasting, 4 waffle fries, 3 standard fries, 2 sweet potato fries, all salted differently - light salt on waffle, heavy salt on standard, no salt on sweet potato, 3 chocolate shakes, 2 vanilla shakes, 1 strawberry shake, and 2 fudge sundaes with extra whipped cream, everything for takeout bags, and I need it in 5 minutes!"

2. "Kitchen nightmare order: 8 different burgers - 2 plain patties no cheese, 3 single cheese, 2 double cheese with bacon, 1 triple cheese with double bacon, toast every single bun differently, fry 5 batches of different fries, make every shake flavor you have, create 3 different sundaes, and I want half for dine-in, quarter for tray service, quarter bagged, and coordinate it so everything comes out at exactly the same time!"

3. "Last-minute catering disaster: 15 burgers total - 5 bacon cheeseburgers with specific instructions (2 bottom-bun-only toasted, 2 top-bun-only toasted, 1 both-buns toasted), 5 double cheeseburgers with varying bacon (3 with bacon, 2 without), 5 regular cheeseburgers, plus 8 different fry orders, 12 shakes (4 of each flavor), 6 sundaes (2 of each type), everything needs different presentation styles!"

## Indecisive Customer Orders

4. "Actually, wait, change that... I want a burger, no wait, make it a cheeseburger, actually add bacon, but only on half the burger, and can you toast just the edges of the buns? And fries, but not too crispy, but not too soft, somewhere in between, and a shake that's half chocolate half vanilla swirled together with whipped cream but not too much whipped cream."

5. "I'm ordering for my whole office but everyone wants something different and I'm not sure... can you make 4 burgers but each one completely different - one with everything, one with just cheese, one with bacon but no cheese, one with cheese but weird bun toasting, plus someone wants fries but they're allergic to salt, someone else wants extra salt, and we need shakes but make them all different somehow?"

6. "Complicated group order: I need to feed 12 people but they all have specific requests - 3 people want bacon cheeseburgers but different bun preferences, 2 want just burgers with cheese, 3 want double everything, 2 want fries only but different types, 1 wants only dessert, 1 wants only shakes, and they want it served family style but also individually plated somehow?"

## Perfectionist Orders

7. "Everything must be perfect: I need 2 bacon cheeseburgers where the cheese is melted to exactly the right consistency, the bacon is perfectly crispy but not burnt, the bottom buns are toasted golden brown but the top buns are just lightly warmed, waffle fries that are crispy outside and fluffy inside with precisely the right amount of salt, a chocolate shake that's thick but not too thick with exactly 3 dollops of whipped cream, and a fudge sundae with the fudge warmed but not hot."

8. "Gourmet precision order: 3 burgers cooked to perfection with cheese that's melted but not runny, bacon that's crispy but still chewy, buns toasted to a perfect golden ratio, 2 orders of each fry type prepared to different doneness levels, 2 shakes with specific thickness requirements, and 1 sundae with architectural precision in presentation, all coordinated for simultaneous serving."

## Multi-Station Chaos

9. "Every station working: Start 5 patties cooking immediately, begin melting cheese on 4 of them, get bacon going for 3 burgers, toast 8 bun halves in different combinations, start 3 different fry batches at staggered times, begin 4 shakes simultaneously, prep 2 sundaes, add whipped cream to everything possible, coordinate so grill finishes before fryer, dessert station stays busy throughout, and plating happens in waves!"

10. "Station coordination nightmare: I need the grill making 6 different burger combinations while the fryer handles 4 different fry orders at different salt levels, dessert station making 3 shakes and 2 sundaes with varying toppings, and plating station assembling everything in 3 different presentation styles, all while maintaining quality and speed!"

11. "Full kitchen stress test: 10 burgers with every possible combination of cheese, bacon, and bun toasting, 6 fry orders covering all types and salt preferences, 5 shakes with different whipped cream requirements, 3 sundaes with complex topping combinations, half the order for immediate dine-in service, half for delayed takeout preparation."

## Timing Nightmares

12. "Synchronized chaos: I need 4 orders that must be completed at exactly the same time - Order 1: bacon cheeseburger with waffle fries and chocolate shake, Order 2: double cheeseburger with sweet potato fries and vanilla shake, Order 3: plain burger with standard fries and strawberry shake, Order 4: just fries and a fudge sundae - everything must hit the plating station simultaneously!"

13. "Staggered complexity: Start with 2 simple cheeseburgers, then 30 seconds later add 3 bacon burgers with complex bun requirements, then 30 seconds after that add 4 different fry orders, then desserts, but everything needs to be ready for serving within a 2-minute window!"

## Quantity Overload

14. "Mega order madness: 20 burgers total - 8 bacon cheeseburgers, 6 double cheeseburgers, 4 regular cheeseburgers, 2 plain burgers, plus 12 fry orders covering every type and salt combination, 15 shakes distributed across all flavors, 8 sundaes with every topping variation, half for dine-in, half for takeout!"

15. "Party chaos special: 25 items total but I need them in waves - first wave: 5 bacon cheeseburgers with fries, second wave: 8 regular burgers with different fry types, third wave: 6 shakes and 4 sundaes, final wave: 2 special requests with custom modifications, all timed 3 minutes apart!"

## Special Requests Gone Wild

16. "Customization extreme: I want a burger but the patty needs to be cooked twice, cheese melted and re-melted, bacon added in layers between multiple cheese applications, buns toasted, then re-toasted, fries that are fried, then re-fried with salt applied between frying sessions, and a shake that's blended, then re-blended with additional ingredients."

17. "Modification mayhem: 3 burgers but each one needs to be modified during cooking - start as regular burgers, add cheese halfway through cooking, add bacon three-quarters through, change bun toasting requirements mid-process, plus fries that change salt requirements during cooking, and shakes that get modified after initial preparation."

## Communication Chaos

18. "Confusing instructions: I want some burgers, you know, the good ones, with the stuff on them, and some of those potato things, but not the regular ones, the other ones, and something cold and sweet, but not too sweet, and make sure it's all good for eating, you know what I mean?"

19. "Unclear quantity chaos: I need burgers for everyone, and fries for most people, and some of those dessert things, but not too many, and make sure there's enough but not too much, and some people don't want certain things but I can't remember what, so just make it good!"

## Rush Hour Simulation

20. "Peak hour pandemonium: Simulate rush hour with 7 separate orders arriving simultaneously - Order A: 2 bacon cheeseburgers with waffle fries, Order B: 3 regular burgers with standard fries, Order C: 1 double cheeseburger with sweet potato fries and chocolate shake, Order D: just 2 vanilla shakes and fudge sundae, Order E: 4 different fry orders only, Order F: 3 shakes of different flavors, Order G: 2 bacon cheeseburgers with specific bun toasting and 2 caramel sundaes!"

21. "Concurrent complexity: Handle 5 overlapping orders where each order affects the others - shared ingredients, timing dependencies, station resource conflicts, varying presentation requirements, and different completion deadlines!"

## Equipment Stress Tests

22. "Maximum capacity test: Push every station to its limits - grill handling 8 patties with different cooking stages, fryer managing 4 different fry types simultaneously, dessert station making 6 different items at once, plating station handling 3 different presentation styles, all while maintaining quality standards!"

23. "Resource management chaos: 12 burgers requiring different cheese melting stages, 8 fry orders with varying salt applications, 6 shakes with different whipped cream requirements, 4 sundaes with complex topping combinations, all competing for station time and coordination!"

## Customer Service Nightmares

24. "Demanding customer simulation: Everything must be perfect, hot, fresh, exactly on time, with specific temperatures, precise salt levels, perfect presentation, and if anything is slightly off, the entire order needs to be remade, plus they want updates every 30 seconds on order progress!"

25. "Multiple modification requests: Start with a simple order, then add modifications every 15 seconds - add bacon, change fry type, modify shake flavor, alter bun toasting, change presentation style, add extra items, remove items, change quantities, modify cooking instructions!"

## Holiday Rush Scenarios

26. "Black Friday kitchen chaos: 30 orders in queue, each with 3-5 items, random complexity levels, varying presentation requirements, some orders with rush timing, others with specific delay requests, ingredient shortages requiring substitutions, equipment running at maximum capacity!"

27. "Catering emergency: Large order that keeps changing - started as 10 simple burgers, now it's 25 complex items with special dietary requirements, timing changes, presentation modifications, quantity adjustments, and quality specifications that keep evolving!"

## System Breaking Attempts

28. "Chaos theory test: Order designed to create maximum complexity with minimum items - 1 burger that requires every grill tool, 1 fry order that uses every fryer capability, 1 shake that requires every dessert station function, 1 plating request that demands every presentation style, all coordinated in an impossible timeline!"

29. "Edge case explosion: Push every agent to their limits simultaneously while maintaining coordination - grill agent handling maximum patty count with complex timing, fryer agent managing all fry types with different salt requirements, dessert agent creating multiple complex items, plating agent handling every presentation style, all with interdependent timing requirements!"

30. "Ultimate kitchen stress test: The order that breaks everything - 15 bacon cheeseburgers with individually specified modifications, 12 fry orders covering every possible variation, 10 shakes with custom blending requirements, 8 sundaes with architectural presentation demands, all for a mix of dine-in, takeout, and tray service, required to be completed in coordinated waves while maintaining perfect quality and handling real-time modifications!"
```

### NormalOrders.md
Create `Prompts/NormalOrders.md` with typical everyday orders:

```markdown
# Normal Orders - Typical Daily Operations

These represent the standard, everyday orders that AgentBurgers would typically receive during normal operations.

## Classic Favorites

1. "I'll have a cheeseburger with fries and a Coke, please."

2. "Can I get a bacon cheeseburger with waffle fries and a chocolate shake?"

3. "Just a regular burger and fries for me."

4. "I'd like a double cheeseburger, fries, and a vanilla shake."

5. "Cheeseburger combo with a drink, please."

## Quick Lunch Orders

6. "I'll take a cheeseburger and fries to go."

7. "Can I get a bacon burger with sweet potato fries?"

8. "Just fries and a shake, thanks."

9. "Regular burger, no cheese, with standard fries."

10. "Bacon cheeseburger, hold the fries, add a chocolate shake."

## Family Meals

11. "We need 3 cheeseburgers, 2 orders of fries, and 2 vanilla shakes."

12. "Can we get 2 bacon cheeseburgers, 1 regular burger, 3 fries, and a chocolate shake?"

13. "Family meal: 4 cheeseburgers, 3 waffle fries, 1 sweet potato fries, and 2 shakes."

14. "Two bacon cheeseburgers, two regular fries, and one fudge sundae to share."

## Simple Requests

15. "I just want a vanilla shake."

16. "Can I get an order of waffle fries?"

17. "Just a cheeseburger, that's it."

18. "I'll have the fudge sundae."

19. "Sweet potato fries and a strawberry shake, please."

## Casual Orders

20. "Hey, can I get a burger with cheese and some fries?"

21. "I'll take a bacon cheeseburger and whatever fries you recommend."

22. "Surprise me with a shake and give me a regular burger."

23. "The usual - cheeseburger, fries, and a chocolate shake."

## Light Orders

24. "Just a shake for me, chocolate please."

25. "I only want fries today, waffle fries."

26. "Can I just get a caramel sundae?"

27. "Small order - just a regular burger."

## Standard Combos

28. "Bacon cheeseburger combo with waffle fries and vanilla shake."

29. "Double cheeseburger, standard fries, chocolate shake."

30. "Regular combo - burger, fries, and a drink."

## Weekend Orders

31. "Saturday special - bacon cheeseburger, waffle fries, and a fudge sundae."

32. "Weekend treat: double bacon cheeseburger with sweet potato fries and a strawberry shake."

33. "Relaxed lunch: cheeseburger, fries, and time to enjoy a vanilla shake."

## After School Rush

34. "Quick after-school snack: fries and a chocolate shake."

35. "I'm hungry - bacon cheeseburger and waffle fries."

36. "Student special: regular burger and standard fries."

## Dinner Orders

37. "Dinner for one: double cheeseburger, fries, and a shake."

38. "Evening meal: bacon cheeseburger with sweet potato fries."

39. "Simple dinner: burger, fries, and a vanilla shake."

## Takeout Orders

40. "Everything to go: 2 cheeseburgers, 2 fries, 1 shake."

41. "Takeout order: bacon cheeseburger and waffle fries."

42. "To go: just a chocolate shake and standard fries."

## Dine-in Relaxed

43. "We'll eat here: 2 bacon cheeseburgers, waffle fries to share, and 2 vanilla shakes."

44. "Dine-in order: cheeseburger, fries, and we'll take our time with a fudge sundae."

45. "Eating here: double cheeseburger and sweet potato fries."

## Regular Customer Orders

46. "My usual: bacon cheeseburger, no modifications needed."

47. "The regular: cheeseburger, waffle fries, chocolate shake."

48. "Same as always: double cheese, standard fries, vanilla shake."

## Simple Modifications

49. "Cheeseburger but toast both buns please, with regular fries."

50. "Bacon cheeseburger, extra salt on the fries, and a shake."
```

---

## 13. API Test Scenarios (AgentBurgers.http)

Create `Orchestrator/AgentBurgers.http` with comprehensive test scenarios for the API endpoints:

```http
### AgentBurgers API Test Scenarios
### This file contains comprehensive test scenarios for the AgentBurgers kitchen API
### Use with VS Code REST Client extension or similar HTTP testing tools

# Variables - Update these to match your environment
@baseUrl = https://localhost:7042
@contentType = application/json

### Health Check
GET {{baseUrl}}/health

### Alive Check  
GET {{baseUrl}}/alive

### ===== BASIC ORDERS =====

### Simple Cheeseburger
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "I'd like a cheeseburger, please"
}

### Cheeseburger with Fries
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "Can I get a cheeseburger with fries?"
}

### Basic Combo Meal
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "I want a burger, fries, and a vanilla shake"
}

### ===== COMPLEX ORDERS =====

### Bacon Cheeseburger Deluxe
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "I'd like a bacon cheeseburger with both buns toasted, waffle fries with extra salt, and a chocolate shake with whipped cream"
}

### Double Everything
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "Double cheeseburger with bacon, sweet potato fries, and a fudge sundae with whipped cream"
}

### Gourmet Special
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "I want a burger with melted cheese and bacon, the top bun toasted, standard fries that are salted and bagged, plus a caramel sundae"
}

### ===== DESSERT FOCUSED =====

### Just Desserts
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "I only want dessert - a strawberry shake and a cherry sundae with whipped cream"
}

### Shake Only
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "Just a vanilla shake, please"
}

### Sundae Combo
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "Can I get a chocolate shake and a fudge sundae?"
}

### ===== FRIES VARIATIONS =====

### Fries Sampler
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "I want to try all your fries - standard, waffle, and sweet potato fries, all salted"
}

### Just Fries
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "Just waffle fries, salted and bagged for takeout"
}

### ===== SPECIFIC INSTRUCTIONS =====

### Detailed Burger Order
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "I need a well-done beef patty with American cheese melted on top, bacon strips, and I want only the bottom bun toasted. Serve it for dine-in presentation."
}

### Precise Fry Order
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "Standard fries, lightly salted, bagged for takeout"
}

### Custom Shake
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "Chocolate shake with extra whipped cream on top"
}

### ===== LARGE ORDERS =====

### Family Feast
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "We need 3 cheeseburgers with bacon, 2 orders of waffle fries, 1 order of sweet potato fries, 2 vanilla shakes, 1 chocolate shake, and 1 fudge sundae"
}

### Party Order
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "Large order: 4 double cheeseburgers, 3 regular fries, 2 waffle fries, 3 chocolate shakes, 2 vanilla shakes, 1 fudge sundae, 1 caramel sundae, all for tray service"
}

### ===== EDGE CASES =====

### Minimal Order
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "burger"
}

### Casual Language
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "Hey, can I get like a burger and some fries? Oh and throw in a shake too, chocolate if you have it"
}

### Empty Order (Should handle gracefully)
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": ""
}

### Invalid JSON (Missing quotes)
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  order: "This should fail due to invalid JSON"
}

### Very Long Order
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "I would like to order a delicious cheeseburger with perfectly melted American cheese, crispy bacon strips, a well-done beef patty, both buns toasted to golden perfection, accompanied by crispy waffle fries that are perfectly salted, and for dessert I'd love a rich chocolate shake with extra whipped cream on top, and also a decadent fudge sundae with additional whipped cream, all prepared for dine-in presentation with the highest quality and attention to detail"
}

### ===== AMBIGUOUS ORDERS (Test AI Understanding) =====

### Ambiguous Request
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "I want something good with cheese and maybe something crispy on the side"
}

### Vague Dessert Request
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "Something sweet and cold please"
}

### Mixed Request
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "The usual, but make it special today"
}

### ===== STRESS TEST ORDERS =====

### Kitchen Chaos 1
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "Rush order! 5 bacon cheeseburgers, 3 with only top bun toasted, 2 with both buns toasted, 4 orders of waffle fries, 3 orders of standard fries, 2 sweet potato fries, all salted, 3 chocolate shakes, 2 vanilla shakes, 1 strawberry shake, 2 fudge sundaes, 1 caramel sundae, everything for bag service ASAP!"
}

### Kitchen Chaos 2
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "Complex order: I need 2 double cheeseburgers with bacon and only bottom buns toasted, 1 regular burger with cheese and top bun only toasted, 2 waffle fries with light salt, 1 sweet potato fries with extra salt, 1 standard fries bagged separately, 2 chocolate shakes with whipped cream, 1 vanilla shake without whipped cream, 1 fudge sundae, 1 cherry sundae with extra whipped cream, half for dine-in, half for takeout"
}

### Multi-Station Coordination
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "I need everything coordinated: start with 3 beef patties cooking with cheese melting, toast 6 bun halves (3 top, 3 bottom), fry 2 batches of waffle fries and 1 batch of sweet potato fries, make 2 chocolate shakes and 1 vanilla shake with whipped cream, prepare 1 fudge sundae, add bacon to 2 burgers, salt all fries, bag 1 fries order, plate everything for dine-in except 1 burger combo for bag service"
}

### ===== CONCURRENT TESTING =====
### Run these simultaneously to test concurrent processing

### Concurrent Order 1
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "Concurrent test 1: Bacon cheeseburger with waffle fries and chocolate shake"
}

### Concurrent Order 2  
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "Concurrent test 2: Double burger with sweet potato fries and vanilla shake"
}

### Concurrent Order 3
POST {{baseUrl}}/api/order
Content-Type: {{contentType}}

{
  "order": "Concurrent test 3: Just fries and a fudge sundae"
}
```

---

## End

You now have a fully operational AI-driven kitchen simulation built with .NET Aspire, featuring both API and web UI interfaces. Key features include:

### Orchestrator Interfaces:
- **Minimal API**: RESTful endpoints for programmatic order processing
- **Blazor Server UI**: Interactive web interface with real-time updates
- **Flexible Integration**: Can be consumed by external applications or used directly by customers

### Aspire Benefits:
- **Service Discovery**: Automatic discovery of agent services using logical names instead of hardcoded URLs
- **Observability**: Built-in OpenTelemetry tracing, metrics, and logging across all services
- **Resilience**: HTTP client resilience patterns with retry and circuit breaker policies
- **Health Checks**: Integrated health monitoring for all services
- **Dashboard**: Aspire dashboard for monitoring service health, logs, and metrics
- **Development Experience**: Simplified local development with automatic service orchestration

### Architecture:
- **AppHost**: Orchestrates all services and manages service discovery
- **ServiceDefaults**: Provides common configuration for observability, health checks, and resilience
- **MCP Agents**: Each kitchen station exposes deterministic tools and follows strict instructions
- **Orchestrator**: Microservice with web UI and API endpoints that uses LLM reasoning to route tasks and leverages service discovery to find agents
- **OrderSimulator**: Dedicated service that continuously generates realistic order loads for testing and demonstration

### Running the Application:
1. Set the `AgentBurgers.AppHost` as the startup project
2. Configure Azure OpenAI environment variables:
   - `AZURE_OPENAI_ENDPOINT`
   - `AZURE_OPENAI_DEPLOYMENT`
3. Ensure Azure authentication is configured (see Authentication Setup below)
4. Run the application - Aspire will automatically start all services including the OrderSimulator
5. Access the Aspire dashboard to monitor all services
6. Navigate to the Orchestrator web UI for the kitchen interface
7. Use the API endpoints for programmatic integration
8. **Automatic simulation**: The OrderSimulator service will start generating random orders every second for continuous testing

### Authentication Setup:
The application uses Azure DefaultAzureCredential for authentication, which supports multiple authentication methods in order:
1. **Environment Variables**: `AZURE_TENANT_ID`, `AZURE_CLIENT_ID`, `AZURE_CLIENT_SECRET`
2. **Managed Identity**: When deployed to Azure (App Service, Container Apps, etc.)
3. **Azure CLI**: `az login` for local development
4. **Visual Studio**: Signed-in Azure account
5. **Visual Studio Code**: Azure Account extension
6. **PowerShell**: `Connect-AzAccount`

**Benefits of Azure Credential Authentication:**
- **More Secure**: No API keys to manage or accidentally commit to source control
- **Automatic Token Refresh**: Handles token expiration automatically
- **Production Ready**: Uses Managed Identity when deployed to Azure
- **Developer Friendly**: Uses existing Azure CLI or IDE authentication for local development
- **Auditable**: Better tracking of authentication events in Azure AD logs

### Local Development Setup:
```bash
# Option 1: Use Azure CLI (Recommended for development)
az login

# Option 2: Set environment variables for service principal
$env:AZURE_TENANT_ID="your-tenant-id"
$env:AZURE_CLIENT_ID="your-client-id"  
$env:AZURE_CLIENT_SECRET="your-client-secret"

# Set Azure OpenAI configuration
$env:AZURE_OPENAI_ENDPOINT="https://your-instance.openai.azure.com/"
$env:AZURE_OPENAI_DEPLOYMENT="gpt-4"
```

### Azure Deployment:
When deployed to Azure services like App Service or Container Apps, the application will automatically use Managed Identity:
1. Enable System-assigned Managed Identity on your Azure service
2. Grant the Managed Identity "Cognitive Services OpenAI User" role on your Azure OpenAI resource
3. No additional configuration needed - DefaultAzureCredential handles the rest

### Order Simulation Features:
- **Independent Service**: OrderSimulator runs as its own microservice node
- **Realistic Load**: Continuous order flow simulating real restaurant operations
- **Mixed Complexity**: 80% normal orders, 20% chaos orders for balanced testing
- **Live Monitoring**: Watch orders being processed in real-time through logs and UI
- **Stress Testing**: Chaos orders push the system to its limits
- **Performance Validation**: Continuous operation validates system stability and agent coordination
- **Service Discovery**: Automatic discovery and health checking of the Orchestrator service

Never let agents reason. That is the orchestrator's job. The agents are purely functional and follow their instructions exactly. The orchestrator now provides a proper microservice interface suitable for production deployment within an Aspire topology.

---

## 13. Implementation Summary

### Event-Driven Real-Time Updates

**CRITICAL: This implementation uses event-driven service architecture for real-time UI updates instead of SignalR (which is incompatible with Blazor Server).**

The KitchenService exposes the following events that Blazor components can subscribe to:

```csharp
public event EventHandler<OrderHistoryItem>? OrderStarted;
public event EventHandler<OrderHistoryItem>? OrderCompleted; 
public event EventHandler<OrderHistoryItem>? OrderFailed;
public event EventHandler<(string OrderId, string Message)>? OrderProgressUpdated;
```

**Blazor Components** subscribe to these events in their `OnInitializedAsync()` method:

```csharp
Kitchen.OrderStarted += OnOrderStarted;
Kitchen.OrderCompleted += OnOrderCompleted;
Kitchen.OrderFailed += OnOrderFailed;
Kitchen.OrderProgressUpdated += OnOrderProgressUpdated;
```

**Event Handlers** call `InvokeAsync()` and `StateHasChanged()` to update the UI reactively:

```csharp
private void OnOrderStarted(object? sender, OrderHistoryItem order)
{
    InvokeAsync(() =>
    {
        orderHistory.Insert(0, order);
        StateHasChanged();
    });
}
```

**Components implement `IDisposable`** to unsubscribe from events and prevent memory leaks:

```csharp
public void Dispose()
{
    Kitchen.OrderStarted -= OnOrderStarted;
    Kitchen.OrderCompleted -= OnOrderCompleted;
    Kitchen.OrderFailed -= OnOrderFailed;
    Kitchen.OrderProgressUpdated -= OnOrderProgressUpdated;
}
```

This approach provides:
- ✅ Real-time UI updates without SignalR
- ✅ Perfect compatibility with Blazor Server
- ✅ Clean separation of concerns
- ✅ Memory leak prevention through proper disposal
- ✅ Reactive, event-driven architecture

### User Experience:

- **Zero-Latency Updates**: Orders appear within milliseconds of submission
- **Live Status Indicators**: Visual feedback showing order progression  
- **Event-Driven Architecture**: Service events automatically update UI
- **Clean Component Lifecycle**: Proper subscription/unsubscription management

This ensures that as the OrderSimulator continuously generates orders, users watching the Orders page see immediate, real-time updates without any manual intervention required.

