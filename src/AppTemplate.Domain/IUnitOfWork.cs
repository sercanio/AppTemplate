namespace AppTemplate.Domain;

public interface IUnitOfWork
{
  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
  void ClearChangeTracker();
}
