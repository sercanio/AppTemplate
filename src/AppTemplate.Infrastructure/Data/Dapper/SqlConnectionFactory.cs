using AppTemplate.Application.Data.Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

namespace AppTemplate.Infrastructure.Data.Dapper;

public sealed class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly IConfiguration _configuration;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IDbConnection CreateConnection()
    {
        // Use the Aspire-provided connection string
        var connectionString = _configuration.GetConnectionString("AppTemplateDb");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("AppTemplateDb connection string not found. Make sure Aspire is properly configured.");
        }

        var connection = new NpgsqlConnection(connectionString);
        connection.Open();

        return connection;
    }
}
