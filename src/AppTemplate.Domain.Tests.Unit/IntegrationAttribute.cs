using Xunit;

namespace AppTemplate.Domain.Tests.Integration;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class IntegrationAttribute : TraitAttribute
{
  public IntegrationAttribute() : base("Category", "Integration") { }
}