namespace AppTemplate.Domain.OutboxMessages;

public sealed class OutboxOptions
{
  public int IntervalInSeconds { get; init; }

  public int BatchSize { get; init; }
}
