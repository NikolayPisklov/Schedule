using Dapper;
using Schedule.Models;

namespace Schedule.DataProviders
{
    public interface ITeacherDataProvider 
    {
        Task<IEnumerable<Teacher>?> GetAllTeachersAsync();
        Task InsertTeacherAsync(Teacher teacher);
        Task DeleteTeacherAndAttachedSubjectsAsync(int id);
        Task UpdateTeacherAsync(Teacher teacher);
        Task<Teacher> GetLatestAddedTeacherAsync();
        Task<bool> IsSlotForTeacherExistsAsync(int teacherId);
        
    }
    public class TeacherDataProvider : DataProviderBase, ITeacherDataProvider
    {
        public async Task<bool> IsSlotForTeacherExistsAsync(int teacherId)
        {
            using (var connection = CreateConnection())
            {
                var sql = "SELECT Id FROM TeacherSubject WHERE Id = @Id";
                int teacherSubjectId = await connection.ExecuteScalarAsync<int>(sql, new {Id = teacherId });
                sql = "SELECT EXISTS(SELECT 1 FROM Slot WHERE FkTeacherSubject = @FkTeacherSubject);";
                bool isExists = await connection.ExecuteScalarAsync<bool>(sql, 
                    new { FkTeacherSubject = teacherSubjectId });
                return isExists;
            }
        }

        public async Task DeleteTeacherAndAttachedSubjectsAsync(int id)
        {
            using (var connection = CreateConnection())
            {
                var sql = "DELETE FROM TeacherSubject Where FkTeacher = @Id";
                await connection.ExecuteAsync(sql, new { Id = id });
                sql = "DELETE FROM Teacher Where Id = @Id";
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
                var sql = "UPDATE Teacher SET FullName = @FullName WHERE Id = @Id";
                await connection.ExecuteAsync(sql, new
                {
                    FullName = teacher.FullName,
                    Id = teacher.Id
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
    }
}
