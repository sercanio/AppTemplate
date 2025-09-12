namespace AppTemplate.Domain;

public abstract class ValueObject
{
  protected abstract IEnumerable<object?> GetEqualityComponents();

  public override bool Equals(object? obj)
  {
    if (obj is null || obj.GetType() != GetType())
      return false;

    var other = (ValueObject)obj;
    return GetEqualityComponents()
        .SequenceEqual(other.GetEqualityComponents());
  }

  public override int GetHashCode()
  {
    return GetEqualityComponents()
        .Aggregate(1, (current, obj) =>
        {
          return HashCode.Combine(current, obj);
        });
  }

  public static bool operator ==(ValueObject? left, ValueObject? right)
  {
    if (left is null)
      return right is null;
    return left.Equals(right);
  }

  public static bool operator !=(ValueObject? left, ValueObject? right)
  {
    return !(left == right);
  }
}
