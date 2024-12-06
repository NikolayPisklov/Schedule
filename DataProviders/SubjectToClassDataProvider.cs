using Dapper;
using Schedule.Models;

namespace Schedule.DataProviders
{
    public interface ISubjectToClassDataProvider 
    {
        Task InsertAsync(int fkSchedule, int fkTs, double hours);
    }
    public class SubjectToClassDataProvider : DataProviderBase, ISubjectToClassDataProvider
    {
        public async Task InsertAsync(int fkSchedule, int fkTs, double hours)
        {
            var sql = "INSERT INTO SubjectToClass (FkTs, FkSchedule, Hours) VALUES (@FkTs, @FkSchedule, @Hours);";
            using (var connection = CreateConnection())
            {
                await connection.ExecuteAsync(sql, new { FkTs = fkTs, FkSchedule = fkSchedule, Hours = hours});
            }
        }
    }
}
