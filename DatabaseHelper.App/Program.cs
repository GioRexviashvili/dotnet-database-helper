using System.Data.Common;
using DatabaseHelper.Core;
using Microsoft.Data.SqlClient;

namespace DatabaseHelper.App;

internal static class Program
{
    private static void Main(string[] args)
    {
        const string connectionString = "Server=localhost,1433;Database=Northwind;User Id=sa;Password=***;TrustServerCertificate=True";
        
        var database = new DatabaseHelper.MsSql.Database(connectionString);
        var database2 = new DatabaseHelper.MySql.Database(connectionString);
        
        var con1 = database.GetConnection(); // con1 automatically is type of SqlConnection and not DbConnection :)
        con1.OpenAsync();
        
        var con2 = database2.GetConnection(); // con2 automatically is type of MySqlConnection and not DbConnection :)
        con2.OpenAsync();
        
        var command1 = database.GetCommand("select 1"); // but here command1 is type of DbCommand (not SqlCommand) :(
                                                        // there is a problem when I add generic for command
    }
}