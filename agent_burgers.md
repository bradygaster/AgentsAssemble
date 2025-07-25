# AgentBurgers.prompt.md

This markdown document describes an end-to-end implementation plan for a distributed AI-powered restaurant simulation using C#, Azure OpenAI (or Azure AI Foundry), and Model Context Protocol (MCP). Each kitchen station is hosted as an individual MCP server exposing tools. The orchestrator interprets user prompts (customer orders) and delegates tasks to the correct agents.

Each agent must strictly follow its `Instructions.md` file. No hallucination or deviation is allowed.

---

## 1. Folder Structure

```
AgentBurgers/
├── Orchestrator/
│   ├── Program.cs
│   └── Orchestrator.csproj
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

## 2. Agent: GrillAgent

### Program.cs
```csharp
using ModelContextProtocol;
using ModelContextProtocol.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<GrillTools>();

builder.Services.AddOpenTelemetry()
    .WithTracing(b => b.AddSource("*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithMetrics(b => b.AddMeter("*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation());

var app = builder.Build();

app.MapMcp();

app.Run("http://localhost:5201");

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
    <UseGlobalApplication>true</UseGlobalApplication>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.7" />
    <PackageReference Include="ModelContextProtocol" Version="0.3.0-preview.3" />
    <PackageReference Include="ModelContextProtocol.AspNetCore" Version="0.3.0-preview.3" />
    <PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.12.0" />
    <PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.12.0" />
    <PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.12.0" />
    <PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.12.0" />
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
      "applicationUrl": "http://localhost:5201",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "https://localhost:7201;http://localhost:5201",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## 3. Agent: FryerAgent

### Program.cs
```csharp
using ModelContextProtocol;
using ModelContextProtocol.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<FryerTools>();

builder.Services.AddOpenTelemetry()
    .WithTracing(b => b.AddSource("*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithMetrics(b => b.AddMeter("*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation());

var app = builder.Build();

app.MapMcp();

app.Run("http://localhost:5202");

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
    <UseGlobalApplication>true</UseGlobalApplication>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.7" />
    <PackageReference Include="ModelContextProtocol" Version="0.3.0-preview.3" />
    <PackageReference Include="ModelContextProtocol.AspNetCore" Version="0.3.0-preview.3" />
    <PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.12.0" />
    <PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.12.0" />
    <PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.12.0" />
    <PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.12.0" />
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
      "applicationUrl": "http://localhost:5202",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "https://localhost:7202;http://localhost:5202",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## 4. Agent: DessertAgent

### Program.cs
```csharp
using ModelContextProtocol;
using ModelContextProtocol.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<DessertTools>();

builder.Services.AddOpenTelemetry()
    .WithTracing(b => b.AddSource("*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithMetrics(b => b.AddMeter("*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation());

var app = builder.Build();

app.MapMcp();

app.Run("http://localhost:5203");

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
    <UseGlobalApplication>true</UseGlobalApplication>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.7" />
    <PackageReference Include="ModelContextProtocol" Version="0.3.0-preview.3" />
    <PackageReference Include="ModelContextProtocol.AspNetCore" Version="0.3.0-preview.3" />
    <PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.12.0" />
    <PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.12.0" />
    <PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.12.0" />
    <PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.12.0" />
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
      "applicationUrl": "http://localhost:5203",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "https://localhost:7203;http://localhost:5203",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## 5. Agent: PlatingAgent

### Program.cs
```csharp
using ModelContextProtocol;
using ModelContextProtocol.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithTools<PlatingTools>();

builder.Services.AddOpenTelemetry()
    .WithTracing(b => b.AddSource("*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation())
    .WithMetrics(b => b.AddMeter("*")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation());

var app = builder.Build();

app.MapMcp();

app.Run("http://localhost:5204");

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
    <UseGlobalApplication>true</UseGlobalApplication>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="9.0.7" />
    <PackageReference Include="ModelContextProtocol" Version="0.3.0-preview.3" />
    <PackageReference Include="ModelContextProtocol.AspNetCore" Version="0.3.0-preview.3" />
    <PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.12.0" />
    <PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.12.0" />
    <PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.12.0" />
    <PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.12.0" />
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
      "applicationUrl": "http://localhost:5204",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "https://localhost:7204;http://localhost:5204",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## 6. Orchestrator

### Program.cs
```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.AI.OpenAI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.Text;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOpenAIChatCompletion("default", options =>
{
    options.Endpoint = new Uri("https://your-azure-openai-endpoint");
    options.DeploymentName = "gpt-4";
    options.ApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY")!;
});

var host = builder.Build();
var chat = host.Services.GetRequiredService<IChatCompletionService>();

var allTools = new List<AIFunction>();
var toolSources = new[]
{
    "http://localhost:5201",
    "http://localhost:5202",
    "http://localhost:5203",
    "http://localhost:5204"
};

foreach (var url in toolSources)
{
    var tools = await new McpToolClient(new Uri(url)).ListToolsAsync();
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
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.AI" Version="9.0.0-preview.3.24174.3" />
    <PackageReference Include="Microsoft.Extensions.AI.OpenAI" Version="9.0.0-preview.3.24174.3" />
    <PackageReference Include="ModelContextProtocol.Client" Version="0.1.0-preview.8" />
  </ItemGroup>
</Project>
```

---

## 7. Prompts

Create `Prompts/TicketChaos.md` with 30 creative and chaotic food orders to stress test orchestration.

---

## End

You now have a fully operational AI-driven kitchen simulation. Each agent exposes deterministic tools and is governed by its instructions. The orchestrator uses LLM reasoning to route tasks appropriately. Never let agents reason. That is the orchestrator's job.

