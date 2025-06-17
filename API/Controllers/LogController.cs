using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly SystemSettings _systemConfig;

        public LogsController(IConfiguration config, SystemSettings system)
        {
            _config = config;
            _systemConfig = system;
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs()
        {

            var connectionString = _systemConfig.ConnectionString;
            var logs = new List<dynamic>();

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var command = new SqlCommand("SELECT TOP 100 * FROM Logs ORDER BY TimeStamp DESC", conn);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    logs.Add(new
                    {
                        TimeStamp = reader["TimeStamp"].ToString(),
                        Level = reader["Level"].ToString(),
                        Message = reader["Message"].ToString(),
                        UserId = reader["UserId"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        Role = reader["Role"]?.ToString()
                    });
                }
            }

            return Ok(logs);
        }
    }

}
