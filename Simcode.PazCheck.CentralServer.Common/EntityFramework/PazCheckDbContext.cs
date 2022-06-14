using Microsoft.EntityFrameworkCore;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;

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
        public DbSet<Actuator> Actuators { get; set; } = null!;
        public DbSet<UnitEventsInterval> UnitEventsIntervals { get; set; } = null!;
        public DbSet<Job> Jobs { get; set; } = null!;
        public DbSet<Simuser> SimUsers { get; set; } = null!;
        public DbSet<Office> Offices { get; set; } = null!;        
        public DbSet<Result> Results { get; set; } = null!;
        public DbSet<CeMatrixResult> CeMatrixResuls { get; set; } = null!;
        public DbSet<CauseResult> CauseResults { get; set; } = null!;
        public DbSet<EffectResult> EffectResults { get; set; } = null!;

        public PazCheckDbContext(DbContextOptions<PazCheckDbContext> options) : base(options)
        { }

        public PazCheckDbContext() : base()
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {            
            optionsBuilder.UseNpgsql(@"Host=localhost;Username=postgres;Password=postgres;Database=PazCheck");            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasCollation(@"case_insensitive_collation", locale: "en-u-ks-primary", provider: "icu", deterministic: false);

            modelBuilder.Entity<Tag>().Property(t => t.TagName)
                    .UseCollation(@"case_insensitive_collation");

            modelBuilder.Entity<VersionEntity>()
                .Property(p => p.IsActive)
                .HasComputedColumnSql(@"""_CreateTimeUtc"" IS NOT NULL AND ""_DeleteDateTimeUtc"" IS NULL", stored: true);
        }
    }
}

//services.AddDbContext<PazCheckDbContext>(opt =>
//{
//    opt.UseNpgsql(@"Host=localhost;Username=postgres;Password=postgres;Database=PazCheck");
//});
