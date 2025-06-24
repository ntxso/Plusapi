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
    public class ProductPhoneModelController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductPhoneModelController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductPhoneModel>>> GetAll()
        {
            return await _context.ProductPhoneModels
                .Include(p => p.Product)
                .Include(p => p.PhoneModel)
                .ToListAsync();
        }
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<ProductPhoneModel>>> GetByProductId(int productId)
        {
            var models = await _context.ProductPhoneModels
                .Where(x => x.ProductId == productId)
                .Include(p => p.PhoneModel)
                .ToListAsync();
            return Ok(models ?? new List<ProductPhoneModel>());
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> Create(ProductPhoneModel model)
        {
            // İlişki zaten varsa, tekrar eklenmesin
            var exists = await _context.ProductPhoneModels
                .AnyAsync(x => x.ProductId == model.ProductId && x.PhoneModelId == model.PhoneModelId);

            if (exists)
                return Conflict("Bu eşleşme zaten mevcut.");

            _context.ProductPhoneModels.Add(model);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("delete")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete([FromQuery] int productId, [FromQuery] int phoneModelId)
        {
            var relation = await _context.ProductPhoneModels
                .FirstOrDefaultAsync(x => x.ProductId == productId && x.PhoneModelId == phoneModelId);

            if (relation == null) return NotFound();

            _context.ProductPhoneModels.Remove(relation);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("remove")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Remove([FromBody] ProductPhoneModel model)
        {
            var relation = await _context.ProductPhoneModels
                .FirstOrDefaultAsync(x => x.ProductId == model.ProductId && x.PhoneModelId == model.PhoneModelId);

            if (relation == null) return NotFound();

            _context.ProductPhoneModels.Remove(relation);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

}
