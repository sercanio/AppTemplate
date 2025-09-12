using System.Data;

namespace AppTemplate.Application.Data.Dapper;

public interface ISqlConnectionFactory
{
  IDbConnection CreateConnection();
}
