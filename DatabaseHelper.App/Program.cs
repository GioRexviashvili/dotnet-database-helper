using DatabaseHelper.MsSql;

namespace DatabaseHelper.App;

internal static class Program
{
    private static void Main(string[] args)
    {
        Database database = new Database("");
        var connection = database.GetConnection();
        var command = database.GetCommand("SELECT * FROM [Table]");
    }
}