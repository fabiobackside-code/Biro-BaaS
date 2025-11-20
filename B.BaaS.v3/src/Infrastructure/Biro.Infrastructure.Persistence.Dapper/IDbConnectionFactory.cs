using System.Data;

namespace Biro.Infrastructure.Persistence.Dapper;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync();
}
