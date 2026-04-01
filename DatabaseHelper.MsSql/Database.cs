using DatabaseHelper.Core;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace DatabaseHelper.MsSql;

public sealed class Database(string connectionString) : CommonDatabase<SqlConnection>(connectionString);