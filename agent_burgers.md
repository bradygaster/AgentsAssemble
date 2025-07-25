# AgentBurgers.prompt.md

This markdown document describes an end-to-end implementation plan for a distributed AI-powered restaurant simulation using C#, .NET Aspire, Azure OpenAI (or Azure AI Foundry), and Model Context Protocol (MCP). Each kitchen station is hosted as an individual MCP server exposing tools. The orchestrator interprets user prompts (customer orders) and delegates tasks to the correct agents using Aspire's service discovery.

Each agent must strictly follow its `Instructions.md` file. No hallucination or deviation is allowed.

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
│   ├── Orchestrator.csproj
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
    └── TicketChaos.md
```

---

## 2. Aspire AppHost

### Program.cs
```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add service defaults
var serviceDefaults = builder.AddProject<Projects.AgentBurgers_ServiceDefaults>("servicedefaults");

// Add MCP Agent services
var grillAgent = builder.AddProject<Projects.GrillAgent>("grillagent")
    .WithReference(serviceDefaults);

var fryerAgent = builder.AddProject<Projects.FryerAgent>("fryeragent")
    .WithReference(serviceDefaults);

var dessertAgent = builder.AddProject<Projects.DessertAgent>("dessertagent")
    .WithReference(serviceDefaults);

var platingAgent = builder.AddProject<Projects.PlatingAgent>("platingagent")
    .WithReference(serviceDefaults);

// Add Orchestrator service with references to all agents
var orchestrator = builder.AddProject<Projects.Orchestrator>("orchestrator")
    .WithReference(serviceDefaults)
    .WithReference(grillAgent)
    .WithReference(fryerAgent)
    .WithReference(dessertAgent)
    .WithReference(platingAgent);

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
    <ProjectReference Include="..\AgentBurgers.ServiceDefaults\AgentBurgers.ServiceDefaults.csproj" />
    <ProjectReference Include="..\Orchestrator\Orchestrator.csproj" />
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

## 3. Aspire ServiceDefaults

### Extensions.cs
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

## 4. Agent: GrillAgent

### Program.cs
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

### GrillAgent.csproj
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

## 5. Agent: FryerAgent

### Program.cs
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

### FryerAgent.csproj
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

## 6. Agent: DessertAgent

### Program.cs
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

### DessertAgent.csproj
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

## 7. Agent: PlatingAgent

### Program.cs
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

### PlatingAgent.csproj
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

## 8. Orchestrator

### Program.cs
```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.AI.OpenAI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenAIChatCompletion("default", options =>
{
    options.Endpoint = new Uri("https://your-azure-openai-endpoint");
    options.DeploymentName = "gpt-4";
    options.ApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY")!;
});

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

var app = builder.Build();

app.MapDefaultEndpoints();

var chat = app.Services.GetRequiredService<IChatCompletionService>();
var httpClientFactory = app.Services.GetRequiredService<IHttpClientFactory>();

var allTools = new List<AIFunction>();
var agentNames = new[] { "grillagent", "fryeragent", "dessertagent", "platingagent" };

foreach (var agentName in agentNames)
{
    var httpClient = httpClientFactory.CreateClient(agentName);
    var mcpClient = new McpToolClient(httpClient.BaseAddress!);
    var tools = await mcpClient.ListToolsAsync();
    allTools.AddRange(tools);
}

var systemPrompt = $"""
You are a kitchen expediter. You interpret customer orders and use only the tools provided in the instructions below. Never invent tools.

{LoadAllAgentInstructions()}
""";

var history = new List<ChatMessage> { ChatMessage.CreateSystemMessage(systemPrompt) };

Console.WriteLine("Kitchen is open. Type a customer order or 'exit' to quit.");
while (true)
{
    Console.Write("\nOrder: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "exit") break;

    history.Add(ChatMessage.CreateUserMessage(input));
    var result = await chat.GetResponseAsync(history, new ChatRequestOptions { Tools = allTools });
    Console.WriteLine("\nResponse:\n" + result.Content);
    history.Add(result);
}

app.Run();

static string LoadAllAgentInstructions()
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

## 9. Prompts

Create `Prompts/TicketChaos.md` with 30 creative and chaotic food orders to stress test orchestration.

---

## End

You now have a fully operational AI-driven kitchen simulation built with .NET Aspire. Key features include:

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
- **Orchestrator**: Uses LLM reasoning to route tasks and leverages service discovery to find agents

### Running the Application:
1. Set the `AgentBurgers.AppHost` as the startup project
2. Run the application - Aspire will automatically start all services
3. Access the Aspire dashboard to monitor services
4. The orchestrator console will be available for customer orders

Never let agents reason. That is the orchestrator's job. The agents are purely functional and follow their instructions exactly.

