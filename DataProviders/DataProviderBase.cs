using System.Data;
using System.IO;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Schedule.DataProviders
{
    public class DataProviderBase
    {
        private string _dbPath = Path
            .Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Schedule.db");
        public IDbConnection CreateConnection() 
        {
            return new SqliteConnection($"Data Source={_dbPath}");
        }
    }
}
