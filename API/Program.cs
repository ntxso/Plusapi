using API.Context;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<UserService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

//  Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var config = builder.Configuration;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();

//  Build uygulama
var app = builder.Build();

//  Veritabaný migration ve baþlangýç verisi
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Ýlk çalýþtýrmada test verisi oluþturmak istersen aktif et:
    
    if (!db.Categories.Any())
    {
        db.Categories.AddRange(new List<Category>
        {
            new Category { Name = "Telefon Kýlýfý" },
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

        // Admin ekle
        await userService.CreateUserAsync("admin", "138181", "admin");

        // Bayi ve bayi kullanýcýsý ekle
        var bayi = new Customer
        {
            Name = "Antalya Test",
            Title = "Test Aksesuar - Ali Tester",
            Address = "Muratpaþa mh 1111 sk Antalya",
            Phone = "123 456 45 45",
            Balance = 0
        };
        db.Customers.Add(bayi);
        await db.SaveChangesAsync();

        await userService.CreateUserAsync("bayi1", "1234", "dealer", bayi.Id);
    }
    

}

//  Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.UseAuthentication();  // Önce kimlik doðrulama
app.UseAuthorization();   // Sonra yetkilendirme

app.MapControllers();

app.Run();
