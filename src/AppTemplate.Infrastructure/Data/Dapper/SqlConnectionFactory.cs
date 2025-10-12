using AppTemplate.Application.Data.Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

namespace AppTemplate.Infrastructure.Data.Dapper;

public sealed class SqlConnectionFactory : ISqlConnectionFactory
{
  private readonly IConfiguration _configuration;
  private readonly bool _openConnection;

  public SqlConnectionFactory(IConfiguration configuration, bool openConnection = true)
  {
    _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    _openConnection = openConnection;
  }

  public IDbConnection CreateConnection()
  {
    var connectionString = _configuration.GetConnectionString("AppTemplateDb");
    if (string.IsNullOrEmpty(connectionString))
    {
      throw new InvalidOperationException("AppTemplateDb connection string not found. Make sure Aspire is properly configured.");
    }

    var connection = new NpgsqlConnection(connectionString);
    if (_openConnection)
    {
      connection.Open();
    }
    return connection;
  }
}
