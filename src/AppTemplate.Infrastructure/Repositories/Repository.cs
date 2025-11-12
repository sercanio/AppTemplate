using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using AppTemplate.Application.Data.DynamicQuery;
using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Repositories;
using AppTemplate.Domain;
using Microsoft.EntityFrameworkCore;

namespace AppTemplate.Infrastructure.Repositories;

public abstract class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
{
  protected readonly ApplicationDbContext DbContext;

  protected Repository(ApplicationDbContext dbContext)
  {
    DbContext = dbContext;
  }

  public async Task<TEntity?> GetAsync(
      Expression<Func<TEntity, bool>> predicate,
      bool includeSoftDeleted = false,
      Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
      bool asNoTracking = true,
      CancellationToken cancellationToken = default)
  {
    IQueryable<TEntity> query = DbContext.Set<TEntity>();

    if (!includeSoftDeleted && HasSoftDelete())
      query = query.Where(e => EF.Property<DateTime?>(e, "DeletedOnUtc") == null);

    if (include is not null)
      query = include(query);

    if (asNoTracking)
      query = query.AsNoTracking();

    return await query.FirstOrDefaultAsync(predicate, cancellationToken);
  }

  public async Task<PaginatedList<TEntity>> GetAllAsync(
      int pageIndex = 0,
      int pageSize = 10,
      Expression<Func<TEntity, bool>>? predicate = null,
      bool includeSoftDeleted = false,
      Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
      bool asNoTracking = true,
      CancellationToken cancellationToken = default)
  {
    IQueryable<TEntity> query = DbContext.Set<TEntity>();

    if (!includeSoftDeleted && HasSoftDelete())
      query = query.Where(e => EF.Property<DateTime?>(e, "DeletedOnUtc") == null);

    if (predicate != null)
      query = query.Where(predicate);

    if (include is not null)
      query = include(query);

    if (asNoTracking)
      query = query.AsNoTracking();

    int total = await query.CountAsync(cancellationToken);
    var items = await query
        .Skip(pageIndex * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);

    return new PaginatedList<TEntity>(items, total, pageIndex, pageSize);
  }

  public async Task<PaginatedList<TEntity>> GetAllDynamicAsync(
      DynamicQuery dynamicQuery,
      int pageIndex = 0,
      int pageSize = 10,
      bool includeSoftDeleted = false,
      Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
      bool asNoTracking = false,
      CancellationToken cancellationToken = default)
  {
    IQueryable<TEntity> query = DbContext.Set<TEntity>();

    if (asNoTracking)
      query = query.AsNoTracking();

    if (!includeSoftDeleted && HasSoftDelete())
      query = query.Where(e => EF.Property<DateTime?>(e, "DeletedOnUtc") == null);

    if (include is not null)
      query = include(query);

    query = query.ToDynamic(dynamicQuery);

    int total = await query.CountAsync(cancellationToken);
    var items = await query
        .Skip(pageIndex * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);

    return new PaginatedList<TEntity>(items, total, pageIndex, pageSize);
  }

  public virtual async Task<bool> ExistsAsync(
      Expression<Func<TEntity, bool>> predicate,
      bool includeSoftDeleted = false,
      bool asNoTracking = true,
      CancellationToken cancellationToken = default)
  {
    IQueryable<TEntity> query = DbContext.Set<TEntity>();

    if (!includeSoftDeleted && HasSoftDelete())
      query = query.Where(e => EF.Property<DateTime?>(e, "DeletedOnUtc") == null);

    if (asNoTracking)
      query = query.AsNoTracking();

    return await query.AnyAsync(predicate, cancellationToken);
  }

  public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
  {
    await DbContext.AddAsync(entity, cancellationToken);
  }

  public virtual void Update(TEntity entity, CancellationToken cancellationToken = default)
  {
    DbContext.Update(entity);
  }

  public virtual void Delete(TEntity entity, bool isSoftDelete = true, CancellationToken cancellationToken = default)
  {
    if (isSoftDelete && HasSoftDelete())
    {
      entity.MarkDeleted();
      DbContext.Update(entity);
    }
    else
    {
      DbContext.Remove(entity);
    }
  }

  public async Task<int> CountAsync(
      Expression<Func<TEntity, bool>>? predicate = null,
      bool includeSoftDeleted = false,
      CancellationToken cancellationToken = default)
  {
    IQueryable<TEntity> query = DbContext.Set<TEntity>();

    if (!includeSoftDeleted && HasSoftDelete())
      query = query.Where(e => EF.Property<DateTime?>(e, "DeletedOnUtc") == null);

    if (predicate != null)
      query = query.Where(predicate);

    return await query.CountAsync(cancellationToken);
  }

  private static bool HasSoftDelete()
      => typeof(TEntity).GetProperty("DeletedOnUtc") != null;
}
