using AppTemplate.Application.Data.Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

namespace AppTemplate.Infrastructure.Data.Dapper;

  public sealed class SqlConnectionFactory : ISqlConnectionFactory
  {
      private readonly string _connectionString;

      public SqlConnectionFactory(IConfiguration configuration)
      {
          _connectionString = configuration.GetConnectionString("Database") ??
                              throw new ArgumentNullException(nameof(configuration), "Database connection string not found.");
      }

      public IDbConnection CreateConnection()
      {
          var connection = new NpgsqlConnection(_connectionString);
          connection.Open();

          return connection;
      }
  }
