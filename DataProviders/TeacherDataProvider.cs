using Dapper;
using Schedule.Models;
using System.Windows.Controls.Primitives;

namespace Schedule.DataProviders
{
    public interface ITeacherDataProvider 
    {
        Task<IEnumerable<Teacher>?> GetAllTeachersAsync();
        Task InsertTeacherAsync(Teacher teacher);
        Task DeleteTeacherAsync(int id);
        Task UpdateTeacherAsync(Teacher teacher);
        Task<Teacher> GetLatestAddedTeacherAsync();
        Task<bool> CheckIfSlotForTeacherExistsAsync(int teacherId);
        Task<IEnumerable<Subject>?> GetTeachersSubjects(int id);
        Task InsertSubjectForTeacherAsync(TeacherSubject ts);
        Task<bool> CheckIfSlotForTeacherSubjectExistsAsync(int teacherId, int subjectId);
        Task<TeacherSubject> GetTeacherSubjectAsync(int teacherId, int subjectId);
        Task DeleteTeachersSubjectAsync(int id);
    }
    public class TeacherDataProvider : DataProviderBase, ITeacherDataProvider
    {
        public async Task<bool> CheckIfSlotForTeacherExistsAsync(int teacherId)
        {
            using (var connection = CreateConnection())
            {
                var sql = "SELECT Id FROM TeacherSubject WHERE Id = @Id";//wrong
                int teacherSubjectId = await connection.ExecuteScalarAsync<int>(sql, new {Id = teacherId });
                sql = "SELECT EXISTS(SELECT 1 FROM Slot WHERE FkTeacherSubject = @FkTeacherSubject);";
                bool isExists = await connection.ExecuteScalarAsync<bool>(sql, 
                    new { FkTeacherSubject = teacherSubjectId });
                return isExists;
            }
        }

        public async Task DeleteTeacherAsync(int id)
        {
            using (var connection = CreateConnection())
            {
                var sql = "DELETE FROM Teacher Where Id = @Id";
                await connection.ExecuteAsync(sql, new { Id = id });
            }
        }

        public async Task<IEnumerable<Teacher>?> GetAllTeachersAsync()
        {
            using (var connection = CreateConnection())
            {
                var sql = "SELECT Id, FullName FROM Teacher;";
                var teachers = await connection.QueryAsync<Teacher>(sql);
                return teachers.ToList();
            }
        }

        public async Task<Teacher> GetLatestAddedTeacherAsync()
        {
            using (var connection = CreateConnection()) 
            {
                var sql = "SELECT * FROM Teacher ORDER BY Id DESC LIMIT 1;";
                var teacher = await connection.QuerySingleAsync<Teacher>(sql);
                return teacher;
            }
        }

        public async Task InsertTeacherAsync(Teacher teacher)
        {
            var sql = "INSERT INTO Teacher (FullName) VALUES (@FullName)";
            using (var connection = CreateConnection())
            {
                await connection.ExecuteAsync(sql, teacher);
            }
        }

        public async Task UpdateTeacherAsync(Teacher teacher)
        {
            using (var connection = CreateConnection())
            {
                var sql = "UPDATE Class SET FullName = @FullName WHERE Id = @Id";
                await connection.ExecuteAsync(sql, new
                {
                    FullName = teacher.FullName
                });
            }
        }

        public async Task<IEnumerable<Subject>?> GetTeachersSubjects(int id) 
        {
            using (var connection = CreateConnection()) 
            {
                var sql = @"
                    SELECT s.Id, s.Title 
                    FROM Subject s
                    INNER JOIN TeacherSubject t ON t.FkSubject = s.Id
                    WHERE t.FkTeacher = @TeacherId";
                var subjects = await connection.QueryAsync<Subject>(sql, new { TeacherId = id});
                return subjects.ToList();
            }
        }
        

        public async Task<bool> CheckIfSlotForTeacherSubjectExistsAsync(int teacherId, int subjectId)
        {
            using (var connection = CreateConnection())
            {
                var sql = "SELECT Id FROM TeacherSubject WHERE FkTeacher = @FkTeacher AND FkSubject = @FkSubject";
                int teacherSubjectId = await connection.ExecuteScalarAsync<int>(sql, new { FkTeacher = teacherId, FkSubject = subjectId });
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
                var sql = @"SELECT * FROM TeacherSubject WHERE FkTeacher = @FkTeacher AND FkSubject = @FkSubject";
                var ts = await connection.QuerySingleAsync<TeacherSubject>(sql, new { FkTeacher = teacherId, FkSubject = subjectId });
                return ts;
            }
        }

        public async Task InsertSubjectForTeacherAsync(TeacherSubject ts)
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
