var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.AgentBurgers_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder.Build().Run();
