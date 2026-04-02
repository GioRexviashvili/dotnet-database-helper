using DatabaseHelper.Core;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace DatabaseHelper.MySql;

public sealed class Database(string connectionString) : CommonDatabase<MySqlConnection, MySqlCommand, MySqlTransaction, MySqlDataReader, MySqlParameter>(connectionString);