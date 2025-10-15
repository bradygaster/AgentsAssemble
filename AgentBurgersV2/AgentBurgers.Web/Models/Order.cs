namespace AgentBurgers.Web.Models;

public record CustomerOrder(
    string OrderId,
    string CustomerName,
    string? BurgerType,
    string? FriesType,
    string? DrinkType
);

public class OrderResult
{
    public string OrderId { get; set; } = "";
    public string Result { get; set; } = "";
    public bool IsComplete { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}