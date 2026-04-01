using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace DatabaseHelper.Core;

public abstract class CommonDatabase<T> : IDisposable where T : DbConnection, new()
{
    private readonly string _connectionString;
    private T? _connection;
    private DbTransaction? _transaction;
    private bool _disposed;

    protected CommonDatabase(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be empty or whitespace.", nameof(connectionString));
    }

    public bool InTransaction => _transaction != null;

    public T GetConnection()
    {
        ThrowIfDisposed();
        return _connection ??= new T { ConnectionString = _connectionString };
    }

    public void OpenConnection()
    {
        ThrowIfDisposed();
        var connection = GetConnection();
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

    public DbCommand GetCommand(string commandText, CommandType commandType, params DbParameter[] parameters)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(commandText);
        ArgumentException.ThrowIfNullOrWhiteSpace(commandText);

        var command = GetConnection().CreateCommand();
        command.CommandText = commandText;
        command.CommandType = commandType;
        command.Parameters.AddRange(parameters);

        if (_transaction != null)
            command.Transaction = _transaction;

        return command;
    }

    public DbCommand GetCommand(string commandText, params DbParameter[] parameters)
        => GetCommand(commandText, CommandType.Text, parameters);

    public int ExecuteNonQuery(string commandText, CommandType commandType, params DbParameter[] parameters)
    {
        using var command = GetCommand(commandText, commandType, parameters);
        return command.ExecuteNonQuery();
    }

    public int ExecuteNonQuery(string commandText, params DbParameter[] parameters)
        => ExecuteNonQuery(commandText, CommandType.Text, parameters);

    public object? ExecuteScalar(string commandText, CommandType commandType, params DbParameter[] parameters)
    {
        using var command = GetCommand(commandText, commandType, parameters);
        return command.ExecuteScalar();
    }

    public object? ExecuteScalar(string commandText, params DbParameter[] parameters)
        => ExecuteScalar(commandText, CommandType.Text, parameters);

    public DbDataReader ExecuteReader(string commandText, CommandType commandType, params DbParameter[] parameters)
    {
        var command = GetCommand(commandText, commandType, parameters);
        return command.ExecuteReader();
    }

    public DbDataReader ExecuteReader(string commandText, params DbParameter[] parameters)
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
        HandleTransaction(() => _transaction!.Commit());
    }

    public void RollbackTransaction()
    {
        ThrowIfDisposed();
        HandleTransaction(() => _transaction!.Rollback());
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
            _transaction = null;
            _connection?.Dispose();
            _connection = null;
        }

        _disposed = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            _transaction = null;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(CommonDatabase<T>));
    }

    ~CommonDatabase()
    {
        Dispose(false);
    }
}