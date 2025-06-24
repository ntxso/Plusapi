using API.Context;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductColorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductColorController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductColor>>> GetAll()
        {
            return await _context.ProductColors
                .Include(pc => pc.Product)
                .Include(pc => pc.Color)
                .ToListAsync();
        }

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<ProductColor>>> GetByProductId(int productId)
        {
            var colors = await _context.ProductColors
                .Where(x => x.ProductId == productId)
                .Include(pc => pc.Color)
                .ToListAsync();

            return Ok(colors ?? new List<ProductColor>());
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> Create(ProductColor model)
        {
            var exists = await _context.ProductColors
                .AnyAsync(x => x.ProductId == model.ProductId && x.ColorId == model.ColorId);

            if (exists)
                return Conflict("Bu renk zaten bu ürüne tanımlı.");

            _context.ProductColors.Add(model);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("delete")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete([FromQuery] int productId, [FromQuery] int colorId)
        {
            var relation = await _context.ProductColors
                .FirstOrDefaultAsync(x => x.ProductId == productId && x.ColorId == colorId);

            if (relation == null) return NotFound();

            _context.ProductColors.Remove(relation);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("remove")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Remove([FromBody] ProductColor model)
        {
            var relation = await _context.ProductColors
                .FirstOrDefaultAsync(x => x.ProductId == model.ProductId && x.ColorId == model.ColorId);

            if (relation == null) return NotFound();

            _context.ProductColors.Remove(relation);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

}
