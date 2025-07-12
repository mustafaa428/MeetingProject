using Entities.Models;

namespace Services.Interface
{
    public interface IUserService
    {
        void AddUser(User user);
        User? GetUserByEmail(string email);
        void UpdateUser(User user);
        void DeleteUser(int id);
        User GetUserById(int id);
        void SoftDeleteUser(int id);
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string inputPassword);
    }
}
