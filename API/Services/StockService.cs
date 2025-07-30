using API.Context;
using API.Models;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace API.Services
{
    public class StockService
    {
        private readonly AppDbContext _context;

        public StockService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<StockDto> GetStockById(int stockId)
        {
            var stock = await _context.Stocks
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == stockId);

            if (stock == null)
            {
                throw new KeyNotFoundException($"Stock with ID {stockId} not found");
            }

            return new StockDto
            {
                Id = stock.Id,
                Quantity = stock.Quantity,
                ColorName = stock.Color != null ? stock.Color.Name : null,
                PhoneModelName = stock.PhoneModel != null ? $"{stock.PhoneModel.Brand} {stock.PhoneModel.Model}" : null
            };
        }

        public async Task InitializeAllStocks(int defaultQuantity = 1000)
        {
            var products = await _context.Products.ToListAsync();

            foreach (var product in products)
            {
                var productColors = await _context.ProductColors
                    .Where(pc => pc.ProductId == product.Id)
                    .ToListAsync();

                var productModels = await _context.ProductPhoneModels
                    .Where(pm => pm.ProductId == product.Id)
                    .ToListAsync();

                bool hasColors = productColors.Any();
                bool hasModels = productModels.Any();

                if (!hasColors && !hasModels)
                {
                    // Sadece varyasyonsuz stok kaydı
                    if (!await _context.Stocks.AnyAsync(s => s.ProductId == product.Id &&
                                                          s.PhoneModelId == null &&
                                                          s.ColorId == null))
                    {
                        _context.Stocks.Add(new Stock
                        {
                            ProductId = product.Id,
                            Quantity = defaultQuantity,
                            LastUpdated = DateTime.UtcNow
                        });
                    }
                }
                else if (hasColors && !hasModels)
                {
                    // Sadece renk varyasyonları
                    foreach (var color in productColors)
                    {
                        if (!await _context.Stocks.AnyAsync(s => s.ProductId == product.Id &&
                                                               s.ColorId == color.ColorId &&
                                                               s.PhoneModelId == null))
                        {
                            _context.Stocks.Add(new Stock
                            {
                                ProductId = product.Id,
                                ColorId = color.ColorId,
                                Quantity = defaultQuantity,
                                LastUpdated = DateTime.UtcNow
                            });
                        }
                    }
                }
                else if (!hasColors && hasModels)
                {
                    // Sadece model varyasyonları
                    foreach (var model in productModels)
                    {
                        if (!await _context.Stocks.AnyAsync(s => s.ProductId == product.Id &&
                                                               s.PhoneModelId == model.PhoneModelId &&
                                                               s.ColorId == null))
                        {
                            _context.Stocks.Add(new Stock
                            {
                                ProductId = product.Id,
                                PhoneModelId = model.PhoneModelId,
                                Quantity = defaultQuantity,
                                LastUpdated = DateTime.UtcNow
                            });
                        }
                    }
                }
                else
                {
                    // Hem renk hem model varsa: sadece kombinasyonlar!
                    foreach (var color in productColors)
                    {
                        foreach (var model in productModels)
                        {
                            if (!await _context.Stocks.AnyAsync(s => s.ProductId == product.Id &&
                                                                   s.ColorId == color.ColorId &&
                                                                   s.PhoneModelId == model.PhoneModelId))
                            {
                                _context.Stocks.Add(new Stock
                                {
                                    ProductId = product.Id,
                                    ColorId = color.ColorId,
                                    PhoneModelId = model.PhoneModelId,
                                    Quantity = defaultQuantity,
                                    LastUpdated = DateTime.UtcNow
                                });
                            }
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
        }



        public async Task<List<StockDto>> GetStocksByProductId(int productId)
        {
            return await _context.Stocks
                .Where(s => s.ProductId == productId)
                .Include(s => s.Color)
                .Include(s => s.PhoneModel)
                .Select(s => new StockDto
                {
                    Id = s.Id,
                    Quantity = s.Quantity,
                    ColorName = s.Color != null ? s.Color.Name : null,
                    PhoneModelName = s.PhoneModel != null ? $"{s.PhoneModel.Brand} {s.PhoneModel.Model}" : null
                })
                .ToListAsync();
        }

        public async Task<StockDto> UpdateStockQuantity(int stockId, int newQuantity)
        {
            var stock = await _context.Stocks.FindAsync(stockId);
            if (stock == null)
            {
                throw new KeyNotFoundException("Stok bulunamadı");
            }

            stock.Quantity = newQuantity;
            stock.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new StockDto
            {
                Id = stock.Id,
                Quantity = stock.Quantity,
                ColorName = stock.Color != null ? stock.Color.Name : null,
                PhoneModelName = stock.PhoneModel != null ? $"{stock.PhoneModel.Brand} {stock.PhoneModel.Model}" : null
            };
        }



        public async Task DeleteStock(int stockId)
        {
            var stock = await _context.Stocks.FindAsync(stockId);
            if (stock == null)
            {
                throw new KeyNotFoundException("Stok bulunamadı");
            }

            _context.Stocks.Remove(stock);
            await _context.SaveChangesAsync();
        }
    }

}