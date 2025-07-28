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
        _logger.LogInformation("🚀 OrderSimulator starting up...");

        // Wait for orchestrator to be ready
        await WaitForOrchestratorAsync(stoppingToken);

        _logger.LogInformation("🎯 OrderSimulator ready! Starting order generation...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 80% normal orders, 20% chaos orders
                var useNormal = _random.NextDouble() < 0.8;
                var orders = useNormal ? _normalOrders : _chaosOrders;
                var orderType = useNormal ? "NORMAL" : "CHAOS";

                if (orders.Count > 0)
                {
                    var order = orders[_random.Next(orders.Count)];
                    await SubmitOrderAsync(order, orderType);
                }

                // Wait 1 second between orders
                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "❌ Error in OrderSimulator");
                await Task.Delay(5000, stoppingToken); // Wait longer on errors
            }
        }
    }

    private async Task WaitForOrchestratorAsync(CancellationToken stoppingToken)
    {
        var client = _httpClientFactory.CreateClient("orchestrator");
        var maxRetries = 60; // 5 minutes total
        var retryCount = 0;

        while (retryCount < maxRetries && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = await client.GetAsync("/health", stoppingToken);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Orchestrator is ready!");
                    return;
                }
            }
            catch
            {
                // Continue retrying
            }

            retryCount++;
            _logger.LogInformation($"⏳ Waiting for Orchestrator... (attempt {retryCount}/{maxRetries})");
            await Task.Delay(5000, stoppingToken);
        }

        _logger.LogWarning("⚠️ Could not connect to Orchestrator, but continuing anyway...");
    }

    private async Task SubmitOrderAsync(string order, string orderType)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("orchestrator");
            var json = JsonSerializer.Serialize(new { order });
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/order", content);

            if (response.IsSuccessStatusCode)
            {
                var emoji = orderType == "CHAOS" ? "🔥" : "🍔";
                _logger.LogInformation("{Emoji} [{OrderType}] Order submitted: {Order}", emoji, orderType, order.Length > 100 ? order[..100] + "..." : order);
            }
            else
            {
                _logger.LogWarning("⚠️ Order submission failed: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to submit order");
        }
    }

    private static List<string> LoadChaosOrders()
    {
        // Fallback orders if file doesn't exist
        var fallbackOrders = new List<string>
        {
            "URGENT! I need 6 bacon cheeseburgers, 3 with only top buns toasted, 2 with both buns toasted, 1 with no bun toasting, 4 waffle fries, 3 standard fries, 2 sweet potato fries, all salted differently - light salt on waffle, heavy salt on standard, no salt on sweet potato, 3 chocolate shakes, 2 vanilla shakes, 1 strawberry shake, and 2 fudge sundaes with extra whipped cream, everything for takeout bags, and I need it in 5 minutes!",
            "Kitchen nightmare order: 8 different burgers - 2 plain patties no cheese, 3 single cheese, 2 double cheese with bacon, 1 triple cheese with double bacon, toast every single bun differently, fry 5 batches of different fries, make every shake flavor you have, create 3 different sundaes, and I want half for dine-in, quarter for tray service, quarter bagged, and coordinate it so everything comes out at exactly the same time!",
            "Every station working: Start 5 patties cooking immediately, begin melting cheese on 4 of them, get bacon going for 3 burgers, toast 8 bun halves in different combinations, start 3 different fry batches at staggered times, begin 4 shakes simultaneously, prep 2 sundaes, add whipped cream to everything possible, coordinate so grill finishes before fryer, dessert station stays busy throughout, and plating happens in waves!"
        };

        try
        {
            var chaosFile = Path.Combine("Prompts", "TicketChaos.md");
            if (File.Exists(chaosFile))
            {
                var content = File.ReadAllText(chaosFile);
                var orders = ExtractOrdersFromMarkdown(content);
                return orders.Count > 0 ? orders : fallbackOrders;
            }
        }
        catch (Exception)
        {
            // Use fallback on any error
        }

        return fallbackOrders;
    }

    private static List<string> LoadNormalOrders()
    {
        // Fallback orders if file doesn't exist
        var fallbackOrders = new List<string>
        {
            "I'll have a cheeseburger with fries and a Coke, please.",
            "Can I get a bacon cheeseburger with waffle fries and a chocolate shake?",
            "Just a regular burger and fries for me.",
            "I'd like a double cheeseburger, fries, and a vanilla shake.",
            "Cheeseburger combo with a drink, please.",
            "We need 3 cheeseburgers, 2 orders of fries, and 2 vanilla shakes.",
            "I just want a vanilla shake.",
            "Can I get an order of waffle fries?",
            "Hey, can I get a burger with cheese and some fries?",
            "Bacon cheeseburger combo with waffle fries and vanilla shake."
        };

        try
        {
            var normalFile = Path.Combine("Prompts", "NormalOrders.md");
            if (File.Exists(normalFile))
            {
                var content = File.ReadAllText(normalFile);
                var orders = ExtractOrdersFromMarkdown(content);
                return orders.Count > 0 ? orders : fallbackOrders;
            }
        }
        catch (Exception)
        {
            // Use fallback on any error
        }

        return fallbackOrders;
    }

    private static List<string> ExtractOrdersFromMarkdown(string content)
    {
        var orders = new List<string>();
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            // Look for numbered orders (e.g., "1. ", "15. ") followed by quoted text
            if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^\d+\.\s*"".*"""))
            {
                var startQuote = trimmed.IndexOf('"');
                var endQuote = trimmed.LastIndexOf('"');
                if (startQuote >= 0 && endQuote > startQuote)
                {
                    var order = trimmed.Substring(startQuote + 1, endQuote - startQuote - 1);
                    if (!string.IsNullOrWhiteSpace(order))
                    {
                        orders.Add(order);
                    }
                }
            }
        }

        return orders;
    }
}
