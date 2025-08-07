using System.Collections.Concurrent;
using System.Text;

namespace Orchestrator;

/// <summary>
/// Manages kitchen operations and order processing
/// </summary>
public class KitchenManager
{
  private readonly ConcurrentDictionary<string, List<string>> _orderProgress = new();
  private readonly ConcurrentDictionary<string, OrderHistoryItem> _orderHistory = new();

  public event EventHandler<OrderHistoryItem>? OrderStarted;
  public event EventHandler<OrderHistoryItem>? OrderCompleted;
  public event EventHandler<OrderHistoryItem>? OrderFailed;
  public event EventHandler<(string OrderId, string Message)>? OrderProgressUpdated;

  public KitchenManager()
  {
  }

  public async Task<string> ProcessOrderAsync(string order)
  {
    var orderId = Guid.NewGuid().ToString();
    var orderItem = new OrderHistoryItem
    {
      Id = orderId,
      Timestamp = DateTime.Now,
      OrderText = order,
      Status = OrderStatus.InProgress
    };

    _orderHistory[orderId] = orderItem;
    _orderProgress[orderId] = new List<string>();

    OrderStarted?.Invoke(this, orderItem);

    try
    {
      await Task.Delay(1000); // Simulate processing time
      
      var response = GenerateMockResponse(order);

      // Update order as completed
      orderItem.Status = OrderStatus.Completed;
      orderItem.CompletedAt = DateTime.Now;
      orderItem.Response = response;

      OrderCompleted?.Invoke(this, orderItem);

      return response;
    }
    catch (Exception ex)
    {
      orderItem.Status = OrderStatus.Failed;
      orderItem.Response = $"Error processing order: {ex.Message}";
      
      OrderFailed?.Invoke(this, orderItem);
      
      return orderItem.Response;
    }
  }

  public async IAsyncEnumerable<string> ProcessOrderStreamAsync(string order)
  {
    var orderId = Guid.NewGuid().ToString();
    _orderProgress[orderId] = new List<string>();

    yield return "Order received, processing...";

    var words = GenerateMockResponse(order).Split(' ');
    
    foreach (var word in words)
    {
      await Task.Delay(50);
      _orderProgress[orderId].Add(word + " ");
      OrderProgressUpdated?.Invoke(this, (orderId, word + " "));
      yield return word + " ";
    }

    yield return "Order completed!";
  }

  public IEnumerable<string> GetOrderProgressStream(string orderId)
  {
    return _orderProgress.TryGetValue(orderId, out var progress) ? progress : [];
  }

  public IEnumerable<OrderHistoryItem> GetOrderHistory()
  {
    return _orderHistory.Values.OrderByDescending(o => o.Timestamp);
  }

  private static string GenerateMockResponse(string order)
  {
    var sb = new StringBuilder();
    sb.AppendLine("🍔 Order received! Here's what I'm preparing:");
    sb.AppendLine();
    
    if (order.ToLower().Contains("burger") || order.ToLower().Contains("cheese"))
    {
      sb.AppendLine("🔥 GRILL STATION:");
      sb.AppendLine("- Cooking beef patty to perfection");
      if (order.ToLower().Contains("cheese")) sb.AppendLine("- Melting American cheese");
      if (order.ToLower().Contains("bacon")) sb.AppendLine("- Adding crispy bacon strips");
      if (order.ToLower().Contains("toast")) sb.AppendLine("- Toasting buns golden brown");
      sb.AppendLine();
    }
    
    if (order.ToLower().Contains("fries"))
    {
      sb.AppendLine("🍟 FRYER STATION:");
      if (order.ToLower().Contains("waffle")) sb.AppendLine("- Frying crispy waffle fries");
      else if (order.ToLower().Contains("sweet")) sb.AppendLine("- Frying sweet potato fries");
      else sb.AppendLine("- Frying golden standard fries");
      sb.AppendLine("- Adding perfect amount of salt");
      sb.AppendLine();
    }
    
    if (order.ToLower().Contains("shake") || order.ToLower().Contains("sundae"))
    {
      sb.AppendLine("🍦 DESSERT STATION:");
      if (order.ToLower().Contains("chocolate")) sb.AppendLine("- Blending rich chocolate shake");
      if (order.ToLower().Contains("vanilla")) sb.AppendLine("- Blending creamy vanilla shake");
      if (order.ToLower().Contains("strawberry")) sb.AppendLine("- Blending sweet strawberry shake");
      if (order.ToLower().Contains("sundae")) sb.AppendLine("- Creating delicious sundae");
      if (order.ToLower().Contains("whipped")) sb.AppendLine("- Adding fresh whipped cream");
      sb.AppendLine();
    }
    
    sb.AppendLine("🍽️ PLATING STATION:");
    sb.AppendLine("- Final assembly and presentation");
    if (order.ToLower().Contains("takeout") || order.ToLower().Contains("bag")) 
      sb.AppendLine("- Packing for takeout");
    else 
      sb.AppendLine("- Plating for dine-in service");
    
    sb.AppendLine();
    sb.AppendLine("✅ Order completed! Enjoy your meal!");
    
    return sb.ToString();
  }
}
