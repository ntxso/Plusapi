using API.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using DotNetEnv;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // .env dosyasını oku ve ortam değişkenlerine yükle
        DotNetEnv.Env.Load();

        // Microsoft config üzerinden ENV'leri çöz
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
        Console.WriteLine("----------" + configuration.GetConnectionString("DefaultConnection"));

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
