using Schedule.Models;
using Dapper;

namespace Schedule.DataProviders
{
    public interface ILoginDataProvider 
    {
        User GetUserByLoginDetailsAsync(string login, string password);
    }
    public class LoginDataProvider : DataProviderBase, ILoginDataProvider
    {
        public User? GetUserByLoginDetailsAsync(string login, string password)
        {
            using(var connection = CreateConnection()) 
            {
                login = "Admin";
                password = "admin";
                var sql = "SELECT * FROM User WHERE Login = @Login AND Password = @Password";
                var user = connection.Query<User>(sql, 
                    new { Login = login, Password = password }).FirstOrDefault();
                return user;
            }
        }
    }
}
