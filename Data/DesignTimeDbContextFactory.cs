using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CyberPetApp.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // 1. Read environment variable first
        var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        // 2. If not in environment, build configuration to read from appsettings.json
        if (string.IsNullOrEmpty(connStr))
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
            connStr = configuration.GetConnectionString("DefaultConnection");
        }

        // 3. Fallback to hardcoded default if still empty
        if (string.IsNullOrEmpty(connStr))
        {
            connStr = "Host=localhost;Port=5432;Database=cyberpet_db;Username=admin;Password=secret";
        }

        optionsBuilder.UseNpgsql(connStr);

        return new AppDbContext(optionsBuilder.Options);
    }
}