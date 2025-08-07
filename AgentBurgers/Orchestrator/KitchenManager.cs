using System.Collections.Concurrent;

namespace Orchestrator;

public class KitchenManager
{
  private readonly ConcurrentDictionary<string, List<string>> _orderProgress = new();
  private readonly ConcurrentDictionary<string, OrderHistoryItem> _orderHistory = new();
  private readonly IOrderProcessor _orderProcessor;
  public event EventHandler<OrderHistoryItem>? OrderStarted;
  public event EventHandler<OrderHistoryItem>? OrderCompleted;
  public event EventHandler<OrderHistoryItem>? OrderFailed;
  public event EventHandler<(string OrderId, string Message)>? OrderProgressUpdated;

  public KitchenManager(IOrderProcessor orderProcessor)
  {
    _orderProcessor = orderProcessor;
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
      var response = await _orderProcessor.ProcessOrderAsync(order);

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

    var response = await _orderProcessor.ProcessOrderAsync(order);
    var words = response.Split(' ');
    
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
}
