using API.Context;
using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/order
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var cart = await _context.Carts
                .Include(c => c.Items).
                ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == dto.CustomerId);

            if (cart == null || !cart.Items.Any())
                return BadRequest("Sepet boş.");

            var order = new Order
            {
                CustomerId = dto.CustomerId,
                OrderDate = DateTime.UtcNow,
                Items = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    ColorId = i.ColorId,
                    PhoneModelId = i.PhoneModelId,
                    Quantity = i.Quantity,
                    UnitPrice = i.Product?.Price ?? 0 // NULL kontrolü yapılabilir
                }).ToList()
            };
            // Toplam sipariş tutarını hesapla
            order.TotalAmount = order.Items.Sum(item => item.TotalPrice);

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cart.Items); // Sepeti temizle
            await _context.SaveChangesAsync();

            return Ok(order);
        }

        // GET: api/order/customer/5
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByCustomer(int customerId)
        {
            return await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Color)
                .Include(o => o.Items)
                    .ThenInclude(i => i.PhoneModel)
                .ToListAsync();
        }
    }

}
