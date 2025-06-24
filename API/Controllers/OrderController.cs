using API.Context;
using API.Models;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "dealer")]
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
        [Authorize(Roles = "admin, dealer")]
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

        // GET: api/order/5
        [HttpGet("order/{orderId}")]
        [Authorize(Roles = "admin, dealer")]
        public async Task<ActionResult<Order>> GetOrderById(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items!)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Items!)
                    .ThenInclude(i => i.Color)
                .Include(o => o.Items!)
                    .ThenInclude(i => i.PhoneModel)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            return order;
        }


        // GET: api/order/admin
        [HttpGet("admin")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<Order>>> GetAllOrdersForAdmin(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? customerId,
            [FromQuery] OrderStatus? status)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Color)
                .Include(o => o.Items)
                    .ThenInclude(i => i.PhoneModel)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(o => o.OrderDate >= startDate.Value);

            if (endDate.HasValue)
            {
                DateTime? endDateValue = endDate.Value.Date.AddDays(1).AddTicks(-100); // Gün sonuna kadar
                query = query.Where(o => o.OrderDate <= endDateValue);
            }

            if (customerId.HasValue)
                query = query.Where(o => o.CustomerId == customerId.Value);

            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);
            //else
                //query = query.Where(o => o.Status == OrderStatus.Pending); // varsayılan: hazırlanıyor

            var result = await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return result;
        }

        // POST: api/order/status/5
        [HttpPost("status/update/{orderId}")]
        [Authorize(Roles = "admin, dealer")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] OrderStatus newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound();

            order.Status = newStatus;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("delete/{orderId}")]
        [Authorize(Roles = "admin, dealer")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound();
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return NoContent();
        }


    }

}
