using DataBusiness.Interface;
using Entities.Models;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Services.Services
{
    public class UserService : IUserService
    {
        IRepository<User> _userRepository;

        public UserService(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public void AddUser(User user)
        {
            _userRepository.Add(user);
        }

        public User GetUserById(int id)
        {
            return _userRepository.GetById(id);
        }

        public User? GetUserByEmail(string email)
        {
            return _userRepository.GetAll().FirstOrDefault(u => u.Email == email);
        }

        public void UpdateUser(User user)
        {
            _userRepository.Update(user);
        }

        public void DeleteUser(int id)
        {
            _userRepository.Delete(id);
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _userRepository.GetAll();
        }

        // Yeni: Soft Delete metodu
        public void SoftDeleteUser(int id)
        {
            var user = _userRepository.GetById(id);
            if (user == null)
                throw new InvalidOperationException($"Id'si {id} olan kullanıcı bulunamadı.");

            user.IsDeleted = true;
            _userRepository.Update(user);
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public bool VerifyPassword(string hashedPassword, string inputPassword)
        {
            var inputPasswordHash = HashPassword(inputPassword);
            return hashedPassword == inputPasswordHash;
        }
    }
}
