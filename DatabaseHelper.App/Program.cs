using System.Data;
using DatabaseHelper.MsSql;

namespace DatabaseHelper.App;

internal static class Program
{
    private static void Main(string[] args)
    {
        const string connectionString = "Server=localhost,1433;Database=Northwind;User Id=sa;Password=***;TrustServerCertificate=True";
        var database = new DatabaseHelper.MsSql.Database(connectionString);
    }
}