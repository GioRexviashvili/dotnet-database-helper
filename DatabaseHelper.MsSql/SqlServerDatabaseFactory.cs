using System.Data;
using DatabaseHelper.Core.Interfaces;
using Microsoft.Data.SqlClient;

namespace DatabaseHelper.MsSql;

public sealed class SqlServerDatabaseFactory : IDatabaseFactory
{
    public IDbConnection GetConnection(string connectionString)
    {
        Database db = new Database(connectionString);
        return db.GetConnection();
    }
}