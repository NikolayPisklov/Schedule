using Dapper;
using Schedule.Models;
using Schedule.Models.CombinedModels;

namespace Schedule.DataProviders
{
    public interface ISubjectToClassDataProvider 
    {
        Task InsertAsync(int fkSchedule, int fkTs, double hours);
        Task <IEnumerable<SubjectToClassInfo>?> GetAllForClassAsync(int fkClass);
        Task<SubjectToClassInfo> GetLatestSubjectToClassAsync(int fkClass);
        Task DeleteSubjectToClassAsync(int id);
        Task<List<ScheduleSubjectToClass>> GetAllSubjectToClassForAYear(int year);
    }
    public class SubjectToClassDataProvider : DataProviderBase, ISubjectToClassDataProvider
    {
        public async Task<List<ScheduleSubjectToClass>> GetAllSubjectToClassForAYear(int year) 
        {
            using (var connection = CreateConnection())
            {
                var sql = @"SELECT sc.Id, FkTs, FkSchedule, FkTeacher, Hours, DifficultCoefficient FROM SubjectToClass sc
                    INNER JOIN Schedule sch ON sc.FkSchedule = sch.Id
                    INNER JOIN TeacherSubject ts ON sc.FkTs = ts.Id
                    INNER JOIN Subject s ON ts.FkSubject = s.Id 
                    WHERE sch.Year = @Year";
                var subjectsToClass = await connection.QueryAsync<ScheduleSubjectToClass>(sql, new { Year = year });
                return subjectsToClass.ToList();
            }
        }
        public async Task InsertAsync(int fkSchedule, int fkTs, double hours)
        {
            var sql = "INSERT INTO SubjectToClass (FkTs, FkSchedule, Hours) VALUES (@FkTs, @FkSchedule, @Hours);";
            using (var connection = CreateConnection())
            {
                await connection.ExecuteAsync(sql, new { FkTs = fkTs, FkSchedule = fkSchedule, Hours = hours});
            }
        }
        public async Task<IEnumerable<SubjectToClassInfo>?> GetAllForClassAsync(int fkClass) 
        {
            using (var connection = CreateConnection()) 
            {
                var sql = @"SELECT sc.Id AS ScId, FullName, Title, Hours FROM SubjectToClass sc
                    INNER JOIN  TeacherSubject ts ON sc.FkTs = ts.Id
                    INNER JOIN Teacher t ON ts.FkTeacher = t.Id
                    INNER JOIN Subject s ON ts.FkSubject = s.Id 
                    INNER JOIN Schedule sch ON sc.FkSchedule = sch.Id
                    WHERE FkClass = @FkClass;";
                var subjectsToClass = await connection.QueryAsync<SubjectToClassInfo>(sql, new { FkClass = fkClass });
                return subjectsToClass.ToList();
            }
        }
        public async Task<SubjectToClassInfo> GetLatestSubjectToClassAsync(int fkClass) 
        {
            using (var connection = CreateConnection())
            {
                var sql = @"SELECT sc.Id AS ScId, FullName, Title, Hours FROM SubjectToClass sc
                    INNER JOIN  TeacherSubject ts ON sc.FkTs = ts.Id
                    INNER JOIN Teacher t ON ts.FkTeacher = t.Id
                    INNER JOIN Subject s ON ts.FkSubject = s.Id 
                    INNER JOIN Schedule sch ON sc.FkSchedule = sch.Id
                    WHERE FkClass = @FkClass
                    ORDER BY sc.Id DESC LIMIT 1;";
                var subjectToClass = await connection.QuerySingleAsync<SubjectToClassInfo>(sql, new {FkClass = fkClass});
                return subjectToClass;
            }
        }
        public async Task DeleteSubjectToClassAsync(int id) 
        {
            using (var connection = CreateConnection()) 
            {
                var sql = "DELETE FROM SubjectToClass WHERE Id = @Id";
                await connection.ExecuteAsync(sql, new {Id = id });
            }
        }
    }
}
