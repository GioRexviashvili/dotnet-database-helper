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
        
        var command1 = database.GetCommand("select 1"); // same here for command
    }
}