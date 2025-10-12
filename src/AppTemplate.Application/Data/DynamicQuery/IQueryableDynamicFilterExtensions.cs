using AppTemplate.Domain;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;

namespace AppTemplate.Application.Data.DynamicQuery;

public static class IQueryableDynamicFilterExtensions
{
  private static readonly string[] _orders = { "asc", "desc" };
  private static readonly string[] _logics = { "and", "or" };

  private static readonly IDictionary<string, string> _operators = new Dictionary<string, string>
    {
        { "eq", "=" },
        { "neq", "!=" },
        { "lt", "<" },
        { "lte", "<=" },
        { "gt", ">" },
        { "gte", ">=" },
        { "isnull", "== null" },
        { "isnotnull", "!= null" },
        { "startswith", "StartsWith" },
        { "endswith", "EndsWith" },
        { "contains", "Contains" },
        { "doesnotcontain", "Contains" }
    };

  private static readonly HashSet<string> _stringOperators = new()
    {
        "startswith",
        "endswith",
        "contains",
        "doesnotcontain",
        "eq"
    };

  /// <summary>
  /// Applies filtering and sorting from a DynamicQuery to the IQueryable.
  /// If a filter/sort field corresponds to a ValueObject property,
  /// we append ".Value" automatically; otherwise, we use the field as-is.
  /// </summary>
  public static IQueryable<T> ToDynamic<T>(this IQueryable<T> query, DynamicQuery dynamicQuery)
  {
    if (dynamicQuery.Filter is not null)
      query = ApplyFilter(query, dynamicQuery.Filter);

    if (dynamicQuery.Sort is not null && dynamicQuery.Sort.Any())
      query = ApplySorting(query, dynamicQuery.Sort);

    return query;
  }

  #region Filtering

  private static IQueryable<T> ApplyFilter<T>(IQueryable<T> queryable, Filter filter)
  {
    var filters = GetAllFilters(filter);
    // We'll extract all filter.Value as parameters
    string[] values = filters.Select(f => f.Value).ToArray();

    // Build final Where clause (Dynamic LINQ string)
    string whereClause = BuildWhereClause<T>(filter, filters);
    if (!string.IsNullOrEmpty(whereClause) && values.Any())
    {
      queryable = queryable.Where(whereClause, values);
    }

    return queryable;
  }

  private static IList<Filter> GetAllFilters(Filter filter)
  {
    var filters = new List<Filter>();
    CollectFilters(filter, filters);
    return filters;
  }

  private static void CollectFilters(Filter filter, IList<Filter> filters)
  {
    filters.Add(filter);

    if (filter.Filters is not null && filter.Filters.Any())
    {
      foreach (var subFilter in filter.Filters)
      {
        CollectFilters(subFilter, filters);
      }
    }
  }

  /// <summary>
  /// Recursively build a Where clause string for this filter (and sub-filters),
  /// adding ".Value" if the field is a ValueObject property.
  /// </summary>
  private static string BuildWhereClause<T>(Filter filter, IList<Filter> filters)
  {
    // Basic checks
    if (string.IsNullOrEmpty(filter.Field))
      throw new ArgumentException("Filter field cannot be empty.");
    if (string.IsNullOrEmpty(filter.Operator) || !_operators.ContainsKey(filter.Operator))
      throw new ArgumentException($"Invalid filter operator: {filter.Operator}");

    int index = filters.IndexOf(filter);
    string comparison = _operators[filter.Operator];

    // => figure out how to reference the property in code: "fieldName.Value" if VO, else "fieldName"
    string fieldExpression = GetFieldExpression<T>(filter.Field);

    var whereClause = new StringBuilder();

    // Build expression
    if (!string.IsNullOrEmpty(filter.Value))
    {
      bool isStringOperator = _stringOperators.Contains(filter.Operator);
      bool shouldBeCaseInsensitive = isStringOperator && !filter.IsCaseSensitive;

      // The user might do "doesnotcontain"
      if (filter.Operator == "doesnotcontain")
      {
        if (shouldBeCaseInsensitive)
        {
          whereClause.Append($"(!{fieldExpression}.ToLower().{comparison}(@{index}.ToLower()))");
        }
        else
        {
          whereClause.Append($"(!{fieldExpression}.{comparison}(@{index}))");
        }
      }
      else if (comparison is "StartsWith" or "EndsWith" or "Contains")
      {
        if (shouldBeCaseInsensitive)
        {
          whereClause.Append($"({fieldExpression}.ToLower().{comparison}(@{index}.ToLower()))");
        }
        else
        {
          whereClause.Append($"({fieldExpression}.{comparison}(@{index}))");
        }
      }
      else if (isStringOperator && shouldBeCaseInsensitive)
      {
        // eq, startswith, etc. ignoring case
        whereClause.Append($"{fieldExpression}.ToLower() {comparison} @{index}.ToLower()");
      }
      else
      {
        // normal integer, date, or string eq (with case) etc.
        whereClause.Append($"{fieldExpression} {comparison} @{index}");
      }
    }
    else if (filter.Operator is "isnull" or "isnotnull")
    {
      whereClause.Append($"{fieldExpression} {comparison}");
    }

    // If there are sub-filters, combine them with AND/OR
    if (filter.Logic is not null && filter.Filters is not null && filter.Filters.Any())
    {
      if (!_logics.Contains(filter.Logic))
        throw new ArgumentException($"Invalid filter logic: {filter.Logic}");

      string subFilters = string.Join(
          $" {filter.Logic} ",
          filter.Filters.Select(f => BuildWhereClause<T>(f, filters))
      );

      return $"{whereClause} {filter.Logic} ({subFilters})";
    }

    return whereClause.ToString();
  }

  #endregion

  #region Sorting

  private static IQueryable<T> ApplySorting<T>(IQueryable<T> queryable, IEnumerable<Sort> sort)
  {
    foreach (var item in sort)
    {
      if (string.IsNullOrEmpty(item.Field))
        throw new ArgumentException("Sort field cannot be empty.");
      if (string.IsNullOrEmpty(item.Dir) || !_orders.Contains(item.Dir))
        throw new ArgumentException($"Invalid sort direction: {item.Dir}");
    }

    // e.g. "Title.Value asc, UpdatedAt desc"
    string ordering = string.Join(
        ",",
        sort.Select(s => $"{GetFieldExpression<T>(s.Field)} {s.Dir}")
    );

    return queryable.OrderBy(ordering);
  }

  #endregion

  #region Helpers

  /// <summary>
  /// For a given property name (fieldName), we detect if it's a ValueObject, 
  /// and if so, return "fieldName.Value"; otherwise, just "fieldName".
  /// </summary>
  private static string GetFieldExpression<T>(string fieldName)
  {
    // If user typed e.g. "versions[0].Title" or something, might be more complicated:
    // Basic approach: only handle top-level property
    // If you have nested, you'll need more complex logic

    var propInfo = typeof(T).GetProperty(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
    if (propInfo == null)
    {
      // if we didn't find a property, maybe user is referencing a sub-property? 
      // For example "SomeSubObject.SomeProperty"? 
      // We'll just return the raw fieldName and let the user define or handle partial matches 
      return fieldName;
    }

    // Check if property type inherits from ValueObject
    bool isValueObject = typeof(ValueObject).IsAssignableFrom(propInfo.PropertyType);
    return isValueObject ? $"{fieldName}.Value" : fieldName;
  }

  #endregion
}