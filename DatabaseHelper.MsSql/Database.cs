using DatabaseHelper.Core;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace DatabaseHelper.MsSql;

public sealed class Database : CommonDatabase
{
    public Database(string connectionString) : base(connectionString)
    {
    }

    protected override DbConnection CreateConnection()
    {
        return new SqlConnection(ConnectionString);
    }
}