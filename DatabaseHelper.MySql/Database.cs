using System.Data;
using System.Runtime.CompilerServices;
using DatabaseHelper.Core;
using MySql.Data.MySqlClient;

namespace DatabaseHelper.MySql;

public sealed class Database : CommonDatabase
{
    private readonly string _connectionString;
    private MySqlConnection? _connection;
    private MySqlTransaction? _transaction;
    private bool _disposed = false;

    public Database(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public bool InTransaction => _transaction != null;

    public MySqlConnection GetConnection()
    {
        ThrowIfDisposed();
        if (_connection == null)
            _connection = new MySqlConnection(_connectionString);
        return _connection;
    }

    public void OpenConnection()
    {
        ThrowIfDisposed();
        MySqlConnection connection = GetConnection();
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

    public MySqlCommand GetCommand(string commandText, CommandType commandType, params MySqlParameter[] parameters)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(commandText, nameof(commandText));

        MySqlCommand command = GetConnection().CreateCommand();
        command.CommandText = commandText;
        command.CommandType = commandType;
        command.Parameters.AddRange(parameters);

        if (_transaction != null)
        {
            command.Transaction = _transaction;
        }

        return command;
    }

    public MySqlCommand GetCommand(string commandText, params MySqlParameter[] parameters)
        => GetCommand(commandText, CommandType.Text, parameters);

    public int ExecuteNonQuery(string commandText, CommandType commandType, params MySqlParameter[] parameters)
    {
        using MySqlCommand command = GetCommand(commandText, commandType, parameters);
        return command.ExecuteNonQuery();
    }

    public int ExecuteNonQuery(string commandText, params MySqlParameter[] parameters)
        => ExecuteNonQuery(commandText, CommandType.Text, parameters);

    public object? ExecuteScalar(string commandText, CommandType commandType, params MySqlParameter[] parameters)
    {
        using MySqlCommand command = GetCommand(commandText, commandType, parameters);
        return command.ExecuteScalar();
    }

    public object? ExecuteScalar(string commandText, params MySqlParameter[] parameters)
        => ExecuteScalar(commandText, CommandType.Text, parameters);

    public MySqlDataReader ExecuteReader(string commandText, CommandType commandType, params MySqlParameter[] parameters)
    {
        MySqlCommand command = GetCommand(commandText, commandType, parameters);
        return command.ExecuteReader();
    }

    public MySqlDataReader ExecuteReader(string commandText, params MySqlParameter[] parameters)
        => ExecuteReader(commandText, CommandType.Text, parameters);

    public void BeginTransaction()
    {
        ThrowIfDisposed();
        if (_transaction != null)
            throw new InvalidOperationException("Transaction is already started. Commit or rollback the current transaction before starting a new one.");
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
            throw new ObjectDisposedException(nameof(Database));
    }

    ~Database()
    {
        Dispose(false);
    }
}