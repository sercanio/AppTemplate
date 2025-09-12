namespace AppTemplate.Application.Services.Clock;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
