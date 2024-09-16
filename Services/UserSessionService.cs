namespace Schedule.Services
{
    public class UserSessionService
    {
        public UserStatus Status { get; } = UserStatus.Unauthorized;
        public int Id { get; }
        public int FkStatus { get; }
        public string? Name { get; }
        public string? Login { get; }
        public string? Email { get;}

        public UserSessionService(int id, int fkStatus, string name, 
            string login, string email, UserStatus status)
        {
            Id = id;
            FkStatus = fkStatus;
            Name = name;
            Login = login;
            Email = email;
            Status = status;
        }
        public UserSessionService() { }
        public enum UserStatus
        {
            Unauthorized,
            Admin,
            Teacher,
            Student
        }
    }
}
