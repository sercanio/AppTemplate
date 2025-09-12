namespace AppTemplate.Application.Data.Pagination;

public interface IPaginatedList<T>
{
    List<T> Items { get; }
    int PageIndex { get; }
    int PageSize { get; }
    int TotalPages { get; }
    int TotalCount { get; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
}
