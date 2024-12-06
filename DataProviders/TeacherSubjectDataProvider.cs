using Dapper;
using Schedule.Models;
using Schedule.Models.CombinedModels;

namespace Schedule.DataProviders
{
    public interface ITeacherSubjectDataProvider 
    {
        Task<IEnumerable<Subject>?> GetSubjectsForTeacherAsync(int id);
        Task InsertTeacherSubjectAsync(TeacherSubject ts);
        Task<bool> IsSlotForTeacherSubjectExistsAsync(int teacherId, int subjectId);
        Task<TeacherSubject> GetTeacherSubjectAsync(int teacherId, int subjectId);
        Task DeleteTeachersSubjectAsync(int id);
        Task<IEnumerable<TeacherSubjectInfo>> GetAllTeachersSubjectsAsync();
    }
    public class TeacherSubjectDataProvider : DataProviderBase, ITeacherSubjectDataProvider
    {
        public async Task<IEnumerable<TeacherSubjectInfo>> GetAllTeachersSubjectsAsync() 
        {
            using (var connection = CreateConnection()) 
            {
                var sql = @"SELECT ts.Id, FkTeacher, FkSubject, Title AS SubjectTitle, FullName AS TeachersName
                          FROM TeacherSubject ts INNER JOIN Teacher t ON ts.FkTeacher = t.Id
                          INNER JOIN Subject s ON s.Id = ts.FkSubject ORDER BY s.Title ASC; ";
                var tsList = await connection.QueryAsync<TeacherSubjectInfo>(sql);
                return tsList.ToList();
            }
        }
        public async Task<IEnumerable<Subject>?> GetSubjectsForTeacherAsync(int id)
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    SELECT s.Id, s.Title 
                    FROM Subject s
                    INNER JOIN TeacherSubject t ON t.FkSubject = s.Id
                    WHERE t.FkTeacher = @TeacherId";
                var subjects = await connection.QueryAsync<Subject>(sql, new { TeacherId = id });
                return subjects.ToList();
            }
        }
        public async Task<bool> IsSlotForTeacherSubjectExistsAsync(int teacherId, int subjectId)
        {
            using (var connection = CreateConnection())
            {
                var sql = @"SELECT Id FROM TeacherSubject 
                            WHERE FkTeacher = @FkTeacher 
                            AND FkSubject = @FkSubject";
                int teacherSubjectId = await connection.ExecuteScalarAsync<int>(sql, 
                    new { FkTeacher = teacherId, FkSubject = subjectId });
                sql = "SELECT EXISTS(SELECT 1 FROM Slot WHERE FkTeacherSubject = @FkTeacherSubject);";
                bool isExists = await connection.ExecuteScalarAsync<bool>(sql,
                    new { FkTeacherSubject = teacherSubjectId });
                return isExists;
            }
        }
        public async Task<TeacherSubject> GetTeacherSubjectAsync(int teacherId, int subjectId)
        {
            using (var connection = CreateConnection())
            {
                var sql = @"SELECT * FROM TeacherSubject 
                    WHERE FkTeacher = @FkTeacher AND FkSubject = @FkSubject";
                var ts = await connection.QuerySingleAsync<TeacherSubject>(sql, 
                    new { FkTeacher = teacherId, FkSubject = subjectId });
                return ts;
            }
        }

        public async Task InsertTeacherSubjectAsync(TeacherSubject ts)
        {
            using (var connection = CreateConnection())
            {
                var sql = "INSERT INTO TeacherSubject (FkTeacher, FkSubject) VALUES (@FkTeacher, @FkSubject)";
                await connection.ExecuteAsync(sql, ts);
            }
        }

        public async Task DeleteTeachersSubjectAsync(int id)
        {
            using (var connection = CreateConnection())
            {
                var sql = "DELETE FROM TeacherSubject Where Id = @Id";
                await connection.ExecuteAsync(sql, new { Id = id });
            }
        }
    }
}
