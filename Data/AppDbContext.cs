using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Ignore(p => p.Backpack);
            entity.Ignore(p => p.FishBackpack);
            entity.Ignore(p => p.Items);
            // 老玩家数据迁移时默认 1 级 0 经验
            entity.Property(p => p.FishingLevel).HasDefaultValue(1);
            entity.Property(p => p.FishingXp).HasDefaultValue(0);
            entity.Property(p => p.CookingLevel).HasDefaultValue(1);
            entity.Property(p => p.CookingXp).HasDefaultValue(0);
            entity.Property(p => p.SelectedWorkJob).HasDefaultValue(WorkJobType.Construction);
            entity.Property(p => p.MaintenanceOverdue).HasDefaultValue(false);
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
            entity.Property(p => p.ExpeditionZoneId).HasMaxLength(32);
        });

        modelBuilder.Entity<CyberCat>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.CatLevel).HasDefaultValue(1);
            entity.Property(c => c.CatXp).HasDefaultValue(0);
            entity.Property(c => c.Str).HasDefaultValue(CatFishingStatsHelper.BaseStat);
            entity.Property(c => c.Agi).HasDefaultValue(CatFishingStatsHelper.BaseStat);
            entity.Property(c => c.Sen).HasDefaultValue(CatFishingStatsHelper.BaseStat);
            entity.Property(c => c.Sta).HasDefaultValue(CatFishingStatsHelper.BaseStat);
            entity.Property(c => c.Chm).HasDefaultValue(CatFishingStatsHelper.BaseStat);
            entity.Property(c => c.Luk).HasDefaultValue(CatFishingStatsHelper.BaseStat);
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(c => c.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BackpackItem>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.HasIndex(b => new { b.PlayerId, b.ItemName }).IsUnique();
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(b => b.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Fish>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(f => f.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlayerHouse>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Ignore(h => h.Rooms);
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(h => h.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => new { r.PlayerHouseId, r.Name }).IsUnique();
            entity.HasOne<PlayerHouse>()
                .WithMany()
                .HasForeignKey(r => r.PlayerHouseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Furniture>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.UpgradeLevel).HasDefaultValue(0);
            entity.HasIndex(f => new { f.RoomId, f.FurnitureId }).IsUnique();
            entity.HasOne<Room>()
                .WithMany()
                .HasForeignKey(f => f.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AutoFeeder>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Ignore(a => a.Foods);
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(a => a.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FeederFood>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.HasIndex(f => new { f.AutoFeederId, f.SlotIndex }).IsUnique();
            entity.HasOne<AutoFeeder>()
                .WithMany()
                .HasForeignKey(f => f.AutoFeederId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AutoWaterer>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Ignore(a => a.Waters);
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(a => a.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WatererWater>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.HasIndex(w => new { w.AutoWatererId, w.SlotIndex }).IsUnique();
            entity.HasOne<AutoWaterer>()
                .WithMany()
                .HasForeignKey(w => w.AutoWatererId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GameAccount>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => a.Username).IsUnique();
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(a => a.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

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

        modelBuilder.Entity<FishingLine>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.HasIndex(l => new { l.PlayerId, l.Name }).IsUnique();
            entity.Property(l => l.TargetDepth).HasDefaultValue(WaterDepth.Middle);
            entity.Property(l => l.Durability).HasDefaultValue(100);
            entity.Property(l => l.RequiredLevel).HasDefaultValue(1);
            entity.Property(l => l.IsCrafted).HasDefaultValue(false);
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(l => l.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FishingLure>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.HasIndex(l => new { l.PlayerId, l.Name }).IsUnique();
            entity.Property(l => l.TargetDepth).HasDefaultValue(WaterDepth.Middle);
            entity.Property(l => l.DurabilityRemaining).HasDefaultValue(20);
            entity.Property(l => l.RequiredLevel).HasDefaultValue(1);
            entity.Property(l => l.IsCrafted).HasDefaultValue(false);
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(l => l.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MarketListing>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.HasIndex(l => new { l.PlayerId, l.IsActive });
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(l => l.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NpcOffer>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.HasIndex(o => new { o.ListingId, o.IsAccepted });
            entity.HasOne<MarketListing>()
                .WithMany()
                .HasForeignKey(o => o.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NpcListingBan>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.HasIndex(b => new { b.ListingId, b.BuyerType }).IsUnique();
            entity.HasOne<MarketListing>()
                .WithMany()
                .HasForeignKey(b => b.ListingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

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

        modelBuilder.Entity<SpotLicense>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.HasIndex(l => new { l.PlayerId, l.SpotName }).IsUnique();
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(l => l.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

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

        modelBuilder.Entity<PlayerMilestoneUnlock>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => new { u.PlayerId, u.ItemId }).IsUnique();
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(u => u.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlayerGem>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.HasIndex(g => new { g.PlayerId, g.IsSocketed, g.SocketedSlot });
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(g => g.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlayerTargetLure>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.HasIndex(l => new { l.PlayerId, l.RecipeId });
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(l => l.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlayerCatBuff>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.HasIndex(b => new { b.PlayerId, b.BuffType });
            entity.HasOne<Player>()
                .WithMany()
                .HasForeignKey(b => b.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
