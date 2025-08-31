using AppTemplate.Core.Domain.Abstractions;

namespace AppTemplate.Infrastructure.Tests.Unit.RepositoriesTests;

public class SoftDeleteEntity : Entity<Guid>
{
  public DateTime? DeletedOnUtc { get; set; }
}

public class RepositoryUnitTests
{
  [Fact]
  public void IsSoftDeleted_ShouldReturnTrue_IfDeletedOnUtcHasValue()
  {
    var entity = new SoftDeleteEntity { DeletedOnUtc = DateTime.UtcNow };
    var isSoftDeleted = entity.DeletedOnUtc != null;
    Assert.True(isSoftDeleted);
  }

  [Fact]
  public void IsSoftDeleted_ShouldReturnFalse_IfDeletedOnUtcIsNull()
  {
    var entity = new SoftDeleteEntity { DeletedOnUtc = null };
    var isSoftDeleted = entity.DeletedOnUtc != null;
    Assert.False(isSoftDeleted);
  }
}