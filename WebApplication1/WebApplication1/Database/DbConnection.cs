using System.Data;
using System.Data.SqlClient;

namespace QL_NganQuy.Database
{
    public class DbConnection
    {
        private readonly string _connectionString;

        public DbConnection(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}