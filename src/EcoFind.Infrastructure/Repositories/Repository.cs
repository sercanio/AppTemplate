using Microsoft.EntityFrameworkCore;
using Myrtus.Clarity.Core.Application.Abstractions.Pagination;
using Myrtus.Clarity.Core.Application.Repositories;
using Myrtus.Clarity.Core.Domain.Abstractions;
using Myrtus.Clarity.Core.Infrastructure.Dynamic;
using Myrtus.Clarity.Core.Infrastructure.Pagination;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace EcoFind.Infrastructure.Repositories;

internal abstract class Repository<T>(ApplicationDbContext dbContext) : IRepository<T>
    where T : Entity
{
    protected readonly ApplicationDbContext DbContext = dbContext;

    public async Task<T?> GetAsync(
        Expression<Func<T, bool>> predicate,
        bool includeSoftDeleted = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] include)
    {
        IQueryable<T> query = DbContext.Set<T>().AsQueryable();

        if (!includeSoftDeleted && typeof(T).GetProperty("DeletedOnUtc") != null)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T), "entity");
            MemberExpression property = Expression.Property(parameter, "DeletedOnUtc");
            BinaryExpression comparison = Expression.Equal(property, Expression.Constant(null));
            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(comparison, parameter);

            query = query.Where(lambda);
        }

        foreach (Expression<Func<T, object>> item in include)
        {
            query = query.Include(item);
        }

        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<IPaginatedList<T>> GetAllAsync(
        int pageIndex = 0,
        int pageSize = 10,
        bool includeSoftDeleted = false,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] include)
    {
        IQueryable<T> query = DbContext.Set<T>().AsQueryable();

        if (!includeSoftDeleted && typeof(T).GetProperty("DeletedOnUtc") != null)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T), "entity");
            MemberExpression property = Expression.Property(parameter, "DeletedOnUtc");
            BinaryExpression comparison = Expression.Equal(property, Expression.Constant(null));
            Expression<Func<T, bool>> softDeleteFilter = Expression.Lambda<Func<T, bool>>(comparison, parameter);
            query = query.Where(softDeleteFilter);
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        foreach (Expression<Func<T, object>> includeExpression in include)
        {
            query = query.Include(includeExpression);
        }

        int count = await query.CountAsync(cancellationToken);

        List<T> items = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<T>(items, count, pageIndex, pageSize);
    }

    public async Task<IPaginatedList<T>> GetAllDynamicAsync(
        DynamicQuery dynamicQuery,
        int pageIndex = 0,
        int pageSize = 10,
        bool includeSoftDeleted = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] include)
    {
        IQueryable<T> query = DbContext.Set<T>().AsQueryable();

        if (!includeSoftDeleted && typeof(T).GetProperty("DeletedOnUtc") != null)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T), "entity");
            MemberExpression property = Expression.Property(parameter, "DeletedOnUtc");
            BinaryExpression comparison = Expression.Equal(property, Expression.Constant(null));
            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(comparison, parameter);
            query = query.Where(lambda);
        }

        foreach (Expression<Func<T, object>> includeExpression in include)
        {
            query = query.Include(includeExpression);
        }

        query = query.ToDynamic(dynamicQuery);

        int totalCount = await query.CountAsync(cancellationToken);

        List<T> items = await query
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<T>(items, totalCount, pageIndex, pageSize);
    }

    public virtual async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        bool includeSoftDeleted = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = DbContext.Set<T>().AsQueryable();

        if (!includeSoftDeleted && typeof(T).GetProperty("DeletedOnUtc") != null)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T), "entity");
            MemberExpression property = Expression.Property(parameter, "DeletedOnUtc");
            BinaryExpression comparison = Expression.Equal(property, Expression.Constant(null));
            Expression<Func<T, bool>> lambda = Expression.Lambda<Func<T, bool>>(comparison, parameter);

            query = query.Where(lambda);
        }

        return await query.AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task AddAsync(T entity)
    {
        await DbContext.AddAsync(entity);
    }

    public virtual void Update(T entity)
    {
        DbContext.Update(entity);
    }

    public virtual void Delete(T entity, bool isSoftDelete = true)
    {
        PropertyInfo? property = typeof(T).GetProperty("MarkDeleted");
        property?.SetValue(entity, null);

        DbContext.Update(entity);
    }
}

