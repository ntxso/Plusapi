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
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/cart/{customerId}
        [HttpGet("{customerId}")]
        [Authorize(Roles = "admin, dealer")]
        public async Task<ActionResult<Cart>> GetCart(int customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Color)
                .Include(c => c.Items)
                    .ThenInclude(i => i.PhoneModel)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null)
            {
                cart = new Cart { CustomerId = customerId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        // POST: api/cart/add-item
        [HttpPost("add-item")]
        [Authorize(Roles = "dealer")]
        public async Task<IActionResult> AddToCart(CartItemDto dto)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == dto.CustomerId);

            if (cart == null)
            {
                cart = new Cart { CustomerId = dto.CustomerId };
                _context.Carts.Add(cart);
            }

            var existingItem = cart.Items.FirstOrDefault(i =>
                i.ProductId == dto.ProductId &&
                i.ColorId == dto.ColorId &&
                i.PhoneModelId == dto.PhoneModelId
            );

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = dto.ProductId,
                    ColorId = dto.ColorId,
                    PhoneModelId = dto.PhoneModelId,
                    Quantity = dto.Quantity
                });
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("items/update/{itemId}")]
        [Authorize(Roles = "dealer")]
        public async Task<IActionResult> UpdateCartItem(int itemId, [FromBody] UpdateCartItemDto dto)
        {
            var item = await _context.CartItems.FindAsync(itemId);
            if (item == null) return NotFound();

            item.Quantity = dto.Quantity;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/cart/remove-item/{cartItemId}
        [HttpPost("remove-item/{cartItemId}")]
        [Authorize(Roles = "dealer")]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item == null) return NotFound();

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/cart/clear/{customerId}
        [HttpPost("clear/{customerId}")]
        [Authorize(Roles = "admin,dealer")]
        public async Task<IActionResult> ClearCart(int customerId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (cart == null) return NotFound();

            _context.CartItems.RemoveRange(cart.Items);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public class UpdateCartItemDto
        {
            public int Quantity { get; set; }
        }
    }

}
