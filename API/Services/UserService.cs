using API.Context;
using API.Models;
using API.Security;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> CreateUserAsync(string username, string password, string role, int? customerId = null)
        {
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (existing != null)
                throw new Exception("Bu kullanıcı adı zaten kullanılıyor");

            var user = new User
            {
                Username = username,
                PasswordHash = PasswordHasher.HashPassword(password),
                Role = role,
                CustomerId = customerId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }
    }

}
