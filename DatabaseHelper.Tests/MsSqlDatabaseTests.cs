using System.Data;
using DatabaseHelper.MsSql;
using Microsoft.Data.SqlClient;

namespace DatabaseHelper.Tests;

[TestFixture]
public class MsSqlDatabaseTests
{
    private string _connectionString;
    private Database _database;
    private SqlConnection? _connection;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _connectionString = "Server=localhost;Database=Test;User Id=sa;Password=***;TrustServerCertificate=True";
    }

    [SetUp]
    public void Setup()
    {
        _database = new Database(_connectionString);
    }

    #region Ctor Tests

    [Test]
    public void Constructor_WithConnectionString_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => new Database(_connectionString));
    }

    [Test]
    public void Constructor_NullConnectionString_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Database(null));
    }

    [Test]
    public void Constructor_EmptyConnectionString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Database(string.Empty));
    }

    #endregion

    #region GetConnection Tests

    [Test]
    public void GetConnection_AtFirstCall_ReturnsSqlConnection()
    {
        _connection = _database.GetConnection();

        Assert.That(_connection, Is.Not.Null);
        Assert.That(_connection, Is.InstanceOf<SqlConnection>());
        Assert.That(_connection.State, Is.EqualTo(ConnectionState.Closed));
    }

    [Test]
    public void GetConnection_AtSecondCall_ReturnsSameSqlConnection()
    {
        _connection = _database.GetConnection();
        var connection2 = _database.GetConnection();
        Assert.That(_connection, Is.SameAs(connection2));
    }

    [Test]
    public void GetConnection_AfterDispose_ThrowsObjectDisposedException()
    {
        _database.Dispose();
        Assert.Throws<ObjectDisposedException>(() => _database.GetConnection());
    }

    #endregion

    #region Open/Close Connection Tests

    [Test]
    public void OpenConnection_WithNullConnection_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _database.OpenConnection());
    }
    
    [Test]
    public void OpenConnection_WithClosedConnection_OpensConnection()
    {
        _connection = _database.GetConnection();
        _database.OpenConnection();
        Assert.That(_connection.State, Is.EqualTo(ConnectionState.Open));
    }

    [Test]
    public void OpenConnection_WithOpenConnection_DoesNotThrows()
    {
        _connection = _database.GetConnection();
        _connection.Open();
        Assert.DoesNotThrow(() => _database.OpenConnection());
        Assert.That(_connection.State, Is.EqualTo(ConnectionState.Open));
    }
    
    [Test]
    public void OpenConnection_AfterDispose_ThrowsObjectDisposedException()
    {
        _database.Dispose();
        Assert.Throws<ObjectDisposedException>(() => _database.OpenConnection());
    }

    [Test]
    public void CloseConnection_WithOpenConnection_ClosesConnection()
    {
        _connection = _database.GetConnection();
        _connection.Open();
        _database.CloseConnection();
        Assert.That(_connection.State, Is.EqualTo(ConnectionState.Closed));
    }

    [Test]
    public void CloseConnection_WithNullConnection_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _database.CloseConnection());
    }

    [Test]
    public void CloseConnection_WithClosedConnection_DoesNotThrow()
    {
        _connection = _database.GetConnection();
        Assert.DoesNotThrow(() => _database.CloseConnection());
        Assert.That(_connection.State, Is.EqualTo(ConnectionState.Closed));
    }
    
    [Test]
    public void CloseConnection_AfterDispose_ThrowsObjectDisposedException()
    {
        _database.Dispose();
        Assert.Throws<ObjectDisposedException>(() => _database.CloseConnection());
    }

    #endregion

    #region GetCommand Tests

    [Test]
    public void GetCommand_WithoutCommandType_ReturnsTextTypeCommand()
    {
        _connection = _database.GetConnection();
        var command = _database.GetCommand("SELECT 1");
        Assert.That(command.CommandType, Is.EqualTo(CommandType.Text));
    }

    [Test]
    public void GetCommand_WithCommandType_ReturnsCommand()
    {
        _connection = _database.GetConnection();
        var command = _database.GetCommand("SELECT 1", CommandType.StoredProcedure);
        Assert.That(command.CommandType, Is.EqualTo(CommandType.StoredProcedure));   
    }
    
    [Test]
    public void GetCommand_WithNullCommandText_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _database.GetCommand(null!));
    }
    
    [Test]
    public void GetCommand_WithEmptyCommandText_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _database.GetCommand(string.Empty));
    }

    [Test]
    public void GetCommand_WithCommandText_ReturnsCommand()
    {
        _connection = _database.GetConnection();
        var command = _database.GetCommand("select 1");
        Assert.That(command.CommandText, Is.EqualTo("select 1"));  
    }

    [Test]
    public void GetCommand_WithParameters_ReturnsCommand()
    {
        _connection = _database.GetConnection();
        var command = _database.GetCommand("select @p1", new SqlParameter("@p1", 1));
        Assert.That(command.Parameters.Count, Is.EqualTo(1)); 
    }
    
    [Test]
    public void GetCommand_WithMultipleParameters_ReturnsCommand()
    {
        _connection = _database.GetConnection();
        var command = _database.GetCommand("select @p1, @p2", new SqlParameter("@p1", 1), new SqlParameter("@p2", 2));
        Assert.That(command.Parameters.Count, Is.EqualTo(2)); 
    }
    
    [Test]
    public void GetCommand_AfterDispose_ThrowsObjectDisposedException()
    {
        _database.Dispose();
        Assert.Throws<ObjectDisposedException>(() => _database.GetCommand("SELECT 1"));
    }

    #endregion

    #region ExecuteNonQuery Tests

    [Test]

    #endregion
    
    [TearDown]
    public void TearDown()
    {
        _connection.Dispose();
        _database.Dispose();
    }
}