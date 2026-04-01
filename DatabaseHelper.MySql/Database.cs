using DatabaseHelper.Core;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace DatabaseHelper.MySql;

public sealed class Database : CommonDatabase
{

    public Database(string connectionString) : base(connectionString)
    {
    }

    protected override DbConnection CreateConnection()
    {
        return new MySqlConnection(ConnectionString);
    }
}