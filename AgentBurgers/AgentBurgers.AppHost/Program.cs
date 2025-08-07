var builder = DistributedApplication.CreateBuilder(args);

// Add MCP Agent services
var grillAgent = builder.AddProject<Projects.GrillAgent>("grillagent");

var fryerAgent = builder.AddProject<Projects.FryerAgent>("fryeragent");

var dessertAgent = builder.AddProject<Projects.DessertAgent>("dessertagent");

var platingAgent = builder.AddProject<Projects.PlatingAgent>("platingagent");

// Add Orchestrator service with references to all agents
var orchestrator = builder.AddProject<Projects.Orchestrator>("orchestrator")
    .WaitFor(grillAgent)
    .WaitFor(fryerAgent)
    .WaitFor(dessertAgent)
    .WaitFor(platingAgent)
    .WithReference(grillAgent)
    .WithReference(fryerAgent)
    .WithReference(dessertAgent)
    .WithReference(platingAgent);

// Add Order Simulator service that calls the orchestrator
var orderSimulator = builder.AddProject<Projects.OrderSimulator>("ordersimulator")
    .WaitFor(orchestrator)
    .WithReference(orchestrator);

builder.Build().Run();
