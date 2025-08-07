var builder = DistributedApplication.CreateBuilder(args);

// Add Azure AI Foundry services
var localFoundry = builder.AddAzureAIFoundry("foundry")
                          .RunAsFoundryLocal()
                          .AddDeployment("chat", "phi-4", "1", "Microsoft");

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
