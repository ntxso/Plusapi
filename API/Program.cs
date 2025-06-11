using API.Context;
using API.Models;
using API.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using StackExchange.Redis;
using System.Text;
using System.Text.Json.Serialization;
using static Serilog.Sinks.MSSqlServer.ColumnOptions;

static ColumnOptions GetSqlColumnOptions()
{
    var columnOptions = new ColumnOptions();

    columnOptions.Store.Remove(StandardColumn.Properties);
    columnOptions.Store.Add(StandardColumn.LogEvent);

    columnOptions.AdditionalColumns = new List<SqlColumn>
    {
        new SqlColumn { ColumnName = "UserId", DataType = System.Data.SqlDbType.NVarChar, DataLength = 50 },
        new SqlColumn { ColumnName = "Email", DataType = System.Data.SqlDbType.NVarChar, DataLength = 100 },
        new SqlColumn { ColumnName = "Role", DataType = System.Data.SqlDbType.NVarChar, DataLength = 50 }
    };

    return columnOptions;
}


var builder = WebApplication.CreateBuilder(args);


var isDevelopment = builder.Environment.IsDevelopment();
//var connectionString = isDevelopment
//    ? Environment.GetEnvironmentVariable("CONNECTION_STRING_DEVELOPMENT")
//    : Environment.GetEnvironmentVariable("CONNECTION_STRING_PRODUCTION");

// ENV deðiþkenlerini yükle (.env dosyasýndan)
DotNetEnv.Env.Load();
builder.WebHost.UseSetting("detailedErrors", "true");
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables(); // Çift alt tireyi ':' olarak çözümler


try
{
    Log.Information("Uygulama baþlatýlýyor.");
    var config = builder.Configuration;

    // JSON ayarlarý
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

    builder.Services.AddScoped<TokenService>();
    builder.Services.AddScoped<UserService>();
    if (isDevelopment)
    {
        Console.WriteLine("----- " + config.GetConnectionString("Development"));
    }
    else
    {
        Console.WriteLine("----- " + config.GetConnectionString("Production"));
    }
    // DbContext
    var _conString = isDevelopment
        ? config.GetConnectionString("Development")
        : config.GetConnectionString("Production");

    Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.MSSqlServer(
        connectionString: _conString, // yukarýdan gelen uygun baðlantý dizesi
        sinkOptions: new MSSqlServerSinkOptions
        {
            TableName = "Logs",
            AutoCreateSqlTable = true
        },
        restrictedToMinimumLevel: LogEventLevel.Information,
        columnOptions: GetSqlColumnOptions()
    )
    .CreateLogger();

    builder.Host.UseSerilog();

    builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(_conString));

    builder.Services.AddSingleton(new SystemSettings
    {
        ConnectionString = _conString ?? "baðlantý dizesi okunamadý",
        DefaultConnection = _conString ?? "okunamadý !"
    });

    builder.Services.AddSingleton<CloudinaryService>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        });
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // JWT Authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["JWT_ISSUER"],
                ValidAudience = config["JWT_AUDIENCE"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT_KEY"]))
            };
        });

    builder.Services.AddAuthorization();
    Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
    var app = builder.Build();

   

    Console.WriteLine("==== ENVIRONMENT CONFIG DEBUG ====");
    Console.WriteLine($"ASPNETCORE_ENVIRONMENT: {config["ASPNETCORE_ENVIRONMENT"]}");
    Console.WriteLine($"ConnectionStrings:DefaultConnection: {config.GetConnectionString("DefaultConnection")}");


    // Veritabaný migration
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();

        if (!db.Categories.Any())
        {
            db.Categories.AddRange(new List<Category>
 {
     new Category { Name = "Plus Telefon Kýlýfý" },
     new Category { Name = "Outlet Telefon Kýlýfý" },
     new Category { Name = "Ekran Koruyucu" },
     new Category { Name = "Þarj Cihazý" },
     new Category { Name = "Kablo" },
     new Category { Name = "Kulaklýk" },
     new Category { Name = "Hoparlör" },
     new Category { Name = "Telefon Tutucu" },
     new Category { Name = "Powerbank" },
     new Category { Name = "Batarya" },
     new Category { Name = "Airpods Aksesuar" },
     new Category { Name = "Tablet Aksesuar" },
     new Category { Name = "Saat Aksesuar" },
     new Category { Name = "Bilgisayar Ürünleri" },
     new Category { Name = "Bellek" },
     new Category { Name = "Kablo Koruyucu" },
     new Category { Name = "Diðer" }
 });
            await db.SaveChangesAsync();
        }

        if (!db.Users.Any())
        {
            var userService = new UserService(db);
            await userService.CreateUserAsync("admin", "138181", "admin");

            var bayi = new Customer
            {
                Name = "Antalya Test",
                CompanyName = "Test Aksesuar - Ali Tester",
                Address = "Muratpaþa mh 1111 sk Antalya",
                Phone = "123 456 45 45",
                Balance = 0
            };

            db.Customers.Add(bayi);
            await db.SaveChangesAsync();
            await userService.CreateUserAsync("bayi1", "1234", "dealer", bayi.Id);
        }
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("AllowAll");
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseMiddleware<UserLoggingMiddleware>();
    app.UseSerilogRequestLogging(); // HTTP isteklerini otomatik loglar

    app.MapControllers();
    Log.Information("Serilog test mesajý.");
    app.Run();


}
catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama baþlatýlýlýrken ");

}
finally
{
    Log.CloseAndFlush();
}
