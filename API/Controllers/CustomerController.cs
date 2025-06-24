using API.Context;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace API.Controllers
{
    // CustomersController.cs
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await _context.Customers.ToListAsync();
        }

        [HttpGet("search")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<Customer>>> Search([FromQuery] string? name, [FromQuery] string? companyName)
        {
            var query = _context.Customers.AsQueryable();
            if (!string.IsNullOrEmpty(name))
                query = query.Where(c => c.Name.Contains(name));
            if (!string.IsNullOrEmpty(companyName))
                query = query.Where(c => c.CompanyName.Contains(companyName));

            return await query.ToListAsync();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "admin, dealer")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            return customer;
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<Customer>> CreateCustomer(CreateCustomerDto dto)
        {
            var customer = new Customer
            {
                Name = dto.Name,
                CompanyName = dto.CompanyName,
                Phone = dto.Phone,
                CityId = dto.CityId,
                DistrictId = dto.DistrictId,
                Address = dto.Address,
                SalesType = dto.SalesType,
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
        }

        [HttpPost("Update/{id}")]
        [Authorize(Roles = "admin, dealer")]
        public async Task<IActionResult> UpdateCustomer(int id, CreateCustomerDto customerDto)
        {
            Console.WriteLine($"müşteri:{JsonConvert.SerializeObject(customerDto)}");
            if (id != customerDto.Id) return BadRequest();
            var existingCustomer = await _context.Customers.FindAsync(id);
            if (existingCustomer == null)
            {
                Console.WriteLine($"Hata: Müşteri bulunamadı. ID: {id}");
                return NotFound($"ID'si {id} olan müşteri bulunamadı.");
            }

            existingCustomer.Name = customerDto.Name;
            existingCustomer.Phone = customerDto.Phone;
            existingCustomer.Address = customerDto.Address;
            existingCustomer.CompanyName= customerDto.CompanyName;
            //_context.Entry(customer).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("Delete/{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
