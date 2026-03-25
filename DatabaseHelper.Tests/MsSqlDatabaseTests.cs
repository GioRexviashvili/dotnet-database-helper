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

        SqlConnection sqlConnection = new SqlConnection(_connectionString);
        sqlConnection.Open();
        using var command = sqlConnection.CreateCommand();
        command.CommandText = @"
if object_id('dbo.TestTable', 'U') is null
create table dbo.TestTable (
    Id int identity(1,1) primary key,
    Name nvarchar(50) not null
);";

        command.ExecuteNonQuery();
        sqlConnection.Close();
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
    public void ExecuteNonQuery_WithCommand_ReturnsAffectedRows()
    {
        _database.OpenConnection();
        var affectedRows = _database.ExecuteNonQuery("insert into dbo.TestTable (Name) values ('Test1')");

        Assert.That(affectedRows, Is.EqualTo(1));
        Assert.That(_database.ExecuteScalar("select count(*) from dbo.TestTable where Name = 'Test1'"), Is.EqualTo(1));
    }

    [Test]
    public void ExecuteNonQuery_AfterDispose_ThrowsObjectDisposedException()
    {
        _database.Dispose();
        Assert.Throws<ObjectDisposedException>(() => _database.ExecuteNonQuery("select 1"));
    }

    #endregion

    #region ExecuteScalar Tests

    [Test]
    public void ExecuteScalar_AfterDispose_ThrowsObjectDisposedException()
    {
        _database.Dispose();
        Assert.Throws<ObjectDisposedException>(() => _database.ExecuteScalar("select 1"));
    }

    [Test]
    public void ExecuteScalar_WithCommand_ReturnsScalar()
    {
        _connection = _database.GetConnection();
        _database.OpenConnection();
        var result = _database.ExecuteScalar("select 1");
        Assert.That(result, Is.EqualTo(1));
    }

    #endregion

    #region ExecuteReader Tests

    [Test]
    public void ExecuteReader_WithCommand_ReturnsDataReader()
    {
        _connection = _database.GetConnection();
        _database.OpenConnection();
        using var reader = _database.ExecuteReader("select * from dbo.TestTable");
        Assert.That(reader, Is.Not.Null);
        Assert.That(reader, Is.InstanceOf<SqlDataReader>());
    }

    [Test]
    public void ExecuteReader_AfterDispose_ThrowsObjectDisposedException()
    {
        _database.Dispose();
        Assert.Throws<ObjectDisposedException>(() => _database.ExecuteReader("select * from dbo.TestTable"));
    }

    #endregion

    #region Transaction Tests

    [Test]
    public void BeginTransaction_WithTransactionInProgress_ThrowsInvalidOperationException()
    {
        _database.OpenConnection();
        _database.BeginTransaction();
        Assert.Throws<InvalidOperationException>(() => _database.BeginTransaction());
    }

    [Test]
    public void BeginTransaction_StartsTransaction() {
        _database.OpenConnection();
        _database.BeginTransaction();
        Assert.That(_database.InTransaction, Is.True);
    }

    [Test]
    public void BeginTransaction_AfterDispose_ThrowsObjectDisposedException()
    {
        _database.Dispose();
        Assert.Throws<ObjectDisposedException>(() => _database.BeginTransaction());
    }

    [Test]
    public void CommitTransaction_AfterDispose_ThrowsInvalidOperationException()
    {
        _database.Dispose();
        Assert.Throws<ObjectDisposedException>(() => _database.CommitTransaction());
    }

    [Test]
    public void CommitTransaction_WithoutTransactionInProgress_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => _database.CommitTransaction());
    }

    [Test]
    public void CommitTransaction_CommitsTransaction()
    {
        _database.OpenConnection();
        _database.BeginTransaction();
        _database.ExecuteNonQuery("insert into dbo.TestTable (Name) values ('Test2')");
        _database.CommitTransaction();
        var result = _database.ExecuteScalar("select count(*) from dbo.TestTable where Name = 'Test2'");
        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public void RollbackTransaction_WithoutTransactionInProgress_ThrowsInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => _database.RollbackTransaction());
    }

    [Test]
    public void RollbackTransaction_RollsBackTransaction()
    {
        _database.OpenConnection();
        _database.BeginTransaction();
        _database.ExecuteNonQuery("insert into dbo.TestTable (Name) values ('Test3')");
        _database.RollbackTransaction();
        var result = _database.ExecuteScalar("select count(*) from dbo.TestTable where Name = 'Test3'");
        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void RollbackTransaction_AfterDispose_ThrowsInvalidOperationException()
    {
        _database.Dispose();
        Assert.Throws<ObjectDisposedException>(() => _database.RollbackTransaction());
    }

    #endregion

    [TearDown]
    public void TearDown()
    {
        _connection?.Dispose();
        _database.Dispose();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        using (var sqlConnection = new SqlConnection(_connectionString))
        {
            sqlConnection.Open();
            var command = sqlConnection.CreateCommand();
            command.CommandText = "if object_id('dbo.TestTable', 'U') is not null drop table dbo.TestTable";
            command.ExecuteNonQuery();
            sqlConnection.Dispose();
        }
    }
}