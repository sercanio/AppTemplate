using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using System.Linq.Expressions;

namespace AppTemplate.Application.Repositories.NoSQL;

public interface INoSqlRepository<T>
{
    Task<T?> GetAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<IPaginatedList<T>> GetAllAsync<TKey>(Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>> orderBy = null,
        bool descending = false,
        int pageIndex = 0,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
    Task<IPaginatedList<T>> GetAllAsync(int pageIndex = 0, int pageSize = 10, Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetByPredicateAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}
