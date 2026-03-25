using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data.SqlClient;
using Npgsql;

namespace DataGenerator.Data
{
    public class DbConnectionFactory
    {
        public static DbConnection CreateDbConnection(string connectionString) => new NpgsqlConnection(connectionString);
    }
}
