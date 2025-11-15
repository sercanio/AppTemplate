using System.Data;
using AppTemplate.Application.Data.Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

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
    var connectionString = _configuration.GetConnectionString("Database");
    if (string.IsNullOrEmpty(connectionString))
    {
      throw new InvalidOperationException("Database connection string not found. Make sure your solution is properly configured.");
    }

    var connection = new NpgsqlConnection(connectionString);
    if (_openConnection)
    {
      connection.Open();
    }
    return connection;
  }
}
