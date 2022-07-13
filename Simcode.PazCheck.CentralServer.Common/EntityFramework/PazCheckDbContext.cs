using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using System;
using System.Linq;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class PazCheckDbContext : DbContext
    {
        public DbSet<Unit> Units { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<CeMatrix> CeMatrices { get; set; } = null!;
        public DbSet<Cause> Causes { get; set; } = null!;
        public DbSet<Effect> Effects { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<BaseActuator> BaseActuators { get; set; } = null!;        
        public DbSet<BaseActuatorType> BaseActuatorTypes { get; set; } = null!;
        public DbSet<EngineeringUnit> EngineeringUnits { get; set; } = null!;
        public DbSet<TagConditionIdentifier> TagConditionElementNames { get; set; } = null!;
        public DbSet<UnitEventsInterval> UnitEventsIntervals { get; set; } = null!;
        public DbSet<UnitEvent> UnitEvents { get; set; } = null!;
        public DbSet<Job> Jobs { get; set; } = null!;
        public DbSet<Simuser> SimUsers { get; set; } = null!;
        public DbSet<Office> Offices { get; set; } = null!;        
        public DbSet<Result> Results { get; set; } = null!;
        public DbSet<CeMatrixResult> CeMatrixResuls { get; set; } = null!;
        public DbSet<CauseResult> CauseResults { get; set; } = null!;
        public DbSet<EffectResult> EffectResults { get; set; } = null!;
        public DbSet<SetActiveProjectVersionRequest> SetActiveProjectVersionRequests { get; set; } = null!;

        public PazCheckDbContext(DbContextOptions<PazCheckDbContext> options) : base(options)
        { }

        public PazCheckDbContext() : base()
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {            
            optionsBuilder.UseNpgsql(@"Host=localhost;Username=postgres;Password=postgres;Database=pazcheck");            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Not working partial search with JSON API
            //modelBuilder.HasCollation(@"case_insensitive_collation", locale: "en-u-ks-primary", provider: "icu", deterministic: false);

            //modelBuilder.Entity<Tag>().Property(t => t.TagName)
            //        .UseCollation(@"case_insensitive_collation");

            var projectEntry = modelBuilder.Entity<Project>();
            projectEntry.HasOne(s => s.ActiveProjectVersion);
            projectEntry.HasOne(s => s.LastProjectVersion);
        }

        public override int SaveChanges()
        {
            DateTime utcNow = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseActuatorParam>().ToArray())
                BeforeSaveChanges(entry, utcNow);
            foreach (var entry in ChangeTracker.Entries<BaseActuator>().ToArray())
                BeforeSaveChanges(entry, utcNow);

            foreach (var entry in ChangeTracker.Entries<ActuatorParam>().ToArray())
                BeforeSaveChanges(entry, utcNow);
            foreach (var entry in ChangeTracker.Entries<TagParam>().ToArray())
                BeforeSaveChanges(entry, utcNow);
            foreach (var entry in ChangeTracker.Entries<TagCondition>().ToArray())
                BeforeSaveChanges(entry, utcNow);            
            foreach (var entry in ChangeTracker.Entries<Tag>().ToArray())
                BeforeSaveChanges(entry, utcNow);

            foreach (var entry in ChangeTracker.Entries<SubCause>().ToArray())
                BeforeSaveChanges(entry, utcNow);
            foreach (var entry in ChangeTracker.Entries<Cause>().ToArray())
                BeforeSaveChanges(entry, utcNow);
            foreach (var entry in ChangeTracker.Entries<Effect>().ToArray())
                BeforeSaveChanges(entry, utcNow);
            foreach (var entry in ChangeTracker.Entries<Intersection>().ToArray())
                BeforeSaveChanges(entry, utcNow);
            foreach (var entry in ChangeTracker.Entries<CeMatrix>().ToArray())
                BeforeSaveChanges(entry, utcNow);

            return base.SaveChanges();
        }

        private void BeforeSaveChanges(EntityEntry entry, DateTime utcNow)
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                ILastChangeEntity lastChangeEntity = (ILastChangeEntity)entry.Entity;
                lastChangeEntity._LastChangeTimeUtc = utcNow;
                lastChangeEntity.UpdateParentLastChange();
            }
        }
    }
}

//try
//{
//    ChangeTracker.AutoDetectChangesEnabled = false;

//}
//finally
//{
//    ChangeTracker.AutoDetectChangesEnabled = true;
//}

//services.AddDbContext<PazCheckDbContext>(opt =>
//{
//    opt.UseNpgsql(@"Host=localhost;Username=postgres;Password=postgres;Database=PazCheck");
//});

//modelBuilder.Entity<Cause>()
//                           .Property(p => p.IsActive)
//                           .HasComputedColumnSql(@"""_CreateTimeUtc"" IS NOT NULL AND ""_DeleteDateTimeUtc"" IS NULL", stored: true);
//modelBuilder.Entity<CeMatrix>()
//               .Property(p => p.IsActive)
//               .HasComputedColumnSql(@"""_CreateTimeUtc"" IS NOT NULL AND ""_DeleteDateTimeUtc"" IS NULL", stored: true);
//modelBuilder.Entity<Effect>()
//               .Property(p => p.IsActive)
//               .HasComputedColumnSql(@"""_CreateTimeUtc"" IS NOT NULL AND ""_DeleteDateTimeUtc"" IS NULL", stored: true);
//modelBuilder.Entity<Intersection>()
//               .Property(p => p.IsActive)
//               .HasComputedColumnSql(@"""_CreateTimeUtc"" IS NOT NULL AND ""_DeleteDateTimeUtc"" IS NULL", stored: true);
//modelBuilder.Entity<SubCause>()
//               .Property(p => p.IsActive)
//               .HasComputedColumnSql(@"""_CreateTimeUtc"" IS NOT NULL AND ""_DeleteDateTimeUtc"" IS NULL", stored: true);
//modelBuilder.Entity<Param>()
//               .Property(p => p.IsActive)
//               .HasComputedColumnSql(@"""_CreateTimeUtc"" IS NOT NULL AND ""_DeleteDateTimeUtc"" IS NULL", stored: true);
//modelBuilder.Entity<TagParam>()
//               .Property(p => p.IsActive)
//               .HasComputedColumnSql(@"""_CreateTimeUtc"" IS NOT NULL AND ""_DeleteDateTimeUtc"" IS NULL", stored: true);
//modelBuilder.Entity<ActuatorParam>()
//               .Property(p => p.IsActive)
//               .HasComputedColumnSql(@"""_CreateTimeUtc"" IS NOT NULL AND ""_DeleteDateTimeUtc"" IS NULL", stored: true);
//modelBuilder.Entity<BaseActuatorParam>()
//               .Property(p => p.IsActive)
//               .HasComputedColumnSql(@"""_CreateTimeUtc"" IS NOT NULL AND ""_DeleteDateTimeUtc"" IS NULL", stored: true);
//modelBuilder.Entity<Tag>()
//               .Property(p => p.IsActive)
//               .HasComputedColumnSql(@"""_CreateTimeUtc"" IS NOT NULL AND ""_DeleteDateTimeUtc"" IS NULL", stored: true);
//modelBuilder.Entity<TagCondition>()
//               .Property(p => p.IsActive)
//               .HasComputedColumnSql(@"""_CreateTimeUtc"" IS NOT NULL AND ""_DeleteDateTimeUtc"" IS NULL", stored: true);


