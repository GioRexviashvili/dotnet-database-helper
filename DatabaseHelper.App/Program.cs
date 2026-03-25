using System.Data;
using DatabaseHelper.MsSql;
using Microsoft.Data.SqlClient;

namespace DatabaseHelper.App;

internal static class Program
{
    private static void Main(string[] args)
    {
        string connectionString = "Server=localhost,1433;Database=Northwind;User Id=sa;Password=***;TrustServerCertificate=True";

        using (var database = new Database(connectionString))
        {
            database.OpenConnection();
            database.BeginTransaction();
            try
            {
                int rowsAffected = database.ExecuteNonQuery(
                    "insert into Categories (CategoryName, Description) values (@CategoryName, @Description)",
                    new SqlParameter("@CategoryName", "category 001"),
                    new SqlParameter("@Description", "description 001"));

                Console.WriteLine($"Inserted {rowsAffected} rows");

                database.CommitTransaction();
                Console.WriteLine("Transaction committed");
            }
            catch (Exception ex)
            {
                database.RollbackTransaction();
                Console.WriteLine($"Transaction rolled back: {ex.Message}");
            }
        }
    }
}