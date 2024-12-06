using Dapper;
using Schedule.Models;
using Schedule.Models.CombinedModels;

namespace Schedule.DataProviders
{
    public interface IScheduleDataProvider
    {
        Task<IEnumerable<ScheduleJoin>?> GetSchedulesForAYearAsync();
        Task<IEnumerable<DayOfTheWeek?>> GetDaysOfTheWeek();
    }
    public class ScheduleDataProvider : DataProviderBase, IScheduleDataProvider
    {
        public async Task<IEnumerable<ScheduleJoin>?> GetSchedulesForAYearAsync()
        {
            using (var connection = CreateConnection())
            {
                var sql = @"SELECT s.Id AS ScheduleId, Title AS ClassTitle, s.Year AS ScheduleYear 
                    FROM Schedule s INNER JOIN Class c ON s.FkClass = c.Id
                    WHERE s.Id >= 10;";
                var schedules = await connection.QueryAsync<ScheduleJoin>(sql);
                return schedules.ToList();
            }
        }
        public async Task<IEnumerable<DayOfTheWeek?>> GetDaysOfTheWeek() 
        {
            using (var connection = CreateConnection()) 
            {
                var sql = "SELECT * FROM DayOfTheWeek";
                var days = await connection.QueryAsync<DayOfTheWeek>(sql);
                return days.ToList();
            }
        }
    }
}
