using System.Data;
using AppTemplate.Infrastructure.Data.Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace AppTemplate.Infrastructure.Tests.Unit.Data.Dapper;

public class SqlConnectionFactoryUnitTests
{
  [Fact]
  public void Constructor_ThrowsArgumentNullException_WhenConfigurationIsNull()
  {
    Assert.Throws<ArgumentNullException>(() => new SqlConnectionFactory(null!));
  }

  [Fact]
  public void CreateConnection_ThrowsInvalidOperationException_WhenConnectionStringIsMissing()
  {
    var config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>())
        .Build();

    var factory = new SqlConnectionFactory(config);

    var ex = Assert.Throws<InvalidOperationException>(() => factory.CreateConnection());
    Assert.Contains("AppTemplateDb connection string not found", ex.Message);
  }

  [Fact]
  public void CreateConnection_ReturnsConnection_WhenConnectionStringIsValid()
  {
    var connectionString = "Host=localhost;Database=testdb;Username=test;Password=test";
    var config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            { "ConnectionStrings:AppTemplateDb", connectionString }
        })
        .Build();

    var factory = new SqlConnectionFactory(config, openConnection: false);

    using var connection = factory.CreateConnection();
    Assert.NotNull(connection);
    Assert.IsType<NpgsqlConnection>(connection);
    Assert.Equal(connectionString, connection.ConnectionString);
  }
}