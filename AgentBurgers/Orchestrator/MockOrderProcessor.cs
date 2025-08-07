using System.Text;

namespace Orchestrator;

public class MockOrderProcessor : IOrderProcessor
{
  public async Task<string> ProcessOrderAsync(string order)
  {
    await Task.Delay(100);
    
    return GenerateMockResponse(order);
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
