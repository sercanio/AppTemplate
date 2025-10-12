using System.Data;
using AppTemplate.Infrastructure.Data.Dapper;

public class DateOnlyTypeHandlerUnitTests
{
  [Fact]
  public void Parse_ReturnsDateOnly_FromDateTime()
  {
    var handler = new DateOnlyTypeHandler();
    var dateTime = new DateTime(2024, 6, 1, 12, 30, 0);
    var expected = DateOnly.FromDateTime(dateTime);

    var result = handler.Parse(dateTime);

    Assert.Equal(expected, result);
  }

  [Fact]
  public void SetValue_SetsParameterDbTypeAndValue()
  {
    var handler = new DateOnlyTypeHandler();
    var parameter = new FakeDbDataParameter();
    var dateOnly = new DateOnly(2024, 6, 1);

    handler.SetValue(parameter, dateOnly);

    Assert.Equal(DbType.Date, parameter.DbType);
    Assert.Equal(dateOnly, parameter.Value);
  }

  // Simple fake for IDbDataParameter
  private class FakeDbDataParameter : IDbDataParameter
  {
    public DbType DbType { get; set; }
    public object? Value { get; set; }
    // Unused members
    public byte Precision { get; set; }
    public byte Scale { get; set; }
    public int Size { get; set; }
    public string ParameterName { get; set; } = string.Empty;
    public ParameterDirection Direction { get; set; }
    public bool IsNullable => true;
    public string SourceColumn { get; set; } = string.Empty;
    public DataRowVersion SourceVersion { get; set; }
    public bool SourceColumnNullMapping { get; set; }
  }
}
