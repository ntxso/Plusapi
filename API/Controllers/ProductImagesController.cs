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
    public class ProductImagesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinaryService;

        public ProductImagesController(AppDbContext context, CloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("upload")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UploadImage(IFormFile file, int productId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Dosya seçilmedi.");

            try
            {
                var (url, width, height, publicId) = await _cloudinaryService.UploadImageAsync(file);
                var image = new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = url,
                    Width = width,
                    Height = height,
                    FileSizeKb = (int)(file.Length / 1024),
                    PublicId = publicId
                };

                _context.ProductImages.Add(image);
                await _context.SaveChangesAsync();

                return Ok(image);
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Upload hatası: {ex.Message}");
            }
        }

        [HttpPost("Delete/{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _context.ProductImages.FindAsync(id);
            if (image == null)
                return NotFound("Resim bulunamadı.");

            try
            {
                // Cloudinary'den sil
                if (!string.IsNullOrEmpty(image.PublicId))
                {
                    await _cloudinaryService.DeleteImageAsync(image.PublicId);
                }

                // Veritabanından sil
                _context.ProductImages.Remove(image);
                await _context.SaveChangesAsync();

                return Ok("Resim başarıyla silindi.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Silme hatası: {ex.Message}");
            }
        }

        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllImages([FromQuery] int page = 1, [FromQuery] int pageSize = 30, [FromQuery] int? categoryId = null)
        {
            var query = _context.ProductImages
                .Include(p => p.Product)
                .ThenInclude(p => p.Category)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.Product != null && p.Product.CategoryId == categoryId.Value);
            }

            var totalCount = await query.CountAsync();

            var images = await query
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                data = images,
                total = totalCount,
                page,
                pageSize
            });
        }


    }

}
