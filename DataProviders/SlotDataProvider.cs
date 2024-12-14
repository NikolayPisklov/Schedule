using Dapper;
using Schedule.Models;
using Schedule.Models.CombinedModels;

namespace Schedule.DataProviders
{
    public interface ISlotDataProvider 
    {
        Task InsertSlotAsync(TakenSlotInfo slotInfo);
        Task<IEnumerable<SlotInfo>> GetSlotsInfoForScheduleAsync(int year);
    }
    public class SlotDataProvider : DataProviderBase, ISlotDataProvider
    {
        public async Task<IEnumerable<SlotInfo>> GetSlotsInfoForScheduleAsync(int year)
        {
            using (var connection = CreateConnection())
            {
                var sql = @"SELECT FkClass AS ClassId, FkDay AS DayId, FkTime AS TimeId,
                    FullName, sub.Title AS SubjectTitle  FROM Slot s 
                    INNER JOIN Classroom cr ON s.FkClassroom = cr.Id
                    INNER JOIN SubjectToClass sc ON s.FkSubjectToClass = sc.Id
                    INNER JOIN TeacherSubject ts ON sc.FkTs = ts.Id
                    INNER JOIN Subject sub ON ts.FkSubject = sub.Id
                    INNER JOIN Teacher t ON ts.FkTeacher = t.Id
                    INNER JOIN Schedule sch ON sc.FkSchedule = sch.Id";
                var slots = await connection.QueryAsync<SlotInfo>(sql);
                return slots.ToList();
            }
        }

        public async Task InsertSlotAsync(TakenSlotInfo slotInfo)
        {
            using(var connection = CreateConnection()) 
            {
                var sql = @"INSERT INTO Slot (FkSubjectToClass, FkDay, FkTime, FkClassroom)
                    VALUES (@FkSubjectToClass, @FkDay, @FkTime, @FkClassroom)";
                await connection.ExecuteAsync(sql, new
                {
                    FkSubjectToClass = slotInfo.SubjectToClassId,
                    FkDay = slotInfo.DayId,
                    FkTime = slotInfo.TimeId,
                    FkClassroom = 1//temporary
                });
            }
        }
    }
}
