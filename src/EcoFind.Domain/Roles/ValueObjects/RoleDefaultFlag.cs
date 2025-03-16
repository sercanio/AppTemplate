using Myrtus.Clarity.Core.Domain.Abstractions;

namespace EcoFind.Domain.Roles.ValueObjects;

public sealed class RoleDefaultFlag : ValueObject
{
    public bool Value { get; }

    public RoleDefaultFlag(bool value)
    {
        Value = value;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
