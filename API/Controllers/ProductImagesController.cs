using API.Context;
using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> UploadImage(IFormFile file, int productId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Dosya seçilmedi.");

            try
            {
                var (url, width, height) = await _cloudinaryService.UploadImageAsync(file);
                var image = new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = url,
                    Width = width,
                    Height = height,
                    FileSizeKb = (int)(file.Length / 1024)
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
    }

}
