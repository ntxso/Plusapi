using API.Context;
using API.Models;
using API.Security;
using Azure.Core;
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

        public async Task<User> CreateUserAsync(string email, string password, string role, int? customerId = null)
        {
            var existing = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existing != null)
                throw new Exception("Bu kullanıcı adı zaten kullanılıyor");

            var user = new User
            {
                Email = email,
                PasswordHash = PasswordHasher.HashPassword(password),
                Role = role,
                CustomerId = customerId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.
                Include(u=>u.Customer)
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Customer)
           .FirstOrDefaultAsync(u => u.Id == id);

        }
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }
        public async Task<IEnumerable<User>> GetUsersByCustomerIdAsync(int customerId)
        {
            return await _context.Users
                                .Where(u => u.CustomerId == customerId)
                                .ToListAsync();
        }
    }

}
