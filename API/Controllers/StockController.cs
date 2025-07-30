using API.Context;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly StockService _stockService;
        private readonly AppDbContext _context;

        public StockController(StockService stockService,AppDbContext context)
        {
            _stockService = stockService;
            _context = context;
        }

        [HttpPost("sync-stocks/{productId}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> SyncStocksWithProduct(int productId)
        {
            try
            {
                // Ürüne ait renk ve modelleri al
                var productColors = await _context.ProductColors
                    .Where(pc => pc.ProductId == productId)
                    .Select(pc => pc.ColorId)
                    .ToListAsync();

                var productModels = await _context.ProductPhoneModels
                    .Where(pm => pm.ProductId == productId)
                    .Select(pm => pm.PhoneModelId)
                    .ToListAsync();

                // Ürüne ait tüm stokları al
                var existingStocks = await _context.Stocks
                    .Where(s => s.ProductId == productId)
                    .ToListAsync();

                // 1. Silinecek stokları belirle ve sil
                var stocksToDelete = existingStocks.Where(stock =>
                {
                    // Genel stok kaydını koru (her ikisi de nullsa)
                    if (!stock.ColorId.HasValue && !stock.PhoneModelId.HasValue)
                        return false;
                    // Renk kontrolü
                    var hasColor = !stock.ColorId.HasValue || productColors.Contains(stock.ColorId.Value);

                    // Model kontrolü
                    var hasModel = !stock.PhoneModelId.HasValue || productModels.Contains(stock.PhoneModelId.Value);

                    return !(hasColor && hasModel);
                }).ToList();

                foreach (var stock in stocksToDelete)
                {
                    _context.Stocks.Remove(stock);
                }

                // 2. Eklenecek yeni stokları belirle
                var stocksToAdd = new List<Stock>();
                var defaultQuantity = 100; // Varsayılan stok miktarı

                // Senaryo 1: Sadece renk varyasyonları varsa
                if (productColors.Any() && !productModels.Any())
                {
                    foreach (var colorId in productColors)
                    {
                        // Bu renk için stok kaydı var mı kontrol et
                        if (!existingStocks.Any(s => s.ColorId == colorId && !s.PhoneModelId.HasValue))
                        {
                            stocksToAdd.Add(new Stock
                            {
                                ProductId = productId,
                                ColorId = colorId,
                                PhoneModelId = null,
                                Quantity = defaultQuantity,
                                LastUpdated = DateTime.UtcNow
                            });
                        }
                    }
                }
                // Senaryo 2: Sadece model varyasyonları varsa
                else if (!productColors.Any() && productModels.Any())
                {
                    foreach (var modelId in productModels)
                    {
                        // Bu model için stok kaydı var mı kontrol et
                        if (!existingStocks.Any(s => s.PhoneModelId == modelId && !s.ColorId.HasValue))
                        {
                            stocksToAdd.Add(new Stock
                            {
                                ProductId = productId,
                                ColorId = null,
                                PhoneModelId = modelId,
                                Quantity = defaultQuantity,
                                LastUpdated = DateTime.UtcNow
                            });
                        }
                    }
                }
                // Senaryo 3: Hem renk hem model varyasyonları varsa
                else if (productColors.Any() && productModels.Any())
                {
                    foreach (var colorId in productColors)
                    {
                        foreach (var modelId in productModels)
                        {
                            // Bu renk-model kombinasyonu için stok kaydı var mı kontrol et
                            if (!existingStocks.Any(s => s.ColorId == colorId && s.PhoneModelId == modelId))
                            {
                                stocksToAdd.Add(new Stock
                                {
                                    ProductId = productId,
                                    ColorId = colorId,
                                    PhoneModelId = modelId,
                                    Quantity = defaultQuantity,
                                    LastUpdated = DateTime.UtcNow
                                });
                            }
                        }
                    }
                }
                // Senaryo 4: Varyasyon yoksa (genel stok)
                else if (!productColors.Any() && !productModels.Any())
                {
                    // Genel stok kaydı var mı kontrol et
                    if (!existingStocks.Any(s => !s.ColorId.HasValue && !s.PhoneModelId.HasValue))
                    {
                        stocksToAdd.Add(new Stock
                        {
                            ProductId = productId,
                            ColorId = null,
                            PhoneModelId = null,
                            Quantity = defaultQuantity,
                            LastUpdated = DateTime.UtcNow
                        });
                    }
                }

                // Yeni stokları ekle
                if (stocksToAdd.Any())
                {
                    await _context.Stocks.AddRangeAsync(stocksToAdd);
                }

                // Değişiklikleri kaydet
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    DeletedCount = stocksToDelete.Count,
                    AddedCount = stocksToAdd.Count,
                    Message = $"{stocksToDelete.Count} stok kaydı silindi, {stocksToAdd.Count} yeni stok kaydı eklendi."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Hata: " + ex.Message });
            }
        }

        [HttpPost("initialize")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> InitializeStocks([FromQuery] int quantity = 1000)
        {
            try
            {
                await _stockService.InitializeAllStocks(quantity);
                return Ok(new { message = $"Tüm stoklar {quantity} adet olarak ayarlandı." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("by-product/{productId}")]
        [Authorize(Roles = "admin,editor")]
        public async Task<IActionResult> GetStocksByProductId(int productId)
        {
            try
            {
                var stocks = await _stockService.GetStocksByProductId(productId);
                if (stocks == null || !stocks.Any())
                {
                    return NotFound(new { message = "Stok bulunamadı." });
                }

                return Ok(stocks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        //Dto olmayan entity modeli döndürür
        [HttpGet("by-product2/{productId}")]
        [Authorize(Roles = "admin,dealer")]
        public async Task<IActionResult> GetStocksByProductId2(int productId)
        {
            try
            {
                var stocks = await _context.Stocks.Where(p => p.ProductId == productId)
                    .ToListAsync();
                if (stocks == null || !stocks.Any())
                {
                    return NotFound(new { message = "Stok bulunamadı." });
                }

                return Ok(stocks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("{stockId}")]
        [Authorize(Roles = "admin,dealer")]
        public async Task<IActionResult> UpdateStockQuantity(int stockId, [FromBody] UpdateStockQuantityDto updateDto)
        {
            try
            {
                // DTO validasyonu
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Stok varlığını kontrol et
                var existingStock = await _stockService.GetStockById(stockId);
                if (existingStock == null)
                {
                    return NotFound(new { message = "Belirtilen stok kaydı bulunamadı." });
                }

                // Stok miktarını güncelle
                var updatedStock = await _stockService.UpdateStockQuantity(stockId, updateDto.Quantity);

                return Ok(updatedStock);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

    }




}
