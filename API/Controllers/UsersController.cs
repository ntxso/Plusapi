using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "admin, dealer")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return user;
        }

        [HttpGet]
        [Authorize(Roles = "admin")] // Sadece admin yetkisi olanlar
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("by-customer/{customerId}")]
        [Authorize(Roles = "admin, dealer")] // Hem admin hem dealer erişebilir
        public async Task<ActionResult<IEnumerable<User>>> GetUsersByCustomerId(int customerId)
        {
            var users = await _userService.GetUsersByCustomerIdAsync(customerId);

            if (users == null || !users.Any())
            {
                return NotFound("Belirtilen CustomerId'ye ait kullanıcı bulunamadı");
            }

            return Ok(users);
        }
    }
}
