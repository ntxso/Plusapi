using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Text;

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
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetLogs(int page = 1, int pageSize = 100, string? level = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var connectionString = _systemConfig.ConnectionString;
            var logs = new List<dynamic>();
            var total = 0; // Ayrı bir değişken olarak tanımla;

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();

                // 1. TOPLAM KAYIT SAYISINI AL
                var countQuery = new StringBuilder("SELECT COUNT(*) FROM Logs WHERE 1=1");

                // Filtreleri count sorgusuna da ekle
                if (!string.IsNullOrEmpty(level))
                {
                    countQuery.Append(" AND Level = @Level");
                }

                if (startDate.HasValue)
                {
                    countQuery.Append(" AND TimeStamp >= @StartDate");
                }

                if (endDate.HasValue)
                {
                    countQuery.Append(" AND TimeStamp <= @EndDate");
                }

                var countCommand = new SqlCommand(countQuery.ToString(), conn);

                // Parametreleri ekle
                if (!string.IsNullOrEmpty(level))
                {
                    countCommand.Parameters.AddWithValue("@Level", level);
                }

                if (startDate.HasValue)
                {
                    countCommand.Parameters.AddWithValue("@StartDate", startDate.Value);
                }

                if (endDate.HasValue)
                {
                    countCommand.Parameters.AddWithValue("@EndDate", endDate.Value);
                }

                // Toplam kayıt sayısını al
                total = (int)await countCommand.ExecuteScalarAsync();

                // 2. SAYFALANMIŞ LOG KAYITLARINI AL
                var query = new StringBuilder("SELECT * FROM Logs WHERE 1=1");

                // Filtreler
                if (!string.IsNullOrEmpty(level))
                {
                    query.Append(" AND Level = @Level");
                }

                if (startDate.HasValue)
                {
                    query.Append(" AND TimeStamp >= @StartDate");
                }

                if (endDate.HasValue)
                {
                    query.Append(" AND TimeStamp <= @EndDate");
                }

                // Sıralama ve sayfalama
                query.Append(" ORDER BY TimeStamp DESC");
                query.Append($" OFFSET {(page - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY");

                var command = new SqlCommand(query.ToString(), conn);

                // Parametreleri tekrar ekle (countCommand'dan farklı scope)
                if (!string.IsNullOrEmpty(level))
                {
                    command.Parameters.AddWithValue("@Level", level);
                }

                if (startDate.HasValue)
                {
                    command.Parameters.AddWithValue("@StartDate", startDate.Value);
                }

                if (endDate.HasValue)
                {
                    command.Parameters.AddWithValue("@EndDate", endDate.Value);
                }

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

            return Ok(new {logs,total});
        }

        [HttpPost("old")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteOldLogs(int months = 1)
        {
            var connectionString = _systemConfig.ConnectionString;
            var cutoffDate = DateTime.Now.AddMonths(-months);

            using (var conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync();
                var command = new SqlCommand("DELETE FROM Logs WHERE TimeStamp < @CutoffDate", conn);
                command.Parameters.AddWithValue("@CutoffDate", cutoffDate);

                var affectedRows = await command.ExecuteNonQueryAsync();
                return Ok(new { DeletedCount = affectedRows });
            }
        }
    }

}
