using API.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("api/[controller]")]
public class TablesController : ControllerBase
{
    private readonly AppDbContext _context;

    public TablesController(AppDbContext context)
    {
        _context = context;
    }

    // 1. Tüm tablo isimlerini getir
    [HttpGet("tables")]
    public IActionResult GetTableNames()
    {
        try
        {
            var tableNames = _context.Model.GetEntityTypes()
                .Select(t => t.GetTableName())
                .Distinct()
                .ToList();

            return Ok(tableNames);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // 2. Tablo verilerini sayfalama ile getir
    [HttpGet("{tableName}/data")]
    public async Task<IActionResult> GetTableData(
    string tableName,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 100)
    {
        var query = $"SELECT * FROM [{tableName}] ORDER BY 1 OFFSET {(page - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY";

        switch (tableName.ToLower())
        {
            case "products":
                var products = await _context.Products.FromSqlRaw(query).ToListAsync();
                return Ok(products);
            case "categories":
                var categories = await _context.Categories.FromSqlRaw(query).ToListAsync();
                return Ok(categories);
            case "users":
                var users = await _context.Users.FromSqlRaw(query).ToListAsync();
                return Ok(users);
            case "productimages":
                var productImages = await _context.ProductImages.FromSqlRaw(query).ToListAsync();
                return Ok(productImages);
            case "customers":
                var customers = await _context.Customers.FromSqlRaw(query).ToListAsync();
                return Ok(customers);
            case "orders":
                var orders = await _context.Orders.FromSqlRaw(query).ToListAsync();
                return Ok(orders);

            default:
                return NotFound("Table not found");
        }
    }
}