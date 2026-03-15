using System.Data;
using Microsoft.Data.SqlClient;

namespace DatabaseHelper.MsSql;

public sealed class Database : IDisposable
{
    private readonly string _connectionString;
    private SqlConnection? _connection;
    private SqlTransaction? _transaction;

    public Database(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public SqlConnection GetConnection()
    {
        if (_connection == null)
            _connection = new SqlConnection(_connectionString);
        return _connection;
    }

    public void OpenConnection()
    {
        SqlConnection connection = GetConnection();
        if (connection.State != ConnectionState.Open)
            connection.Open();
    }

    public void CloseConnection()
    {
        if (_transaction != null)
            throw new InvalidOperationException("Cannot close connection while a transaction is in progress. Commit or rollback the transaction first.");
            
        if (_connection?.State != ConnectionState.Closed)
            _connection?.Close();
    }

    public SqlCommand GetCommand(string commandText, CommandType commandType, params SqlParameter[] parameters)
    {
        SqlCommand command = GetConnection().CreateCommand();
        command.CommandText = commandText;
        command.CommandType = commandType;
        command.Parameters.AddRange(parameters);

        if (_transaction != null)
        {
            command.Transaction = _transaction;
        }

        return command;
    }

    public SqlCommand GetCommand(string commandText, params SqlParameter[] parameters)
        => GetCommand(commandText, CommandType.Text, parameters);

    public int ExecuteNonQuery(string commandText, CommandType commandType, params SqlParameter[] parameters)
    {
        using SqlCommand command = GetCommand(commandText, commandType, parameters);
        return command.ExecuteNonQuery();
    }

    public int ExecuteNonQuery(string commandText, params SqlParameter[] parameters)
        => ExecuteNonQuery(commandText, CommandType.Text, parameters);

    public object? ExecuteScalar(string commandText, CommandType commandType, params SqlParameter[] parameters)
    {
        using SqlCommand command = GetCommand(commandText, commandType, parameters);
        return command.ExecuteScalar();
    }

    public object? ExecuteScalar(string commandText, params SqlParameter[] parameters)
        => ExecuteScalar(commandText, CommandType.Text, parameters);

    public SqlDataReader ExecuteReader(string commandText, CommandType commandType, params SqlParameter[] parameters)
    {
        SqlCommand command = GetCommand(commandText, commandType, parameters);
        return command.ExecuteReader();
    }

    public SqlDataReader ExecuteReader(string commandText, params SqlParameter[] parameters)
        => ExecuteReader(commandText, CommandType.Text, parameters);

    //todo: Add transaction support with methods like BeginTransaction, CommitTransaction, and RollbackTransaction.

    public void BeginTransaction()
    {
        if (_transaction == null)
            _transaction = GetConnection().BeginTransaction();
        else
            throw new InvalidOperationException(
                "Transaction is already started. Commit or rollback the current transaction before starting a new one.");
    }

    public void CommitTransaction()
    {
        if (_transaction == null)
            throw new InvalidOperationException(
                "No transaction is in progress. Start a transaction before committing it.");

        try
        {
            _transaction.Commit();
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void RollbackTransaction()
    {
        if (_transaction == null)
            throw new InvalidOperationException(
                "No transaction is in progress. Start a transaction before rolling it back.");

        try
        {
            _transaction.Rollback();
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
        }
    }

    //todo: Implement correct disposable pattern with finalizer and protected virtual Dispose(bool disposing) method.
    public void Dispose()
    {
        _connection?.Dispose();
    }
}