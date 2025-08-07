using ModelContextProtocol.Client;
using Orchestrator.Models;
using System.Text;

namespace Orchestrator;

public class McpOrderProcessor : IOrderProcessor
{
    private readonly IMcpClient _grillAgent;
    private readonly IMcpClient _fryerAgent;
    private readonly IMcpClient _dessertAgent;
    private readonly IMcpClient _platingAgent;
    private readonly ILogger<McpOrderProcessor> _logger;

    public McpOrderProcessor(
        [FromKeyedServices("grillagent")] IMcpClient grillAgent,
        [FromKeyedServices("fryeragent")] IMcpClient fryerAgent,
        [FromKeyedServices("dessertagent")] IMcpClient dessertAgent,
        [FromKeyedServices("platingagent")] IMcpClient platingAgent,
        ILogger<McpOrderProcessor> logger)
    {
        _grillAgent = grillAgent;
        _fryerAgent = fryerAgent;
        _dessertAgent = dessertAgent;
        _platingAgent = platingAgent;
        _logger = logger;
    }

    public async Task<string> ProcessOrderAsync(string orderText)
    {
        var availableTools = await GetAllAvailableToolsAsync(CancellationToken.None);
        var sb = new StringBuilder();

        foreach (var tool in availableTools)
        {
            sb.AppendLine($"Available tool: {tool.Name} - {tool.Description}");
        }

        _logger.LogInformation("Available tools: {Tools}", sb.ToString());

        return sb.ToString();
    }

    private async Task<List<ToolDefinition>> GetAllAvailableToolsAsync(CancellationToken cancellationToken)
    {
        var allTools = new List<ToolDefinition>();
        var agents = new Dictionary<string, IMcpClient>
        {
            ["grill"] = _grillAgent,
            ["fryer"] = _fryerAgent,
            ["dessert"] = _dessertAgent,
            ["plating"] = _platingAgent
        };

        foreach (var (stationName, client) in agents)
        {
            try
            {
                var toolsResult = await client.ListToolsAsync();
                foreach (var tool in toolsResult)
                {
                    allTools.Add(new ToolDefinition 
                    { 
                        Name = tool.Name,
                        Description = tool.Description,
                        Station = stationName,
                        Parameters = null // Will fix property name later
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to discover tools from {Station}", stationName);
            }
        }

        return allTools;
    }
}
