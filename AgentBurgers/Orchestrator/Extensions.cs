using ModelContextProtocol.Client;

public static class Extensions
{
    public static IServiceCollection AddMcpClient(this IServiceCollection services)
    {
        services.AddTransient<IMcpClient>(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            McpClientOptions mcpClientOptions = new()
            {
                ClientInfo = new (){
                    Name = "AspNetCoreSseClient",
                    Version = "1.0.0"
                }
            };

            var serviceName = "mcpserver";
            var name = $"services__{serviceName}__http__0";
            var url = Environment.GetEnvironmentVariable(name) + "/sse";

            var clientTransport = new SseClientTransport(new (){
                Name = "AspNetCoreSse",
                Endpoint = new Uri(url)
            },loggerFactory);

            // Not ideal pattern but should be enough to get it working.
            var mcpClient = McpClientFactory.CreateAsync(clientTransport, mcpClientOptions, loggerFactory).GetAwaiter().GetResult();

            return mcpClient;
        });

        return services;        
    }
}