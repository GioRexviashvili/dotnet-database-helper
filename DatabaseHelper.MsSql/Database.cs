using System.Data;
using Microsoft.Data.SqlClient;

namespace DatabaseHelper.MsSql;

public sealed class Database : IDisposable
{
    private readonly string _connectionString;
    private SqlConnection? _connection;
    private SqlTransaction? _transaction;
    private bool _disposed = false;

    public Database(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public SqlConnection GetConnection()
    {
        ThrowIfDisposed();
        if (_connection == null)
            _connection = new SqlConnection(_connectionString);
        return _connection;
    }

    public void OpenConnection()
    {
        ThrowIfDisposed();
        SqlConnection connection = GetConnection();
        if (connection.State != ConnectionState.Open)
            connection.Open();
    }

    public void CloseConnection()
    {
        ThrowIfDisposed();
        if (_transaction != null)
            throw new InvalidOperationException(
                "Cannot close connection while a transaction is in progress. Commit or rollback the transaction first.");

        if (_connection?.State != ConnectionState.Closed)
            _connection?.Close();
    }

    public SqlCommand GetCommand(string commandText, CommandType commandType, params SqlParameter[] parameters)
    {
        ThrowIfDisposed();
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
        ThrowIfDisposed();
        using SqlCommand command = GetCommand(commandText, commandType, parameters);
        return command.ExecuteNonQuery();
    }

    public int ExecuteNonQuery(string commandText, params SqlParameter[] parameters)
        => ExecuteNonQuery(commandText, CommandType.Text, parameters);

    public object? ExecuteScalar(string commandText, CommandType commandType, params SqlParameter[] parameters)
    {
        ThrowIfDisposed();
        using SqlCommand command = GetCommand(commandText, commandType, parameters);
        return command.ExecuteScalar();
    }

    public object? ExecuteScalar(string commandText, params SqlParameter[] parameters)
        => ExecuteScalar(commandText, CommandType.Text, parameters);

    public SqlDataReader ExecuteReader(string commandText, CommandType commandType, params SqlParameter[] parameters)
    {
        ThrowIfDisposed();
        SqlCommand command = GetCommand(commandText, commandType, parameters);
        return command.ExecuteReader();
    }

    public SqlDataReader ExecuteReader(string commandText, params SqlParameter[] parameters)
        => ExecuteReader(commandText, CommandType.Text, parameters);

    public void BeginTransaction()
    {
        ThrowIfDisposed();
        if (_transaction != null)
            throw new InvalidOperationException(
                "Transaction is already started. Commit or rollback the current transaction before starting a new one.");

        _transaction = GetConnection().BeginTransaction();
    }

    public void CommitTransaction()
    {
        ThrowIfDisposed();
        HandleTransaction(() => _transaction?.Commit());
    }
    
    public void RollbackTransaction()
    {
        ThrowIfDisposed();
        HandleTransaction(() => _transaction?.Rollback());
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _transaction?.Dispose();
            _connection?.Dispose();
        }

        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Database));
    }
    
    private void HandleTransaction(Action action)
    {
        if (_transaction == null)
            throw new InvalidOperationException("No transaction is in progress.");

        try
        {
            action();
        }
        finally
        {
            _transaction.Dispose();
        }
    }

    ~Database()
    {
        Dispose(false);
    }
}