using ModelContextProtocol.Client;

public static class Extensions
{
    public static IServiceCollection AddMcpClient(this IServiceCollection services, string serviceName, string clientName)
    {
        services.AddKeyedTransient<IMcpClient>(serviceName, (sp, key) =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            McpClientOptions mcpClientOptions = new()
            {
                ClientInfo = new (){
                    Name = clientName,
                    Version = "1.0.0"
                }
            };

            var name = $"services__{serviceName}__http__0";
            var url = Environment.GetEnvironmentVariable(name) + "/sse";

            var clientTransport = new SseClientTransport(new (){
                Name = clientName,
                Endpoint = new Uri(url)
            },loggerFactory);

            // Not ideal pattern but should be enough to get it working.
            var mcpClient = McpClientFactory.CreateAsync(clientTransport, mcpClientOptions, loggerFactory).GetAwaiter().GetResult();

            return mcpClient;
        });

        return services;        
    }

    public static IServiceCollection AddMcpClients(this IServiceCollection services)
    {
        // Add MCP clients for each agent
        services.AddMcpClient("grillagent", "GrillAgentClient");
        services.AddMcpClient("fryeragent", "FryerAgentClient");
        services.AddMcpClient("dessertagent", "DessertAgentClient");
        services.AddMcpClient("platingagent", "PlatingAgentClient");

        return services;
    }
}