using System.Data;
using DatabaseHelper.MsSql;
using Microsoft.Data.SqlClient;

namespace DatabaseHelper.App;

internal static class Program
{
    private static void Main(string[] args)
    {
        string connectionString = "";

        Database database = new Database(connectionString);
        var connection = database.GetConnection();
        var command = database.GetCommand("SELECT * FROM [Table]");
        
        // DatabaseHelper.Core.Database coreDatabase = new Core.Database(connectionString, new SqlServerDatabaseFactory());
        // coreDatabase.OpenConnection();
        //
        // coreDatabase.GetCommand("", CommandType.StoredProcedure, new SqlParameter[] { });
        //
        // coreDatabase.CloseConnection();
    }
}