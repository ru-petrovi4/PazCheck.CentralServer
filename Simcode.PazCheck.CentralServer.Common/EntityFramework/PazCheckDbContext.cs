using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using System;
using System.Linq;
using System.Reflection;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class PazCheckDbContext : DbContext
    {
        public DbSet<Unit> Units { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<ProjectVersion> ProjectVersions { get; set; } = null!;
        public DbSet<CeMatrix> CeMatrices { get; set; } = null!;
        public DbSet<Cause> Causes { get; set; } = null!;
        public DbSet<Effect> Effects { get; set; } = null!;
        public DbSet<Intersection> Intersections { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<BaseActuator> BaseActuators { get; set; } = null!;        
        public DbSet<BaseActuatorType> BaseActuatorTypes { get; set; } = null!;
        public DbSet<BaseActuatorParam> BaseActuatorParams { get; set; } = null!;
        public DbSet<BaseActuatorDbFileReference> BaseActuatorDbFileReferences { get; set; } = null!;
        public DbSet<TagParam> TagParams { get; set; } = null!;
        public DbSet<ActuatorParam> ActuatorParams { get; set; } = null!;
        public DbSet<TagCondition> TagConditions { get; set; } = null!;
        public DbSet<CeMatrixParam> CeMatrixParams { get; set; } = null!;
        public DbSet<CeMatrixDbFileReference> CeMatrixDbFileReferences { get; set; } = null!;
        public DbSet<EngineeringUnit> EngineeringUnits { get; set; } = null!;
        public DbSet<ParamInfo> ParamInfos { get; set; } = null!;
        public DbSet<TagConditionInfo> TagConditionInfos { get; set; } = null!;
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
        public DbSet<LogUserEvent> LogUserEvents { get; set; } = null!;

        public bool IsLastChangeTimeUtcUpdatingDisabled { get; set; }

        public PazCheckDbContext(DbContextOptions<PazCheckDbContext> options) : base(options)
        {
            SavingChanges += OnSavingChanges;
        }

        public PazCheckDbContext() : base()
        {
            SavingChanges += OnSavingChanges;
        }

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

        private void OnSavingChanges(object? sender, SavingChangesEventArgs e)
        {
            if (IsLastChangeTimeUtcUpdatingDisabled)
                return;

            DateTime utcNow = DateTime.UtcNow;

            var entries = ChangeTracker.Entries().ToArray(); // Updates all entity cross-references

            foreach (var entry in entries)
            {
                ILastChangeEntity? lastChangeEntity = entry.Entity as ILastChangeEntity;
                if (lastChangeEntity is not null)
                {
                    bool updateLastChangeTimeUtc = false;
                    if (entry.State == EntityState.Added)
                    {
                        updateLastChangeTimeUtc = true;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        PropertyEntry propertyEntry = entry.Property(nameof(ILastChangeEntity._IsDeleted));
                        if ((bool)propertyEntry.CurrentValue! && !(bool)propertyEntry.OriginalValue!)
                            updateLastChangeTimeUtc = true;
                    }
                    if (updateLastChangeTimeUtc)
                    {
                        lastChangeEntity._LastChangeTimeUtc = utcNow;
                        lastChangeEntity.UpdateParentLastChange();
                    }                    
                }
            }
        }        
    }
}

//var baseActuatorParamEntry = modelBuilder.Entity<BaseActuatorParam>();
//baseActuatorParamEntry.HasOne(s => s.ParamInfo);

//var tagParam = modelBuilder.Entity<TagParam>();
//tagParam.HasOne(s => s.ParamInfo);

//var actuatorParam = modelBuilder.Entity<ActuatorParam>();
//actuatorParam.HasOne(s => s.ParamInfo);

//var ceMatrixParam = modelBuilder.Entity<CeMatrixParam>();
//ceMatrixParam.HasOne(s => s.ParamInfo);

//var tagConditionEntry = modelBuilder.Entity<TagCondition>();
//tagConditionEntry.HasOne(s => s.TagConditionInfo);

//private void BeforeSaveChanges(EntityEntry entry, DateTime utcNow)
//{
//    if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
//    {
//        ILastChangeEntity? lastChangeEntity = entry.Entity as ILastChangeEntity;
//        if (lastChangeEntity is not null)
//        {
//            lastChangeEntity._LastChangeTimeUtc = utcNow;
//            lastChangeEntity.UpdateParentLastChange();
//        }
//    }
//}

//foreach (var entry in ChangeTracker.Entries<BaseActuatorParam>())
//    BeforeSaveChanges(entry, utcNow);
//foreach (var entry in ChangeTracker.Entries<BaseActuator>())
//    BeforeSaveChanges(entry, utcNow);

//foreach (var entry in ChangeTracker.Entries<ActuatorParam>())
//    BeforeSaveChanges(entry, utcNow);
//foreach (var entry in ChangeTracker.Entries<TagParam>())
//    BeforeSaveChanges(entry, utcNow);
//foreach (var entry in ChangeTracker.Entries<TagCondition>())
//    BeforeSaveChanges(entry, utcNow);            
//foreach (var entry in ChangeTracker.Entries<Tag>())
//    BeforeSaveChanges(entry, utcNow);

//foreach (var entry in ChangeTracker.Entries<SubCause>())
//    BeforeSaveChanges(entry, utcNow);
//foreach (var entry in ChangeTracker.Entries<Cause>())
//    BeforeSaveChanges(entry, utcNow);
//foreach (var entry in ChangeTracker.Entries<Effect>())
//    BeforeSaveChanges(entry, utcNow);
//foreach (var entry in ChangeTracker.Entries<Intersection>())
//    BeforeSaveChanges(entry, utcNow);
//foreach (var entry in ChangeTracker.Entries<CeMatrix>())
//    BeforeSaveChanges(entry, utcNow);

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


