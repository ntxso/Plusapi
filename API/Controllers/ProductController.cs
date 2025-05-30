using API.Context;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .Include(p => p.Tag)
                .Include(p => p.Images)
                .ToListAsync();
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stock)
                .Include(p => p.Tag)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return product;
        }
        // GET /api/products/search?code=abc&name=xyz&categoryId=3
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string? code, [FromQuery] string? name, [FromQuery] int? categoryId)
        {
            var query = _context.Products.Include(p => p.Category).Include(p =>p.Images).AsQueryable();

            if (!string.IsNullOrWhiteSpace(code))
                query = query.Where(p => p.Code.Contains(code));
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(p => p.Name.Contains(name));
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            var results = await query.ToListAsync();
            return Ok(results);
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(CreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description,
                Barcode = dto.Barcode,
                CategoryId = dto.CategoryId,
                Price = dto.Price,
                Publish = dto.Publish
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        // POST: api/products/bulk
        [HttpPost("bulk")]
        public async Task<ActionResult> CreateProducts([FromBody] List<CreateProductDto> products)
        {
            if (products == null || products.Count == 0)
                return BadRequest("Ürün listesi boş olamaz.");

            var productEntities = products.Select(dto => new Product
            {
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description,
                Barcode = dto.Barcode,
                CategoryId = dto.CategoryId,
                Price = dto.Price,
                Publish = dto.Publish
            }).ToList();

            _context.Products.AddRange(productEntities);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{productEntities.Count} ürün başarıyla eklendi." });
        }



        // PUT: api/products/5
        [HttpPost("Update/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            if (id != product.Id)
                return BadRequest();

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(p => p.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/products/5
        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Stock)
                .Include(p => p.Tag)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            if (product.Stock != null) _context.Stocks.Remove(product.Stock);
            if (product.Tag != null) _context.Tags.Remove(product.Tag);
            _context.ProductImages.RemoveRange(product.Images?.Any() == true ? product.Images : Enumerable.Empty<ProductImage>());
            _context.Products.Remove(product);

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
