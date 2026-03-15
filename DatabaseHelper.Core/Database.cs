using System.Data;
using DatabaseHelper.Core.Interfaces;

namespace DatabaseHelper.Core;

public sealed class Database : IDisposable
{
    private readonly string _connectionString;
    private IDbConnection? _connection;
    private readonly IDatabaseFactory _databaseFactory;

    public Database(string connectionString, IDatabaseFactory databaseFactory)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _databaseFactory = databaseFactory ?? throw new ArgumentNullException(nameof(databaseFactory));
    }

    public IDbConnection GetConnection()
    {
        _connection ??= _databaseFactory.GetConnection(_connectionString);
        return _connection;
    }

    public IDbCommand GetCommand(string commandText, CommandType commandType, IDbDataParameter[] parameters)
    {
        IDbCommand command = GetConnection().CreateCommand();
        command.CommandText = commandText;
        command.CommandType = commandType;
        command.Parameters.Add(parameters);
        return command;
    }
    
    public IDbCommand GetCommand(string commandText, IDbDataParameter[] parameters)
        => GetCommand(commandText, CommandType.Text, parameters);

    public void OpenConnection()
    {
        GetConnection().Open();
    }
    
    public void CloseConnection()
    {
        GetConnection().Close();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}