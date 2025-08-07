namespace Orchestrator;

public interface IOrderProcessor
{
  Task<string> ProcessOrderAsync(string order);
}
