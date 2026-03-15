using System.Data;

namespace DatabaseHelper.Core.Interfaces;

public interface IDatabaseFactory
{
    public IDbConnection GetConnection(string connectionString);
}