using Dapper;
using Schedule.Models;

namespace Schedule.DataProviders
{
    public interface IClassesDataProvider
    {
        Task <IEnumerable<Class>?> GetAllAsync();
        Task InsertClassAsync(Class classModel);
        Task DeleteClassAsync(int id);
        Task UpdateClassAsync(string newTitle, int newYear, int id);
        Task<Class> GetLatestAddedClass();
        Task<bool> CheckIfSlotForClassExistsAsync(int id);
    }
    public class ClassesDataProvider : DataProviderBase, IClassesDataProvider
    {
        public async Task<bool> CheckIfSlotForClassExistsAsync(int id) 
        {
            using (var connection = CreateConnection())
            {
                var sql = "SELECT EXISTS(SELECT 1 FROM Slot s INNER JOIN  Schedule sch ON sch.Id = s.FkSchedule WHERE sch.FkClass = @Fk);";
                var isExists = await connection.ExecuteScalarAsync<bool>(sql, new { Fk = id});
                return isExists;
            }
        }
        public async Task DeleteClassAsync(int id)
        {
            using (var connection = CreateConnection()) 
            {
                var sql = "DELETE FROM Class Where Id = @Id";
                await connection.ExecuteAsync(sql, new { Id = id});
            }
        }

        public async Task UpdateClassAsync(string newTitle, int newYear, int id)
        {
            using (var connection = CreateConnection()) 
            {
                var sql = "UPDATE Class SET Title = @Title, Year = @Year WHERE Id = @Id";
                await connection.ExecuteAsync(sql, new
                {
                    Title = newTitle,
                    Year = newYear,
                    Id = id
                });
            }
        }

        public async Task<IEnumerable<Class>?> GetAllAsync()
        {
            using(var connection = CreateConnection()) 
            {
                var sql = "SELECT Id, Title, Year FROM Class;";
                var classes = await connection.QueryAsync<Class>(sql);
                return classes.ToList();
            }
        }

        public async Task InsertClassAsync(Class classModel)
        {
            var sql = "INSERT INTO Class (Title, Year) VALUES (@Title, @Year)";
            using(var connection = CreateConnection()) 
            {
                await connection.ExecuteAsync(sql, classModel);
            }
        }

        public async Task<Class> GetLatestAddedClass()
        {           
            using (var connection = CreateConnection()) 
            {
                var sql = "SELECT * FROM Class ORDER BY Id DESC LIMIT 1;";
                var oneClass = await connection.QuerySingleAsync<Class>(sql);
                return oneClass;
            }
        }
    }
}
