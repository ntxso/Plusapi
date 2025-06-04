using API.Context;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using System.Text.Json.Serialization;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// ENV de�i�kenlerini y�kle (.env dosyas�ndan)
DotNetEnv.Env.Load();

// Microsoft yap�land�rma sistemi ile ENV ortam�n� al
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables(); // �ift alt tireyi ':' olarak ��z�mler

var config = builder.Configuration;

// JSON ayarlar�
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<UserService>();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

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

var app = builder.Build();

Console.WriteLine("==== ENVIRONMENT CONFIG DEBUG ====");
Console.WriteLine($"ASPNETCORE_ENVIRONMENT: {config["ASPNETCORE_ENVIRONMENT"]}");
Console.WriteLine($"ConnectionStrings:DefaultConnection: {config.GetConnectionString("DefaultConnection")}");
Console.WriteLine($"JWT_ISSUER: {config["JWT_ISSUER"]}");
Console.WriteLine($"JWT_AUDIENCE: {config["JWT_AUDIENCE"]}");
Console.WriteLine($"JWT_KEY: {config["JWT_KEY"]}");
Console.WriteLine($"Cloudinary:CloudName: {config["Cloudinary:CloudName"]}");
Console.WriteLine($"Cloudinary:ApiKey: {config["Cloudinary:ApiKey"]}");
Console.WriteLine($"Cloudinary:ApiSecret: {config["Cloudinary:ApiSecret"]}");
Console.WriteLine("==================================");

// Veritaban� migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.Categories.Any())
    {
        db.Categories.AddRange(new List<Category>
 {
     new Category { Name = "Plus Telefon K�l�f�" },
     new Category { Name = "Outlet Telefon K�l�f�" },
     new Category { Name = "Ekran Koruyucu" },
     new Category { Name = "�arj Cihaz�" },
     new Category { Name = "Kablo" },
     new Category { Name = "Kulakl�k" },
     new Category { Name = "Hoparl�r" },
     new Category { Name = "Telefon Tutucu" },
     new Category { Name = "Powerbank" },
     new Category { Name = "Batarya" },
     new Category { Name = "Airpods Aksesuar" },
     new Category { Name = "Tablet Aksesuar" },
     new Category { Name = "Saat Aksesuar" },
     new Category { Name = "Bilgisayar �r�nleri" },
     new Category { Name = "Bellek" },
     new Category { Name = "Kablo Koruyucu" },
     new Category { Name = "Di�er" }
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
            Address = "Muratpa�a mh 1111 sk Antalya",
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
app.MapControllers();
app.Run();
