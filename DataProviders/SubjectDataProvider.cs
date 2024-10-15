using Dapper;
using Schedule.Models;

namespace Schedule.DataProviders
{
    public interface ISubjectDataProvider
    {
        Task<IEnumerable<Subject>?> GetAllAsync();
    }
    public class SubjectDataProvider : DataProviderBase, ISubjectDataProvider
    {
        public async Task<IEnumerable<Subject>?> GetAllAsync()
        {
            using (var connection = CreateConnection()) 
            {
                var sql = "SELECT * FROM Subject";
                var subjects = await connection.QueryAsync<Subject>(sql);
                return subjects.ToList();
            }
        }
    }
}
