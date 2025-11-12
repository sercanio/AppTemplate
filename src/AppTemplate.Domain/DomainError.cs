using AppTemplate.Domain.Exceptions;

namespace AppTemplate.Domain;

public record DomainError(string Code, int? StatusCode, string Name)
{
  public static readonly DomainError None = new(string.Empty, null, string.Empty);

  public static readonly DomainError NullValue = new("Error.NullValue", 400, "Null value was provided");


  // Validation errors
  public IEnumerable<ValidationError>? Errors { get; init; }

  // Constructor with validation errors
  public DomainError(string code, int? statusCode, string name, IEnumerable<ValidationError> errors)
      : this(code, statusCode, name)
  {
    Errors = errors?.ToList();
  }
}
