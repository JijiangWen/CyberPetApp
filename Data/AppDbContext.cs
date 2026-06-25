using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Data;

/// <summary>
/// 数据库上下文类。
/// 它是整个程序与外部 PostgreSQL 数据库之间的“翻译官”和“控制台”。
/// 我们在 C# 中对这个类进行的操作，都会被它翻译成 SQL 语句，发给 PostgreSQL 执行。
/// </summary>
public class AppDbContext : DbContext
{
    // 构造函数：由 ASP.NET Core 容器调用，接收外界传入的连接字符串配置并传给基类
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSet<T> 代表数据库里的一张张“数据表”。
    // 例如下面的 Players 属性，就代表数据库中存储玩家账号信息的 "Players" 数据表。
    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<CyberCat> CyberCats { get; set; } = null!;
    public DbSet<BackpackItem> BackpackItems { get; set; } = null!;
    public DbSet<Fish> Fishes { get; set; } = null!;
    public DbSet<PlayerHouse> PlayerHouses { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<Furniture> Furnitures { get; set; } = null!;
    public DbSet<AutoFeeder> AutoFeeders { get; set; } = null!;
    public DbSet<FeederFood> FeederFoods { get; set; } = null!;
    public DbSet<AutoWaterer> AutoWaterers { get; set; } = null!;
    public DbSet<WatererWater> WatererWaters { get; set; } = null!;
    public DbSet<GameAccount> GameAccounts { get; set; } = null!;
    public DbSet<FishingRod> FishingRods { get; set; } = null!;
    public DbSet<FishingReel> FishingReels { get; set; } = null!;
    public DbSet<FishingLine> FishingLines { get; set; } = null!;
    public DbSet<FishingLure> FishingLures { get; set; } = null!;
    public DbSet<MarketListing> MarketListings { get; set; } = null!;
    public DbSet<NpcOffer> NpcOffers { get; set; } = null!;
    public DbSet<NpcListingBan> NpcListingBans { get; set; } = null!;
    public DbSet<FishCatchRecord> FishCatchRecords { get; set; } = null!;
    public DbSet<SpotLicense> SpotLicenses { get; set; } = null!;
    public DbSet<PlayerAchievement> PlayerAchievements { get; set; } = null!;
    public DbSet<PlayerMilestoneUnlock> PlayerMilestoneUnlocks { get; set; } = null!;
    public DbSet<PlayerGem> PlayerGems { get; set; } = null!;
    public DbSet<PlayerTargetLure> PlayerTargetLures { get; set; } = null!;
    public DbSet<PlayerCatBuff> PlayerCatBuffs { get; set; } = null!;
    public DbSet<PlayerBoat> PlayerBoats { get; set; } = null!;
    public DbSet<BoatCatchRecord> BoatCatchRecords { get; set; } = null!;

    // 配置警告忽略规则：在这里我们关闭了一些开发期间无意义的数据库迁移警告
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        base.OnConfiguring(optionsBuilder);
    }

    // 核心重写方法：用于精细化配置每一张数据表的映射规则、字段默认值、以及表与表之间的关系（外键与级联删除）
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. 配置玩家表 (Player Table)
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(p => p.Id); // 设置 Id 字段为“主键”（唯一身份证）

            // ⚠️ 忽略映射属性：下面的这些属性在 C# 内存中用作快捷查询，并不需要真的在数据库表里建字段。
            entity.Ignore(p => p.Backpack);
            entity.Ignore(p => p.FishBackpack);
            entity.Ignore(p => p.Items);
            entity.Ignore(p => p.UnlockedCatSkins);

            // 💡 字段默认值配置：当新玩家注册账号写入数据库时，这些字段如果没有赋值，数据库会自动填入下面的默认初值。
            entity.Property(p => p.FishingLevel).HasDefaultValue(1); // 钓鱼等级默认从 1 开始
            entity.Property(p => p.FishingXp).HasDefaultValue(0);
            entity.Property(p => p.CookingLevel).HasDefaultValue(1); // 烹饪等级默认从 1 开始
            entity.Property(p => p.CookingXp).HasDefaultValue(0);
            entity.Property(p => p.SelectedWorkJob).HasDefaultValue(WorkJobType.Construction); // 默认工种为工地搬砖
            entity.Property(p => p.MaintenanceOverdue).HasDefaultValue(false); // 默认没有拖欠房屋物业折旧费
            entity.Property(p => p.WorkTickCount).HasDefaultValue(0);
            entity.Property(p => p.DailyBountyReward).HasDefaultValue(0);
            entity.Property(p => p.DailyBountyClaimed).HasDefaultValue(false);
            entity.Property(p => p.DailyBountyManualRefreshCount).HasDefaultValue(0);
            entity.Property(p => p.UnlockedWorkJobMask).HasDefaultValue(1);
            entity.Property(p => p.MilestonePoints).HasDefaultValue(0);
            entity.Property(p => p.LifetimeFeedCount).HasDefaultValue(0);
            entity.Property(p => p.TotalFishGoldEarned).HasDefaultValue(0);
            entity.Property(p => p.MarketSalesCount).HasDefaultValue(0);
            entity.Property(p => p.RareCatchCount).HasDefaultValue(0);
            entity.Property(p => p.LegendaryCatchCount).HasDefaultValue(0);
            entity.Property(p => p.CookCount).HasDefaultValue(0);
            entity.Property(p => p.ExpeditionZoneId).HasMaxLength(32); // 限制远征区域 ID 字符串的最大长度为 32
            entity.Property(p => p.AutoRepairUnlocked).HasDefaultValue(false);
            entity.Property(p => p.AutoRepairEnabled).HasDefaultValue(false);
            entity.Property(p => p.AutoRepairThreshold).HasDefaultValue(20);
            entity.Property(p => p.AutoRefillUnlocked).HasDefaultValue(false);
            entity.Property(p => p.AutoRefillEnabled).HasDefaultValue(false);
        });

        // 2. 配置赛博猫咪表 (CyberCat Table)
        modelBuilder.Entity<CyberCat>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.CatLevel).HasDefaultValue(1);
            entity.Property(c => c.CatXp).HasDefaultValue(0);
            // 设定猫咪的力量、敏捷、感知、耐力、魅力、幸运的基础出厂初始属性
            entity.Property(c => c.Str).HasDefaultValue(CatFishingStatsHelper.BaseStat);
            entity.Property(c => c.Agi).HasDefaultValue(CatFishingStatsHelper.BaseStat);
            entity.Property(c => c.Sen).HasDefaultValue(CatFishingStatsHelper.BaseStat);
            entity.Property(c => c.Sta).HasDefaultValue(CatFishingStatsHelper.BaseStat);
            entity.Property(c => c.Chm).HasDefaultValue(CatFishingStatsHelper.BaseStat);
            entity.Property(c => c.Luk).HasDefaultValue(CatFishingStatsHelper.BaseStat);
            
            // 💡 建立关联外键与级联删除：一只猫咪必须归属（HasOne）一个玩家。
            // 开启 DeleteBehavior.Cascade（级联删除）意味着：如果管理员在数据库删除了该玩家账号，
            // 数据库会自动将这只归属于他的赛博猫咪也一并彻底删除，不留垃圾空记录。
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(c => c.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 3. 配置背包表 (BackpackItem Table)
        modelBuilder.Entity<BackpackItem>(entity =>
        {
            entity.HasKey(b => b.Id);
            // 💡 唯一索引设定：让数据库强制校验，同一个玩家的背包里，不能出现两条重名的道具记录。
            entity.HasIndex(b => new { b.PlayerId, b.ItemName }).IsUnique();
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(b => b.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 4. 配置鱼袋表 (Fish Table)
        modelBuilder.Entity<Fish>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(f => f.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 5. 配置玩家小屋表 (PlayerHouse Table)
        modelBuilder.Entity<PlayerHouse>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Ignore(h => h.Rooms);
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(h => h.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 6. 配置房间表 (Room Table)
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(r => r.Id);
            // 确保同一个房子里不会有两个名字一模一样的房间
            entity.HasIndex(r => new { r.PlayerHouseId, r.Name }).IsUnique();
            entity.HasOne<PlayerHouse>()
                .WithMany()
                .HasForeignKey(r => r.PlayerHouseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 7. 配置家具表 (Furniture Table)
        modelBuilder.Entity<Furniture>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.UpgradeLevel).HasDefaultValue(0);
            entity.HasIndex(f => new { f.RoomId, f.FurnitureId }).IsUnique();
            entity.HasOne<Room>()
                .WithMany(r => r.Furniture)
                .HasForeignKey(f => f.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 8. 配置自动喂食器主表 (AutoFeeder Table)
        modelBuilder.Entity<AutoFeeder>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Ignore(a => a.Foods);
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(a => a.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 9. 配置自动喂食器内口粮子表 (FeederFood Table)
        modelBuilder.Entity<FeederFood>(entity =>
        {
            entity.HasKey(f => f.Id);
            // 确保同一个喂食器的同一个槽位序号（SlotIndex）里，只有一份口粮，不允许重叠
            entity.HasIndex(f => new { f.AutoFeederId, f.SlotIndex }).IsUnique();
            entity.HasOne<AutoFeeder>()
                .WithMany()
                .HasForeignKey(f => f.AutoFeederId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 10. 配置自动饮水器表
        modelBuilder.Entity<AutoWaterer>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Ignore(a => a.Waters);
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(a => a.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 11. 配置自动饮水器内存水表
        modelBuilder.Entity<WatererWater>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.HasIndex(w => new { w.AutoWatererId, w.SlotIndex }).IsUnique();
            entity.HasOne<AutoWaterer>()
                .WithMany()
                .HasForeignKey(w => w.AutoWatererId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 12. 关联游戏账号密码表
        modelBuilder.Entity<GameAccount>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => a.Username).IsUnique(); // 限制用户名全服唯一，不重名
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(a => a.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 13. 鱼竿装备表
        modelBuilder.Entity<FishingRod>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => new { r.PlayerId, r.Name }).IsUnique();
            entity.Property(r => r.Durability).HasDefaultValue(100);
            entity.Property(r => r.CastRange).HasDefaultValue(1);
            entity.Property(r => r.RequiredLevel).HasDefaultValue(1);
            entity.Property(r => r.IsCrafted).HasDefaultValue(false);
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(r => r.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 14. 渔轮装备表
        modelBuilder.Entity<FishingReel>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => new { r.PlayerId, r.Name }).IsUnique();
            entity.Property(r => r.Durability).HasDefaultValue(100);
            entity.Property(r => r.LineCapacity).HasDefaultValue(8.0);
            entity.Property(r => r.Smoothness).HasDefaultValue(0.3);
            entity.Property(r => r.RequiredLevel).HasDefaultValue(1);
            entity.Property(r => r.IsCrafted).HasDefaultValue(false);
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(r => r.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 15. 鱼线装备表
        modelBuilder.Entity<FishingLine>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.HasIndex(l => new { l.PlayerId, l.Name }).IsUnique();
            entity.Property(l => l.Durability).HasDefaultValue(100);
            entity.Property(l => l.RequiredLevel).HasDefaultValue(1);
            entity.Property(l => l.IsCrafted).HasDefaultValue(false);
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(l => l.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 16. 拟饵装备表
        modelBuilder.Entity<FishingLure>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.HasIndex(l => new { l.PlayerId, l.Name }).IsUnique();
            entity.Property(l => l.DurabilityRemaining).HasDefaultValue(20);
            entity.Property(l => l.RequiredLevel).HasDefaultValue(1);
            entity.Property(l => l.IsCrafted).HasDefaultValue(false);
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(l => l.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 17. 鱼市挂单表
        modelBuilder.Entity<MarketListing>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.HasIndex(l => new { l.PlayerId, l.IsActive });
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(l => l.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 18. 鱼市 NPC 报价表
        modelBuilder.Entity<NpcOffer>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.HasIndex(o => new { o.ListingId, o.IsAccepted });
            entity.HasOne<MarketListing>()
                .WithMany()
                .HasForeignKey(o => o.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 19. 砍价失败黑名单封禁表
        modelBuilder.Entity<NpcListingBan>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.HasIndex(b => new { b.ListingId, b.BuyerType }).IsUnique();
            entity.HasOne<MarketListing>()
                .WithMany()
                .HasForeignKey(b => b.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 20. 图鉴收集进度与最大重量记录表
        modelBuilder.Entity<FishCatchRecord>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => new { r.PlayerId, r.FishName }).IsUnique();
            entity.Property(r => r.IsTargetExclusive).HasDefaultValue(false);
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(r => r.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 21. 钓点通行许可证表
        modelBuilder.Entity<SpotLicense>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.HasIndex(l => new { l.PlayerId, l.SpotName }).IsUnique();
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(l => l.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 22. 玩家成就条目表
        modelBuilder.Entity<PlayerAchievement>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => new { a.PlayerId, a.AchievementId }).IsUnique();
            entity.Property(a => a.RewardClaimed).HasDefaultValue(false);
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(a => a.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 23. 里程碑商品购买解锁表
        modelBuilder.Entity<PlayerMilestoneUnlock>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => new { u.PlayerId, u.ItemId }).IsUnique();
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(u => u.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 24. 装备插槽上的炼金宝石表
        modelBuilder.Entity<PlayerGem>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.HasIndex(g => new { g.PlayerId, g.IsSocketed, g.SocketedSlot });
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(g => g.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 25. 炼金神话诱饵表
        modelBuilder.Entity<PlayerTargetLure>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.HasIndex(l => new { l.PlayerId, l.RecipeId });
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(l => l.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 26. 猫咪食品 Buff 计时表
        modelBuilder.Entity<PlayerCatBuff>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.HasIndex(b => new { b.PlayerId, b.BuffType });
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(b => b.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 27. 玩家小船表
        modelBuilder.Entity<PlayerBoat>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.HasIndex(b => new { b.PlayerId, b.BoatType }).IsUnique();
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(b => b.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 28. 联机船钓鱼获广播大喇叭记录表
        modelBuilder.Entity<BoatCatchRecord>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.PlayerId);
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(r => r.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
