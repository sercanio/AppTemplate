namespace AppTemplate.Domain.Roles.ValueObjects;

public sealed class RoleName : ValueObject
{
    public string Value { get; }

    public RoleName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Role name cannot be empty.", nameof(value));
        }
        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
