using API.Context;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(AppDbContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Stocks)
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
                .Include(p => p.Stocks)
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
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p =>p.Images)
                .Include(p=>p.Tag)
                .AsQueryable();

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
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<Product>> CreateProduct(CreateProductDto dto)
        {
            //var user = User.Identity?.Name ?? "Anonymous";
            //_logger.LogInformation($"User {user} is requesting product with ID {user}");
            //_logger.LogInformation("Kullanıcı {user} sepete ürün ekledi: {@Product}", user, dto);
            _logger.LogWarning("otomatik kullanıcı bilgisi gelecek mi ürün eklendi");
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
        [Authorize(Roles = "admin")]
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
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            if (id != product.Id)
                return BadRequest();

            // Mevcut ürünü ve ilişkili tag'ini veritabanından çekiyoruz
            var existingProduct = await _context.Products
                .Include(p => p.Tag) // Tag'ı eager loading ile yüklüyoruz
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingProduct == null)
                return NotFound();

            // Ana ürün bilgilerini güncelle
            _context.Entry(existingProduct).CurrentValues.SetValues(product);

            // Tag işlemleri
            if (product.Tag != null && !string.IsNullOrWhiteSpace(product.Tag.Value))
            {
                if (existingProduct.Tag != null)
                {
                    // Varolan tag'i güncelle
                    existingProduct.Tag.Value = product.Tag.Value;
                }
                else
                {
                    // Yeni tag oluştur
                    existingProduct.Tag = new Tag
                    {
                        ProductId = product.Id,
                        Value = product.Tag.Value
                    };
                    _context.Tags.Add(existingProduct.Tag);
                }
            }
            else if (existingProduct.Tag != null)
            {
                // Gelen tag null/boş ama veritabanında tag varsa sil
                _context.Tags.Remove(existingProduct.Tag);
            }

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


        [HttpPost("UpdatePrice/{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateProductPrice(int id, [FromBody] UpdateProductPriceDto productPrice)
        {
           

            // Mevcut ürünü ve ilişkili tag'ini veritabanından çekiyoruz
            var existingProduct = await _context.Products
                //.Include(p => p.Tag) // Tag'ı eager loading ile yüklüyoruz
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingProduct == null)
                return NotFound();

            // Ana ürün bilgilerini güncelle
            existingProduct.Price = productPrice.Price;
            _context.Entry(existingProduct).CurrentValues.SetValues(existingProduct);

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
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Stocks)
                .Include(p => p.Tag)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            if (product.Stocks != null) _context.Stocks.RemoveRange(product.Stocks);
            if (product.Tag != null) _context.Tags.Remove(product.Tag);
            _context.ProductImages.RemoveRange(product.Images?.Any() == true ? product.Images : Enumerable.Empty<ProductImage>());
            _context.Products.Remove(product);

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class UpdateProductPriceDto
    {
        public decimal Price { get; set; }
    }
}
