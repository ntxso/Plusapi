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
    //db.Database.Migrate();

    // Ýlk çalýþtýrmada test verisi oluþturmak istersen aktif et:
    /*
    if (!db.Categories.Any())
    {
        db.Categories.AddRange(new List<Category>
        {
            new Category { Name = "char_value_1" },
            new Category { Name = "char_value_2" },
            new Category { Name = "char_value_3" }
        });

        await db.SaveChangesAsync();
    }

    var userService = new UserService(db);
    
    // Admin ekle
    await userService.CreateUserAsync("admin", "138181", "Admin");

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

    await userService.CreateUserAsync("bayi1", "1234", "Dealer", bayi.Id);
    */
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
