using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CyberPetApp.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // ⚙️ 专门给迁移工具临时指定连接字符串
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=cyberpet_db;Username=admin;Password=secret");

        return new AppDbContext(optionsBuilder.Options);
    }
}