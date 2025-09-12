namespace AppTemplate.Application.Data.DynamicQuery;

public class Filter
{
    public string Field { get; set; }
    public string Operator { get; set; }
    public string? Value { get; set; }
    public string? Logic { get; set; }
    public IEnumerable<Filter>? Filters { get; set; }
    public bool IsCaseSensitive { get; set; } = true;

    public Filter()
    {
        Field = string.Empty;
        Operator = string.Empty;
    }

    public Filter(string field, string @operator)
    {
        Field = field;
        Operator = @operator;
    }
}