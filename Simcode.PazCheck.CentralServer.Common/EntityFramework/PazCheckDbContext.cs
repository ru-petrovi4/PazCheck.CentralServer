using Humanizer.Configuration;
using IdentityServer4;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.Helpers;
using Simcode.PazCheck.CentralServer.Common.Properties;
using Ssz.Utils;
using Ssz.Utils.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using Simcode.PazCheck.CentralServer.Common.Serialization;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Use 
    ///     readOnlyDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking
    ///     for readOnlyDbContext.
    /// </summary>
    public class PazCheckDbContext : DbContext
    {
        #region construction and destruction

        static PazCheckDbContext()
        {
            var props = typeof(PazCheckDbContext).GetProperties()
                       .Where(pi => pi.PropertyType.Name == @"DbSet`1").ToArray();
            EntitiesName_PropertyInfos = props.ToDictionary(pi => pi.Name, pi => pi, StringComparer.InvariantCultureIgnoreCase);
            EntityName_PropertyInfos = props.ToDictionary(pi => pi.PropertyType.GetGenericArguments()[0].Name, pi => pi, StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        ///     Nullable params for design-time tools.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="configuration"></param>        
        /// <param name="httpContextAccessor"></param>
        /// <param name="dbContextFactory"></param>
        /// <param name="informationSecurityEventsLogger"></param>
        /// <param name="dbCache"></param>
        public PazCheckDbContext(
            DbContextOptions<PazCheckDbContext>? options = null,
            IConfiguration? configuration = null,
            IHttpContextAccessor? httpContextAccessor = null,
            IDbContextFactory<PazCheckDbContext>? dbContextFactory = null,
            IInformationSecurityEventsLogger? informationSecurityEventsLogger = null,
            Cache? dbCache = null) :
                this(
                    (DbContextOptions?)options,
                    configuration,                    
                    httpContextAccessor,
                    dbContextFactory,
                    informationSecurityEventsLogger,
                    dbCache)
        {
        }

        /// <summary>
        ///     Nullable params for design-time tools.
        ///     For derived classes.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="configuration"></param>        
        /// <param name="httpContextAccessor"></param>
        /// <param name="dbContextFactory"></param>
        /// <param name="informationSecurityEventsLogger"></param>
        /// <param name="dbCache"></param>
        protected PazCheckDbContext(
            DbContextOptions? options = null,
            IConfiguration? configuration = null,
            IHttpContextAccessor? httpContextAccessor = null,
            IDbContextFactory<PazCheckDbContext>? dbContextFactory = null,
            IInformationSecurityEventsLogger? informationSecurityEventsLogger = null,
            Cache? dbCache = null) :
                base(options ?? new DbContextOptions<PazCheckDbContext>())
        {
            HttpContextAccessor = httpContextAccessor;
            Configuration = configuration;            
            DbContextFactory = dbContextFactory;
            InformationSecurityEventsLogger_ = informationSecurityEventsLogger;
            DbCache = dbCache;

            SavingChanges += OnSavingChanges;
            SavedChanges += OnSavedChanges;
        }

        #endregion

        #region public functions

        /// <summary>
        ///     [PropertyName, PropertyInfo]. Thread-safe.
        /// </summary>
        public static Dictionary<string, PropertyInfo> EntitiesName_PropertyInfos { get; }

        /// <summary>
        ///     [EntityType, PropertyInfo]. Thread-safe.
        /// </summary>
        public static Dictionary<string, PropertyInfo> EntityName_PropertyInfos { get; }

        public DbSet<MetaParam> MetaParams { get; set; } = null!;

        public DbSet<MetaParamArg> MetaParamArgs { get; set; } = null!;

        public DbSet<UserParam> UserParams { get; set; } = null!;

        [MajorEntity] // For DOC generating
        [DefaultEntity_RoleBusinessFunctions(
            // Read            
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View),
                           nameof(DefaultRoleBusinessFunctions.WidgetsLongRunningView) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Supervise) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [InformationSecurityEventEntity(AllRolesAccess = true)]
        [PcDisplayName(ResourceStrings.Unit)]
        public DbSet<Unit> Units { get; set; } = null!;

        [MajorEntity] // For DOC generating
        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [InformationSecurityEventEntity(AllRolesAccess = true)]
        [PcDisplayName(ResourceStrings.Project)]
        public DbSet<Project> Projects { get; set; } = null!;

        [MajorEntity] // For DOC generating
        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit),
                           nameof(DefaultRoleBusinessFunctions.Projects_Supervise)},
            // Delete
            new string[] { },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]        
        public DbSet<ProjectVersion> ProjectVersions { get; set; } = null!;

        [MajorEntity] // For DOC generating
        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [InformationSecurityEventEntity(AllRolesAccess = true)]
        [PcDisplayName(ResourceStrings.ProjectVersionType)]        
        public DbSet<ProjectVersionType> ProjectVersionTypes { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        public DbSet<ProjectVersionDbFileReference> ProjectVersionDbFileReferences { get; set; } = null!;

        [MajorEntity] // For DOC generating
        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [InformationSecurityEventEntity(AllRolesAccess = true)]
        [PcDisplayName(ResourceStrings.CeMatrix)]
        public DbSet<CeMatrix> CeMatrices { get; set; } = null!;        

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [PcDisplayName(ResourceStrings.CeMatrixParam)]
        public DbSet<CeMatrixParam> CeMatrixParams { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [PcDisplayName(ResourceStrings.DbFileReference)]
        public DbSet<CeMatrixDbFileReference> CeMatrixDbFileReferences { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [PcDisplayName(ResourceStrings.Row)]
        public DbSet<Row> Rows { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [PcDisplayName(ResourceStrings.Column)]
        public DbSet<Column> Columns { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [PcDisplayName(ResourceStrings.Cell)]
        public DbSet<Cell> Cells { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [PcDisplayName(ResourceStrings.CeMatrixComment)]
        public DbSet<CeMatrixComment> CeMatrixComments { get; set; } = null!;

        [MajorEntity] // For DOC generating
        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [InformationSecurityEventEntity(AllRolesAccess = true)]
        [PcDisplayName(ResourceStrings.Tag)]
        public DbSet<Tag> Tags { get; set; } = null!;        

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        public DbSet<TagConditionInfo> TagConditionInfos { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [PcDisplayName(ResourceStrings.TagParam)]
        public DbSet<TagParam> TagParams { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [PcDisplayName(ResourceStrings.TagCondition)]
        public DbSet<TagCondition> TagConditions { get; set; } = null!;        

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [PcDisplayName(ResourceStrings.DbFileReference)]
        public DbSet<TagDbFileReference> TagDbFileReferences { get; set; } = null!;

        [MajorEntity] // For DOC generating
        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [InformationSecurityEventEntity(AllRolesAccess = true)]
        [PcDisplayName(ResourceStrings.BaseActuator)]
        public DbSet<BaseActuator> BaseActuators { get; set; } = null!;        

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [PcDisplayName(ResourceStrings.BaseActuatorParam)]
        public DbSet<BaseActuatorParam> BaseActuatorParams { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [PcDisplayName(ResourceStrings.DbFileReference)]
        public DbSet<BaseActuatorDbFileReference> BaseActuatorDbFileReferences { get; set; } = null!;

        [MajorEntity] // For DOC generating
        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [InformationSecurityEventEntity(AllRolesAccess = true)]
        [PcDisplayName(ResourceStrings.SafetyController)]
        public DbSet<SafetyController> SafetyControllers { get; set; } = null!;        

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [PcDisplayName(ResourceStrings.SafetyControllerParam)]
        public DbSet<SafetyControllerParam> SafetyControllerParams { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [PcDisplayName(ResourceStrings.DbFileReference)]
        public DbSet<SafetyControllerDbFileReference> SafetyControllerDbFileReferences { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [InformationSecurityEventEntity(AllRolesAccess = true)]
        [PcDisplayName(ResourceStrings.Legend)]
        public DbSet<Legend> Legends { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [PcDisplayName(ResourceStrings.DbFileReference)]
        public DbSet<LegendDbFileReference> LegendDbFileReferences { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [PcDisplayName(ResourceStrings.LegendParam)]
        public DbSet<LegendParam> LegendParams { get; set; } = null!;

        [MajorEntity] // For DOC generating
        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditUnitEvents) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditUnitEvents) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditUnitEvents) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        public DbSet<UnitEventsInterval> UnitEventsIntervals { get; set; } = null!;

        [MajorEntity] // For DOC generating
        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditUnitEvents) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditUnitEvents) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditUnitEvents) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        public DbSet<UnitEvent> UnitEvents { get; set; } = null!;

        [MajorEntity] // For DOC generating
        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View),
                           nameof(DefaultRoleBusinessFunctions.WidgetsLongRunningView) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [InformationSecurityEventEntity(AllRolesAccess = true)]
        [PcDisplayName(ResourceStrings.BasePcObject)]
        public DbSet<BasePcObject> BasePcObjects { get; set; } = null!;

        [MajorEntity] // For DOC generating
        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [InformationSecurityEventEntity(AllRolesAccess = true)]
        [PcDisplayName(ResourceStrings.PcObjectEventType)]
        public DbSet<PcObjectEventType> PcObjectEventTypes { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        public DbSet<BasePcObjectDbFileReference> BasePcObjectDbFileReferences { get; set; } = null!;

        [MajorEntity] // For DOC generating
        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [PcDisplayName(ResourceStrings.PcObject)]
        public DbSet<PcObject> PcObjects { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        public DbSet<PcObjectDbFileReference> PcObjectDbFileReferences { get; set; } = null!;

        [MajorEntity] // For DOC generating
        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [InformationSecurityEventEntity(AllRolesAccess = true)]
        [PcDisplayName(ResourceStrings.PcObjectEvent)]
        public DbSet<PcObjectEvent> PcObjectEvents { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        public DbSet<PcObjectEventDbFileReference> PcObjectEventDbFileReferences { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        public DbSet<DbFile> DbFiles { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        public DbSet<DbFileContent> DbFileContents { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        public DbSet<ParamInfo> ParamInfos { get; set; } = null!;

        [MajorEntity] // For DOC generating
        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Projects_Edit),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        [InformationSecurityEventEntity(AllRolesAccess = true)]
        [PcDisplayName(ResourceStrings.ParamDesc)]
        public DbSet<ParamDesc> ParamDescs { get; set; } = null!;

        [MajorEntity] // For DOC generating
        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditCalculationResults) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditCalculationResults) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditCalculationResults) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        public DbSet<Result> Results { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditCalculationResults) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditCalculationResults) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditCalculationResults) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        public DbSet<ResultEvent> ResultEvents { get; set; } = null!;

        [MajorEntity] // For DOC generating
        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditCalculationResults) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditCalculationResults) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditCalculationResults) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        public DbSet<CeMatrixResult> CeMatrixResults { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditCalculationResults) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditCalculationResults) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditCalculationResults) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        public DbSet<RowResult> RowResults { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditCalculationResults) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditCalculationResults) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditCalculationResults) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        public DbSet<ColumnResult> ColumnResults { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditCalculationResults) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditCalculationResults) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Diagnost_EditCalculationResults) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        public DbSet<CellResult> CellResults { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        public DbSet<Job> Jobs { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { },
            // Update
            new string[] { },
            // Delete
            new string[] { },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) }
        )]
        public DbSet<AddonStatus> AddonStatuses { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        public DbSet<UserEvent> UserEvents { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.View) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.View) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.View) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View), nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) }
        )]
        public DbSet<InformationMessage> InformationMessages { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.View) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.View) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.View) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) }
        )]
        public DbSet<RequestMessage> RequestMessages { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        public DbSet<BasePcObjectJournalParam> BasePcObjectJournalParams { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        public DbSet<PcObjectJournalParam> PcObjectJournalParams { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        public DbSet<JournalParamValuesCollection> JournalParamValuesCollections { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.View),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.Monitoring_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.View) }
        )]
        public DbSet<FloatJournalParamValue> FloatJournalParamValues { get; set; } = null!;        

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { },
            // Update
            new string[] { },
            // Delete
            new string[] { },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) }
        )]
        public DbSet<InformationSecurityEvent> InformationSecurityEvents { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read
            new string[] { nameof(DefaultRoleBusinessFunctions.AllRoles_RoleApiFunction_Modifier) },
            // Create
            new string[] { },
            // Update
            new string[] { },
            // Delete
            new string[] { },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.AllRoles_RoleApiFunction_Modifier) }
        )]
        public DbSet<AllRolesAccessInformationSecurityEvent> AllRolesAccessInformationSecurityEvents { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read            
            new string[] { nameof(DefaultRoleBusinessFunctions.Public_RoleApiFunction_Modifier) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Roles_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Roles_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Roles_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.Roles_View) }
        )]
        [InformationSecurityEventEntity(AllRolesAccess = false)]
        [PcDisplayName(ResourceStrings.Role)]
        public DbSet<Role> Roles { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read            
            new string[] { nameof(DefaultRoleBusinessFunctions.AllRoles_RoleApiFunction_Modifier) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Roles_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Roles_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Roles_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.Roles_View) }
        )]
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read            
            new string[] { nameof(DefaultRoleBusinessFunctions.AllRoles_RoleApiFunction_Modifier) },
            // Create
            new string[] { nameof(DefaultRoleBusinessFunctions.Roles_Edit) },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Roles_Edit) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Roles_Edit) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.Roles_View) }
        )]
        [InformationSecurityEventEntity(AllRolesAccess = false)]
        [PcDisplayName(ResourceStrings.RoleBusinessFunction)]
        public DbSet<RoleBusinessFunction> RoleBusinessFunctions { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read            
            new string[] { nameof(DefaultRoleBusinessFunctions.AllRoles_RoleApiFunction_Modifier) },
            // Create
            new string[] { },
            // Update
            new string[] { },
            // Delete
            new string[] { },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.Roles_View) }
        )]
        public DbSet<RoleApiFunction> RoleApiFunctions { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read            
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) }
        )]
        [InformationSecurityEventEntity(AllRolesAccess = false)]
        [PcDisplayName(ResourceStrings.LicenseFile)]
        public DbSet<LicenseFile> LicenseFiles { get; set; } = null!;

        [DefaultEntity_RoleBusinessFunctions(
            // Read            
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
                           nameof(DefaultRoleBusinessFunctions.InformationSecurityEvents_View) },
            // Create
            new string[] { },
            // Update
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) },
            // Delete
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) },
            // View            
            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating) }
        )]
        [InformationSecurityEventEntity(AllRolesAccess = false)]
        [PcDisplayName(ResourceStrings.CryptoEntity)]
        public DbSet<CryptoEntity> CryptoEntities { get; set; } = null!;

        /// <summary>
        ///     Used in this.OnSavingChanges()
        ///     User for LastChangeUser fields.
        ///     If empty, tries to get user from HttpContext.
        /// </summary>
        public string User { get; set; } = @"";

        /// <summary>
        ///     Used in this.OnSavingChanges()
        ///     Whether to update when saving to DB 
        ///     _HasUnversionedChanges, _LastChangeTimeUtc
        /// </summary>
        public bool IsLastChangeFieldsUpdatingDisabled { get; set; }

        /// <summary>
        ///     Used in this.OnSavingChanges()
        ///     Is auto logging is disabled.
        /// </summary>
        public bool IsInformationSecurityEventsLoggingDisabled { get; set; }

        /// <summary>
        ///     Used in this.OnSavingChanges()
        ///     Is project.DataGuid updating is disabled.
        /// </summary>
        public bool DisableProjectGuidUpdate { get; set; }

        #endregion

        #region protected functions

        protected IHttpContextAccessor? HttpContextAccessor { get; }
        protected IConfiguration? Configuration { get; }
        protected IDbContextFactory<PazCheckDbContext>? DbContextFactory { get; }
        protected IInformationSecurityEventsLogger? InformationSecurityEventsLogger_ { get; }
        protected Cache? DbCache { get; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (Configuration is null)
            {
                // For Visual Studio designer
                optionsBuilder.UseNpgsql(
                    @"Host=localhost;Username=postgres;Password=postgres;Database=pazcheck;CommandTimeout=3600",
                        o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                    .EnableThreadSafetyChecks(false);
            }
            else
            {
                optionsBuilder.UseNpgsql(
                    Configuration.GetConnectionString("MainDbConnection"),
                        o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                    .EnableThreadSafetyChecks(false);
            }

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Not working partial search with JSON API
            //modelBuilder.HasCollation(@"case_insensitive_collation", locale: "en-u-ks-primary", provider: "icu", deterministic: false);           

            //modelBuilder.Entity<Tag>().Property(t => t.TagName)
            //        .UseCollation(@"case_insensitive_collation");            

            modelBuilder
                .Entity<RowResult>()
                .HasOne(re => re.InputTriggeredUnitEvent)
                .WithMany(ue => ue.RowResults)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder
                .Entity<ColumnResult>()
                .HasOne(re => re.TriggeredUnitEvent)
                .WithMany(ue => ue.ColumnResults)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder
                .Entity<ResultEvent>()
                .HasOne(re => re.TriggeredUnitEvent)
                .WithMany(ue => ue.ResultEvents)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder
                .Entity<ResultEvent>()
                .HasMany(re => re.EffectResultEvents)
                .WithMany(re => re.CauseResultEvents);

            modelBuilder
                .Entity<CeMatrix>()
                .Property(e => e.IdentifierLower)
                .HasComputedColumnSql("LOWER(\"Identifier\")", stored: true);

            modelBuilder
                .Entity<CeMatrix>()
                .HasIndex(e => new { e.ProjectId, e.IdentifierLower })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique();

            modelBuilder
                .Entity<CeMatrix>()
                .HasIndex(e => new { e.ProjectId, e._CreateProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<CeMatrix>()
                .HasIndex(e => new { e.ProjectId, e._DeleteProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<CeMatrixParam>()
                .Property(e => e.ParamNameLower)
                .HasComputedColumnSql("LOWER(\"ParamName\")", stored: true);

            modelBuilder
                .Entity<CeMatrixParam>()                
                .HasIndex(e => new { e.CeMatrixId, e.ParamNameLower })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique();

            modelBuilder
                .Entity<CeMatrixParam>()
                .HasIndex(e => new { e.CeMatrixId, e._CreateProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<CeMatrixParam>()
                .HasIndex(e => new { e.CeMatrixId, e._DeleteProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<CeMatrixDbFileReference>()
                .HasIndex(e => new { e.CeMatrixId, e.Path, e.Name })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique();

            modelBuilder
                .Entity<CeMatrixDbFileReference>()
                .HasIndex(e => new { e.CeMatrixId, e._CreateProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<CeMatrixDbFileReference>()
                .HasIndex(e => new { e.CeMatrixId, e._DeleteProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<Row>()
                .HasIndex(e => new { e.CeMatrixId, e.Order })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique();

            modelBuilder
                .Entity<Row>()
                .HasIndex(e => new { e.CeMatrixId, e._CreateProjectVersionNum })
                .IsUnique(false);

            modelBuilder
                .Entity<Row>()
                .HasIndex(e => new { e.CeMatrixId, e._DeleteProjectVersionNum })
                .IsUnique(false);

            modelBuilder
                .Entity<Column>()
                .HasIndex(e => new { e.CeMatrixId, e.Order })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique();

            modelBuilder
                .Entity<Column>()
                .HasIndex(e => new { e.CeMatrixId, e._CreateProjectVersionNum })
                .IsUnique(false);

            modelBuilder
                .Entity<Column>()
                .HasIndex(e => new { e.CeMatrixId, e._DeleteProjectVersionNum })
                .IsUnique(false);

            modelBuilder
                .Entity<Cell>()
                .HasIndex(e => new { e.CeMatrixId, e._CreateProjectVersionNum })
                .IsUnique(false);

            modelBuilder
                .Entity<Cell>()
                .HasIndex(e => new { e.CeMatrixId, e._DeleteProjectVersionNum })
                .IsUnique(false);

            modelBuilder
                .Entity<CeMatrixComment>()
                .Property(e => e.IdentifierLower)
                .HasComputedColumnSql("LOWER(\"Identifier\")", stored: true);

            modelBuilder
                .Entity<CeMatrixComment>()
                .HasIndex(e => new { e.CeMatrixId, e.IdentifierLower })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique();

            modelBuilder
                .Entity<CeMatrixComment>()
                .HasIndex(e => new { e.CeMatrixId, e._CreateProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<CeMatrixComment>()
                .HasIndex(e => new { e.CeMatrixId, e._DeleteProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<Tag>()
                .Property(e => e.IdentifierLower)
                .HasComputedColumnSql("LOWER(\"Identifier\")", stored: true);

            modelBuilder
                .Entity<Tag>()
                .HasIndex(e => new { e.ProjectId, e.IdentifierLower })                
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique();

            modelBuilder
                .Entity<Tag>()                
                .HasIndex(e => new { e.ProjectId, e._CreateProjectVersionNum })
                .IsUnique(false);

            modelBuilder
                .Entity<Tag>()
                .HasIndex(e => new { e.ProjectId, e._DeleteProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<TagParam>()
                .Property(e => e.ParamNameLower)
                .HasComputedColumnSql("LOWER(\"ParamName\")", stored: true);

            modelBuilder
                .Entity<TagParam>()
                .HasIndex(e => new { e.TagId, e.ParamNameLower })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique();

            modelBuilder
                .Entity<TagParam>()
                .HasIndex(e => new { e.TagId, e._CreateProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<TagParam>()
                .HasIndex(e => new { e.TagId, e._DeleteProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<TagCondition>()
                .HasIndex(e => new { e.TagId, e.ConditionCategory, e.AeCondition, e.DaCondition, e.SymbolToDisplay })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique();

            modelBuilder
                .Entity<TagCondition>()
                .HasIndex(e => new { e.TagId, e._CreateProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<TagCondition>()
                .HasIndex(e => new { e.TagId, e._DeleteProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<TagDbFileReference>()
                .HasIndex(e => new { e.TagId, e.Path, e.Name })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique();

            modelBuilder
                .Entity<TagDbFileReference>()
                .HasIndex(e => new { e.TagId, e._CreateProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<TagDbFileReference>()
                .HasIndex(e => new { e.TagId, e._DeleteProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<BaseActuator>()
                .Property(e => e.IdentifierLower)
                .HasComputedColumnSql("LOWER(\"Identifier\")", stored: true);

            modelBuilder
                .Entity<BaseActuator>()
                .HasIndex(e => new { e.ProjectId, e.IdentifierLower })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique();

            modelBuilder
                .Entity<BaseActuator>()
                .HasIndex(e => new { e.ProjectId, e._CreateProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<BaseActuator>()
                .HasIndex(e => new { e.ProjectId, e._DeleteProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<BaseActuatorParam>()
                .Property(e => e.ParamNameLower)
                .HasComputedColumnSql("LOWER(\"ParamName\")", stored: true);

            modelBuilder
                .Entity<BaseActuatorParam>()
                .HasIndex(e => new { e.BaseActuatorId, e.ParamNameLower })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique();

            modelBuilder
                .Entity<BaseActuatorParam>()
                .HasIndex(e => new { e.BaseActuatorId, e._CreateProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<BaseActuatorParam>()
                .HasIndex(e => new { e.BaseActuatorId, e._DeleteProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<BaseActuatorDbFileReference>()
                .HasIndex(e => new { e.BaseActuatorId, e.Path, e.Name })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique();

            modelBuilder
                .Entity<BaseActuatorDbFileReference>()
                .HasIndex(e => new { e.BaseActuatorId, e._CreateProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<BaseActuatorDbFileReference>()
                .HasIndex(e => new { e.BaseActuatorId, e._DeleteProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<SafetyController>()
                .Property(e => e.IdentifierLower)
                .HasComputedColumnSql("LOWER(\"Identifier\")", stored: true);

            modelBuilder
                .Entity<SafetyController>()
                .HasIndex(e => new { e.ProjectId, e.IdentifierLower })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique();

            modelBuilder
                .Entity<SafetyController>()
                .HasIndex(e => new { e.ProjectId, e._CreateProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<SafetyController>()
                .HasIndex(e => new { e.ProjectId, e._DeleteProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<SafetyControllerParam>()
                .Property(e => e.ParamNameLower)
                .HasComputedColumnSql("LOWER(\"ParamName\")", stored: true);

            modelBuilder
                .Entity<SafetyControllerParam>()
                .HasIndex(e => new { e.SafetyControllerId, e.ParamNameLower })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique();

            modelBuilder
                .Entity<SafetyControllerParam>()
                .HasIndex(e => new { e.SafetyControllerId, e._CreateProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<SafetyControllerParam>()
                .HasIndex(e => new { e.SafetyControllerId, e._DeleteProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<SafetyControllerDbFileReference>()
                .HasIndex(e => new { e.SafetyControllerId, e.Path, e.Name })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique();

            modelBuilder
                .Entity<SafetyControllerDbFileReference>()
                .HasIndex(e => new { e.SafetyControllerId, e._CreateProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<SafetyControllerDbFileReference>()
                .HasIndex(e => new { e.SafetyControllerId, e._DeleteProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<Legend>()
                .Property(e => e.IdentifierLower)
                .HasComputedColumnSql("LOWER(\"Identifier\")", stored: true);

            modelBuilder
                .Entity<Legend>()
                .HasIndex(e => new { e.ProjectId, e.IdentifierLower })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique();

            modelBuilder
                .Entity<Legend>()
                .HasIndex(e => new { e.ProjectId, e._CreateProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<Legend>()
                .HasIndex(e => new { e.ProjectId, e._DeleteProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<LegendParam>()
                .Property(e => e.ParamNameLower)
                .HasComputedColumnSql("LOWER(\"ParamName\")", stored: true);

            modelBuilder
                .Entity<LegendParam>()
                .HasIndex(e => new { e.LegendId, e.ParamNameLower })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique();

            modelBuilder
                .Entity<LegendParam>()
                .HasIndex(e => new { e.LegendId, e._CreateProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<LegendParam>()
                .HasIndex(e => new { e.LegendId, e._DeleteProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<LegendDbFileReference>()
                .HasIndex(e => new { e.LegendId, e.Path, e.Name })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique();

            modelBuilder
                .Entity<LegendDbFileReference>()
                .HasIndex(e => new { e.LegendId, e._CreateProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<LegendDbFileReference>()
                .HasIndex(e => new { e.LegendId, e._DeleteProjectVersionNum })                
                .IsUnique(false);

            modelBuilder
                .Entity<BasePcObject>()
                .Property(e => e.IdentifierLower)
                .HasComputedColumnSql("LOWER(\"Identifier\")", stored: true);

            modelBuilder
                .Entity<BasePcObject>()
                .HasIndex(e => new { e.UnitId, e.IdentifierLower })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique();

            modelBuilder
                .Entity<PcObject>()
                .Property(e => e.IdentifierLower)
                .HasComputedColumnSql("LOWER(\"Identifier\")", stored: true);

            modelBuilder
                .Entity<PcObject>()
                .HasIndex(e => new { e.UnitId, e.IdentifierLower })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique();

            modelBuilder
                .Entity<PcObjectEvent>()
                .Property(e => e.PcObjectEventTypeLower)
                .HasComputedColumnSql("LOWER(\"PcObjectEventType\")", stored: true);

            modelBuilder
                .Entity<PcObjectEvent>()
                .HasIndex(e => new { e.PcObjectId, e.PcObjectEventTypeLower, e.BeginTimeUtc })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique(false);

            modelBuilder
                .Entity<PcObjectEvent>()
                .HasIndex(e => new { e.BeginTimeUtc })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique(false);

            modelBuilder
                .Entity<PcObjectEvent>()
                .HasIndex(e => new { e.EndTimeUtc })
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique(false);

            modelBuilder
                .Entity<Unit>()
                .Property(e => e.IdentifierLower)
                .HasComputedColumnSql("LOWER(\"Identifier\")", stored: true);

            modelBuilder
                .Entity<Unit>()
                .HasIndex(e => e.IdentifierLower)
                .HasFilter("\"_IsDeleted\" = FALSE")
                .IsUnique();

            modelBuilder
                .Entity<ProjectVersionType>()
                .Property(e => e.TypeLower)
                .HasComputedColumnSql("LOWER(\"Type\")", stored: true);

            modelBuilder
                .Entity<ProjectVersionType>()
                .HasIndex(e => e.TypeLower)
                .IsUnique();

            modelBuilder
                .Entity<ProjectVersionType>()
                .HasMany(p => p.StandardParamInfos)
                .WithMany(p => p.ProjectVersionTypes)
                .UsingEntity(j => j.ToTable("ProjectVersionTypes_ParamInfos")); // Explicit name            

            modelBuilder
                .Entity<PcObjectEventType>()
                .Property(e => e.TypeLower)
                .HasComputedColumnSql("LOWER(\"Type\")", stored: true);            

            modelBuilder
                .Entity<PcObjectEventType>()
                .HasIndex(e => e.TypeLower)
                .IsUnique();

            modelBuilder
                .Entity<PcObjectEventType>()
                .HasMany(p => p.StandardParamInfos)
                .WithMany(p => p.PcObjectEventTypes)
                .UsingEntity(j => j.ToTable("PcObjectEventTypes_ParamInfos")); // Explicit name
                                                                               // 
            modelBuilder
                .Entity<ParamInfo>()
                .Property(e => e.ParamNameLower)
                .HasComputedColumnSql("LOWER(\"ParamName\")", stored: true);

            modelBuilder
                .Entity<ParamDesc>()
                .Property(e => e.IdLower)
                .HasComputedColumnSql("LOWER(\"Id\")", stored: true);

            modelBuilder
                .Entity<ParamDesc>()
                .HasIndex(e => e.IdLower)
                .IsUnique();

            modelBuilder
                .Entity<CryptoEntity>()
                .Property(e => e.IdentifierLower)
                .HasComputedColumnSql("LOWER(\"Identifier\")", stored: true);

            modelBuilder
                .Entity<CryptoEntity>()
                .HasIndex(e => e.IdentifierLower)
                .IsUnique();

            modelBuilder
                .Entity<MetaParam>()
                .Property(e => e.ParamNameLower)
                .HasComputedColumnSql("LOWER(\"ParamName\")", stored: true);

            modelBuilder
                .Entity<MetaParam>()
                .HasKey(e => e.ParamName);

            modelBuilder
                .Entity<MetaParam>()
                .HasIndex(e => new { e.ParamNameLower })
                .IsUnique();

            modelBuilder
                .Entity<MetaParamArg>()
                .Property(e => e.ParamNameLower)
                .HasComputedColumnSql("LOWER(\"ParamName\")", stored: true);

            modelBuilder
                .Entity<MetaParamArg>()
                .HasKey(e => e.ParamName);

            modelBuilder
                .Entity<MetaParamArg>()
                .HasIndex(e => new { e.ParamNameLower })
                .IsUnique();

            modelBuilder
                .Entity<BasePcObject>()
                .HasMany(p => p.PcObjectEventTypes)
                .WithMany(p => p.BasePcObjects)
                .UsingEntity(j => j.ToTable("BasePcObjects_EventTypes")); // Explicit name
                                                                          // 
            modelBuilder
                .Entity<BasePcObjectJournalParam>()
                .Property(e => e.ParamNameLower)
                .HasComputedColumnSql("LOWER(\"ParamName\")", stored: true);

            modelBuilder
                .Entity<BasePcObjectJournalParam>()
                .HasIndex(e => new { e.BasePcObjectId, e.ParamNameLower })                
                .IsUnique();

            modelBuilder
                .Entity<PcObjectJournalParam>()
                .Property(e => e.ParamNameLower)
                .HasComputedColumnSql("LOWER(\"ParamName\")", stored: true);

            modelBuilder
                .Entity<PcObjectJournalParam>()
                .HasIndex(e => new { e.PcObjectId, e.ParamNameLower })                
                .IsUnique();

            modelBuilder
                .Entity<JournalParamValuesCollection>()
                .Property(e => e.ParamNameLower)
                .HasComputedColumnSql("LOWER(\"ParamName\")", stored: true);

            modelBuilder
                .Entity<JournalParamValuesCollection>()
                .HasIndex(e => new { e.PcObjectId, e.ParamNameLower })
                .IsUnique();
        }

        #endregion

        #region private functions

        private void OnSavingChanges(object? sender, SavingChangesEventArgs e)
        {
            DateTime utcNow = DateTime.UtcNow;

            var entries = ChangeTracker.Entries().ToArray(); // Updates all entity cross-references

            HttpContext? httpContext = HttpContextAccessor?.HttpContext;
            string user;
            string hubConnectionIds = @"";
            if (!String.IsNullOrEmpty(User))
                user = User;
            else
                user = HttpContextHelper.GetUserLowerInvariant(httpContext);
            if (httpContext is not null)
            {
                var header = httpContext.Request.Headers["HubConnectionId"];
                if (header.Count > 0)
                    hubConnectionIds = CsvHelper.FormatForCsv(@",", header.ToArray());
            }

            using PazCheckDbContext? secondaryDbContext = (!IsLastChangeFieldsUpdatingDisabled || !IsInformationSecurityEventsLoggingDisabled) ?
                DbContextFactory?.CreateDbContext() : null;
            if (secondaryDbContext is not null)
            {
                secondaryDbContext.IsLastChangeFieldsUpdatingDisabled = true;
                secondaryDbContext.IsInformationSecurityEventsLoggingDisabled = true;
            }

            foreach (var entry in entries)
            {
                if (entry.Entity is VersionedEntityBase versionedEntity)
                {
                    bool isAdded = false;
                    bool isDeleted = false;
                    if (entry.State == EntityState.Added)
                    {
                        isAdded = true;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        PropertyEntry propertyEntry = entry.Property(nameof(VersionedEntityBase._IsDeleted));
                        if ((bool)propertyEntry.CurrentValue! != (bool)propertyEntry.OriginalValue!)
                            isDeleted = true;
                    }
                    else if (entry.State == EntityState.Deleted)
                    {
                        isDeleted = true;
                    }

                    ProjectVersionedEntityBase? parentProjectVersionedEntity = null;
                    Type? parentProjectVersionedEntityType = null;                    
                    if (versionedEntity.HasParentProjectVersionedEntity())
                    {
                        parentProjectVersionedEntityType = versionedEntity.GetParentProjectVersionedEntity_PropertyType();
                        parentProjectVersionedEntity = versionedEntity.TryGetParentProjectVersionedEntity();
                    }

                    if (!IsLastChangeFieldsUpdatingDisabled)
                    {
                        if (isAdded)
                        {
                            versionedEntity._CreateTimeUtc = utcNow;
                            versionedEntity._CreateUser = user;
                        }
                        if (isAdded || isDeleted)
                        {
                            versionedEntity._LastChangeTimeUtc = utcNow;
                            versionedEntity._LastChangeUser = user;

                            versionedEntity._HasUnversionedChanges = true;

                            if (versionedEntity is ProjectVersionedEntityBase projectVersionedEntity)
                            {
                                _project_ChangedMessages.Add(new Project_ChangedMessage() { 
                                    ProjectId = projectVersionedEntity.ProjectId,
                                    HubConnectionIds = hubConnectionIds
                                });
                            }
                            else if (parentProjectVersionedEntityType is not null)
                            {
                                if (parentProjectVersionedEntity is null ||
                                        parentProjectVersionedEntity.ProjectId < 1)
                                    parentProjectVersionedEntity = LoadParentProjectVersionedEntityExtended(
                                        secondaryDbContext,
                                        parentProjectVersionedEntityType,
                                        versionedEntity.GetParentProjectVersionedEntity_Id());

                                if (parentProjectVersionedEntity is not null)
                                {
                                    parentProjectVersionedEntity._LastChangeTimeUtc = versionedEntity._LastChangeTimeUtc;
                                    parentProjectVersionedEntity._LastChangeUser = versionedEntity._LastChangeUser;

                                    parentProjectVersionedEntity._HasUnversionedChanges = true;

                                    _project_ChangedMessages.Add(new Project_ChangedMessage() {
                                        ProjectId = parentProjectVersionedEntity.ProjectId,
                                        HubConnectionIds = hubConnectionIds
                                    });                                    
                                }                                    
                            }
                        }
                    }

                    if (!IsInformationSecurityEventsLoggingDisabled)
                    {
                        if ((isAdded || isDeleted) &&
                            parentProjectVersionedEntityType is not null)
                        {
                            EntityName_PropertyInfos.TryGetValue(parentProjectVersionedEntityType.Name, out PropertyInfo? propertyInfo);
                            if (propertyInfo is not null && Attribute.IsDefined(propertyInfo, typeof(InformationSecurityEventEntityAttribute)))
                            {
                                if (parentProjectVersionedEntity is null ||
                                        parentProjectVersionedEntity.Project is null ||
                                        parentProjectVersionedEntity.Project.Unit is null)
                                    parentProjectVersionedEntity = LoadParentProjectVersionedEntityExtended(
                                        secondaryDbContext,
                                        parentProjectVersionedEntityType,
                                        versionedEntity.GetParentProjectVersionedEntity_Id());

                                if (parentProjectVersionedEntity is not null)
                                {
                                    var informationSecurityEventEntityAttribute = propertyInfo.GetCustomAttribute<InformationSecurityEventEntityAttribute>()!;
                                    var pcDisplayNameAttribute = propertyInfo.GetCustomAttribute<PcDisplayNameAttribute>()!;

                                    CaseInsensitiveOrderedDictionary<string?> parentEventAdditionalFields = new();
                                    parentEventAdditionalFields[Properties.Resources.Project] = parentProjectVersionedEntity.Project!.Title;
                                    parentEventAdditionalFields[Properties.Resources.Unit] = parentProjectVersionedEntity.Project!.Unit!.Title;

                                    string enitityTypeAndIdentifier = pcDisplayNameAttribute.DisplayName + @": " + parentProjectVersionedEntity.Identifier;
                                    InformationSecurityEventsLogger_?.InformationSecurityEvent(user,
                                                    HttpContextHelper.GetSourceIpAddress(httpContext),
                                                    HttpContextHelper.GetSourceHost(httpContext),
                                                    informationSecurityEventEntityAttribute.AllRolesAccess ?
                                                        InformationSecurityEventsLogger.EntityChanged_AllRolesAccessEventId :
                                                        InformationSecurityEventsLogger.EntityChanged_EventId,
                                                    6,
                                                    true,
                                                    Properties.Resources.EntityModified_EventName,
                                                    user,
                                                    enitityTypeAndIdentifier,
                                                    NameValueCollectionHelper.GetNameValueCollectionString(parentEventAdditionalFields),
                                                    Properties.Resources.EntityModified_EventName + @" - " + enitityTypeAndIdentifier);
                                }                                    
                            }
                        }

                        CaseInsensitiveOrderedDictionary<string?> eventAdditionalFields = new();
                        if (versionedEntity is ProjectVersionedEntityBase projectVersionedEntity)
                        {
                            entry.Reference(nameof(ProjectVersionedEntityBase.Project)).Load();
                            if (projectVersionedEntity.Project is not null)
                            {
                                eventAdditionalFields[Properties.Resources.Project] = projectVersionedEntity.Project.Title;

                                Entry(projectVersionedEntity.Project).Reference(nameof(Project.Unit)).Load();
                                eventAdditionalFields[Properties.Resources.Unit] = projectVersionedEntity.Project.Unit?.Title;
                            }
                        }

                        if (isAdded)
                        {
                            EntityName_PropertyInfos.TryGetValue(entry.Entity.GetType().Name, out PropertyInfo? propertyInfo);
                            if (propertyInfo is not null && Attribute.IsDefined(propertyInfo, typeof(InformationSecurityEventEntityAttribute)))
                            {
                                var informationSecurityEventEntityAttribute = propertyInfo!.GetCustomAttribute<InformationSecurityEventEntityAttribute>()!;
                                var pcDisplayNameAttribute = propertyInfo!.GetCustomAttribute<PcDisplayNameAttribute>()!;
                                string enitityTypeAndIdentifier = pcDisplayNameAttribute.DisplayName + @": " + entry.Entity.ToString();
                                InformationSecurityEventsLogger_?.InformationSecurityEvent(user,
                                            HttpContextHelper.GetSourceIpAddress(httpContext),
                                            HttpContextHelper.GetSourceHost(httpContext),
                                            informationSecurityEventEntityAttribute.AllRolesAccess ?
                                                InformationSecurityEventsLogger.EntityChanged_AllRolesAccessEventId :
                                                InformationSecurityEventsLogger.EntityChanged_EventId,
                                            6,
                                            true,
                                            Properties.Resources.EntityAdded_EventName,
                                            user,
                                            enitityTypeAndIdentifier,
                                            NameValueCollectionHelper.GetNameValueCollectionString(eventAdditionalFields),
                                            Properties.Resources.EntityAdded_EventName + @" - " + enitityTypeAndIdentifier);
                            }                            
                        }
                        else if (isDeleted)
                        {
                            EntityName_PropertyInfos.TryGetValue(entry.Entity.GetType().Name, out PropertyInfo? propertyInfo);
                            if (propertyInfo is not null && Attribute.IsDefined(propertyInfo, typeof(InformationSecurityEventEntityAttribute)))
                            {
                                var informationSecurityEventEntityAttribute = propertyInfo!.GetCustomAttribute<InformationSecurityEventEntityAttribute>()!;
                                var pcDisplayNameAttribute = propertyInfo!.GetCustomAttribute<PcDisplayNameAttribute>()!;
                                string enitityTypeAndIdentifier = pcDisplayNameAttribute.DisplayName + @": " + entry.Entity.ToString();
                                InformationSecurityEventsLogger_?.InformationSecurityEvent(user,
                                            HttpContextHelper.GetSourceIpAddress(httpContext),
                                            HttpContextHelper.GetSourceHost(httpContext),
                                            informationSecurityEventEntityAttribute.AllRolesAccess ?
                                                InformationSecurityEventsLogger.EntityChanged_AllRolesAccessEventId :
                                                InformationSecurityEventsLogger.EntityChanged_EventId,
                                            7,
                                            true,
                                            Properties.Resources.EntityDeleted_EventName,
                                            user,
                                            enitityTypeAndIdentifier,
                                            NameValueCollectionHelper.GetNameValueCollectionString(eventAdditionalFields),
                                            Properties.Resources.EntityDeleted_EventName + @" - " + enitityTypeAndIdentifier);
                            }                            
                        }
                    }
                }
                else if (entry.Entity is ICreateDeleteInfoEntity createDeleteInfoEntity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        if (!IsLastChangeFieldsUpdatingDisabled)
                        {
                            createDeleteInfoEntity._CreateTimeUtc = utcNow;
                            createDeleteInfoEntity._CreateUser = user;

                            createDeleteInfoEntity._LastChangeTimeUtc = utcNow;
                            createDeleteInfoEntity._LastChangeUser = user;
                        }

                        if (!IsInformationSecurityEventsLoggingDisabled)
                        {
                            EntityName_PropertyInfos.TryGetValue(entry.Entity.GetType().Name, out PropertyInfo? propertyInfo);
                            if (propertyInfo is not null && Attribute.IsDefined(propertyInfo, typeof(InformationSecurityEventEntityAttribute)))
                            {
                                var informationSecurityEventEntityAttribute = propertyInfo!.GetCustomAttribute<InformationSecurityEventEntityAttribute>()!;
                                var pcDisplayNameAttribute = propertyInfo!.GetCustomAttribute<PcDisplayNameAttribute>()!;
                                string enitityTypeAndIdentifier = pcDisplayNameAttribute.DisplayName + @": " + entry.Entity.ToString();
                                InformationSecurityEventsLogger_?.InformationSecurityEvent(user,
                                            HttpContextHelper.GetSourceIpAddress(httpContext),
                                            HttpContextHelper.GetSourceHost(httpContext),
                                            informationSecurityEventEntityAttribute.AllRolesAccess ?
                                                InformationSecurityEventsLogger.EntityChanged_AllRolesAccessEventId :
                                                InformationSecurityEventsLogger.EntityChanged_EventId,
                                            6,
                                            true,
                                            Properties.Resources.EntityAdded_EventName,
                                            user,
                                            enitityTypeAndIdentifier,
                                            @"",
                                            Properties.Resources.EntityAdded_EventName + @" - " + enitityTypeAndIdentifier);
                            }
                        }
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        if (!IsLastChangeFieldsUpdatingDisabled)
                        {
                            createDeleteInfoEntity._LastChangeTimeUtc = utcNow;
                            createDeleteInfoEntity._LastChangeUser = user;
                        }

                        if (!IsInformationSecurityEventsLoggingDisabled)
                        {
                            PropertyEntry propertyEntry = entry.Property(nameof(ICreateDeleteInfoEntity._IsDeleted));
                            if ((bool)propertyEntry.CurrentValue! && !(bool)propertyEntry.OriginalValue!)
                            {
                                EntityName_PropertyInfos.TryGetValue(entry.Entity.GetType().Name, out PropertyInfo? propertyInfo);
                                if (propertyInfo is not null && Attribute.IsDefined(propertyInfo, typeof(InformationSecurityEventEntityAttribute)))
                                {
                                    var informationSecurityEventEntityAttribute = propertyInfo!.GetCustomAttribute<InformationSecurityEventEntityAttribute>()!;
                                    var pcDisplayNameAttribute = propertyInfo!.GetCustomAttribute<PcDisplayNameAttribute>()!;
                                    string enitityTypeAndIdentifier = pcDisplayNameAttribute.DisplayName + @": " + entry.Entity.ToString();
                                    InformationSecurityEventsLogger_?.InformationSecurityEvent(user,
                                                HttpContextHelper.GetSourceIpAddress(httpContext),
                                                HttpContextHelper.GetSourceHost(httpContext),
                                                informationSecurityEventEntityAttribute.AllRolesAccess ?
                                                    InformationSecurityEventsLogger.EntityChanged_AllRolesAccessEventId :
                                                    InformationSecurityEventsLogger.EntityChanged_EventId,
                                                7,
                                                true,
                                                Properties.Resources.EntityDeleted_EventName,
                                                user,
                                                enitityTypeAndIdentifier,
                                                @"",
                                                Properties.Resources.EntityDeleted_EventName + @" - " + enitityTypeAndIdentifier);
                                }
                            }
                            else
                            {
                                EntityName_PropertyInfos.TryGetValue(entry.Entity.GetType().Name, out PropertyInfo? propertyInfo);
                                if (propertyInfo is not null && Attribute.IsDefined(propertyInfo, typeof(InformationSecurityEventEntityAttribute)))
                                {
                                    var informationSecurityEventEntityAttribute = propertyInfo!.GetCustomAttribute<InformationSecurityEventEntityAttribute>()!;
                                    var pcDisplayNameAttribute = propertyInfo!.GetCustomAttribute<PcDisplayNameAttribute>()!;
                                    string enitityTypeAndIdentifier = pcDisplayNameAttribute.DisplayName + @": " + entry.Entity.ToString();
                                    InformationSecurityEventsLogger_?.InformationSecurityEvent(user,
                                                HttpContextHelper.GetSourceIpAddress(httpContext),
                                                HttpContextHelper.GetSourceHost(httpContext),
                                                informationSecurityEventEntityAttribute.AllRolesAccess ?
                                                    InformationSecurityEventsLogger.EntityChanged_AllRolesAccessEventId :
                                                    InformationSecurityEventsLogger.EntityChanged_EventId,
                                                6,
                                                true,
                                                Properties.Resources.EntityModified_EventName,
                                                user,
                                                enitityTypeAndIdentifier,
                                                NameValueCollectionHelper.GetNameValueCollectionString(PazCheckDbHelper.GetChanges(entry, entry.GetDatabaseValues())),
                                                Properties.Resources.EntityModified_EventName + @" - " + enitityTypeAndIdentifier);
                                }
                            }
                        }
                    }
                }
                else if (entry.Entity is RolePermission rolePermission)
                {
                    if (entry.State == EntityState.Modified && !IsInformationSecurityEventsLoggingDisabled)
                    {
                        PropertyEntry propertyEntry = entry.Property(nameof(RolePermission.IsAllowed));
                        bool currentValue = (bool)propertyEntry.CurrentValue!;
                        if (currentValue != (bool)propertyEntry.OriginalValue!)
                        {
                            entry.Reference(nameof(RolePermission.Role)).Load();
                            entry.Reference(nameof(RolePermission.RoleBusinessFunction)).Load();
                            InformationSecurityEventsLogger_?.InformationSecurityEvent(user,
                                        HttpContextHelper.GetSourceIpAddress(httpContext),
                                        HttpContextHelper.GetSourceHost(httpContext),
                                        InformationSecurityEventsLogger.RoleAttributesChanged_EventId,
                                        8,
                                        true,
                                        Properties.Resources.RoleAttributesChanged_EventName,
                                        user,
                                        Properties.Resources.Role + @" - " + rolePermission.Role.ToString(),
                                        NameValueCollectionHelper.GetNameValueCollectionString(new (string, string?)[]
                                        {
                                            (@"Role", rolePermission.Role.Identifier),
                                            (@"BusinessFunction", rolePermission.RoleBusinessFunction.Identifier),
                                            (@"IsAllowed", new Any().ValueAsString(false))
                                        }),
                                        Properties.Resources.RoleAttributesChanged_EventDesc,
                                        rolePermission.Role.Desc,
                                        rolePermission.RoleBusinessFunction.Desc,
                                        currentValue ? Properties.Resources.IsAllowed : Properties.Resources.IsDisallowed);
                        }
                    }
                }
                else if (!IsInformationSecurityEventsLoggingDisabled)
                {
                    if (entry.State == EntityState.Added)
                    {
                        EntityName_PropertyInfos.TryGetValue(entry.Entity.GetType().Name, out PropertyInfo? propertyInfo);
                        if (propertyInfo is not null && Attribute.IsDefined(propertyInfo, typeof(InformationSecurityEventEntityAttribute)))
                        {
                            var informationSecurityEventEntityAttribute = propertyInfo!.GetCustomAttribute<InformationSecurityEventEntityAttribute>()!;
                            var pcDisplayNameAttribute = propertyInfo!.GetCustomAttribute<PcDisplayNameAttribute>()!;
                            string enitityTypeAndIdentifier = pcDisplayNameAttribute.DisplayName + @": " + entry.Entity.ToString();
                            InformationSecurityEventsLogger_?.InformationSecurityEvent(user,
                                        HttpContextHelper.GetSourceIpAddress(httpContext),
                                        HttpContextHelper.GetSourceHost(httpContext),
                                        informationSecurityEventEntityAttribute.AllRolesAccess ?
                                            InformationSecurityEventsLogger.EntityChanged_AllRolesAccessEventId :
                                            InformationSecurityEventsLogger.EntityChanged_EventId,
                                        6,
                                        true,
                                        Properties.Resources.EntityAdded_EventName,
                                        user,
                                        enitityTypeAndIdentifier,
                                        @"",
                                        Properties.Resources.EntityAdded_EventName + @" - " + enitityTypeAndIdentifier);
                        }
                    }
                    else if (entry.State == EntityState.Deleted)
                    {
                        EntityName_PropertyInfos.TryGetValue(entry.Entity.GetType().Name, out PropertyInfo? propertyInfo);
                        if (propertyInfo is not null && Attribute.IsDefined(propertyInfo, typeof(InformationSecurityEventEntityAttribute)))
                        {
                            var informationSecurityEventEntityAttribute = propertyInfo!.GetCustomAttribute<InformationSecurityEventEntityAttribute>()!;
                            var pcDisplayNameAttribute = propertyInfo!.GetCustomAttribute<PcDisplayNameAttribute>()!;
                            string enitityTypeAndIdentifier = pcDisplayNameAttribute.DisplayName + @": " + entry.Entity.ToString();
                            InformationSecurityEventsLogger_?.InformationSecurityEvent(user,
                                        HttpContextHelper.GetSourceIpAddress(httpContext),
                                        HttpContextHelper.GetSourceHost(httpContext),
                                        informationSecurityEventEntityAttribute.AllRolesAccess ?
                                            InformationSecurityEventsLogger.EntityChanged_AllRolesAccessEventId :
                                            InformationSecurityEventsLogger.EntityChanged_EventId,
                                        7,
                                        true,
                                        Properties.Resources.EntityDeleted_EventName,
                                        user,
                                        enitityTypeAndIdentifier,
                                        @"",
                                        Properties.Resources.EntityDeleted_EventName + @" - " + enitityTypeAndIdentifier);
                        }
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        EntityName_PropertyInfos.TryGetValue(entry.Entity.GetType().Name, out PropertyInfo? propertyInfo);
                        if (propertyInfo is not null && Attribute.IsDefined(propertyInfo, typeof(InformationSecurityEventEntityAttribute)))
                        {
                            var informationSecurityEventEntityAttribute = propertyInfo!.GetCustomAttribute<InformationSecurityEventEntityAttribute>()!;
                            var pcDisplayNameAttribute = propertyInfo!.GetCustomAttribute<PcDisplayNameAttribute>()!;
                            string enitityTypeAndIdentifier = pcDisplayNameAttribute.DisplayName + @": " + entry.Entity.ToString();
                            InformationSecurityEventsLogger_?.InformationSecurityEvent(user,
                                        HttpContextHelper.GetSourceIpAddress(httpContext),
                                        HttpContextHelper.GetSourceHost(httpContext),
                                        informationSecurityEventEntityAttribute.AllRolesAccess ?
                                            InformationSecurityEventsLogger.EntityChanged_AllRolesAccessEventId :
                                            InformationSecurityEventsLogger.EntityChanged_EventId,
                                        6,
                                        true,
                                        Properties.Resources.EntityModified_EventName,
                                        user,
                                        enitityTypeAndIdentifier,
                                        NameValueCollectionHelper.GetNameValueCollectionString(PazCheckDbHelper.GetChanges(entry, entry.GetDatabaseValues())),
                                        Properties.Resources.EntityModified_EventName + @" - " + enitityTypeAndIdentifier);
                        }
                    }
                }

                if (entry.State == EntityState.Added ||
                    entry.State == EntityState.Deleted)
                {
                    // See also  PazCheckDbHelper.UpdateMetaParams(...)
                    if (entry.Entity is InformationSecurityEvent ||
                        entry.Entity is AllRolesAccessInformationSecurityEvent ||
                        entry.Entity is UserEvent ||
                        entry.Entity is InformationMessage ||
                        entry.Entity is RequestMessage ||
                        entry.Entity is UnitEventsInterval ||
                        entry.Entity is UnitEvent)
                    {
                        _updateValue_MetaParamNames.Add(entry.Entity.GetType().Name + @"_Changed_Guid");
                    }

                    if (entry.Entity is ParamDesc ||
                        entry.Entity is Unit ||                        
                        entry.Entity is BasePcObject ||
                        entry.Entity is BasePcObjectJournalParam ||                                               
                        entry.Entity is PcObjectJournalParam ||                        
                        entry.Entity is PcObjectEvent ||
                        entry.Entity is PcObjectEventType)
                    {
                        _updateValue_MetaParamNames.Add(PazCheckConstants.MetaParamNameBase_ParamDescs_Units_BasePcObjects_PcObjects_Guid);
                    }
                    else if (entry.Entity is PcObject)
                    {
                        _updateValue_MetaParamNames.Add(PazCheckConstants.MetaParamNameBase_ParamDescs_Units_BasePcObjects_PcObjects_Guid);
                    }

                    if (entry.Entity is JournalParamValuesCollection)
                    {
                        _updateValue_MetaParamNames.Add(PazCheckConstants.MetaParamNameBase_JournalParamValuesCollection_Guid);
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    // See also  PazCheckDbHelper.UpdateMetaParams(...)
                    if (entry.Entity is InformationSecurityEvent ||
                        entry.Entity is AllRolesAccessInformationSecurityEvent ||
                        entry.Entity is UserEvent ||
                        entry.Entity is InformationMessage ||
                        entry.Entity is RequestMessage ||
                        entry.Entity is UnitEventsInterval ||
                        entry.Entity is UnitEvent)
                    {
                        _updateValue_MetaParamNames.Add(entry.Entity.GetType().Name + @"_Changed_Guid");
                    }

                    if (entry.Entity is ParamDesc ||
                        entry.Entity is Unit ||
                        entry.Entity is BasePcObject ||
                        entry.Entity is BasePcObjectJournalParam ||
                        entry.Entity is PcObjectJournalParam ||
                        entry.Entity is PcObjectEvent ||
                        entry.Entity is PcObjectEventType)
                    {
                        _updateValue_MetaParamNames.Add(PazCheckConstants.MetaParamNameBase_ParamDescs_Units_BasePcObjects_PcObjects_Guid);
                    }
                    else if (entry.Entity is PcObject)
                    {
                        var excluded = new[] { nameof(PcObject.SafetyIndex), nameof(PcObject.SafetyIndexDesc), nameof(PcObject.SafetyIndex_LastChangeTimeUtc) };
                        var modifiedOtherThanExcluded = entry.Properties
                            .Where(p => !excluded.Contains(p.Metadata.Name) && p.IsModified)
                            .Any();
                        if (modifiedOtherThanExcluded)
                        {
                            _updateValue_MetaParamNames.Add(PazCheckConstants.MetaParamNameBase_ParamDescs_Units_BasePcObjects_PcObjects_Guid);
                        }
                    }

                    if (entry.Entity is JournalParamValuesCollection)
                    {
                        _updateValue_MetaParamNames.Add(PazCheckConstants.MetaParamNameBase_JournalParamValuesCollection_Guid);
                    }
                }
            }        

            secondaryDbContext?.SaveChanges();
        }

        private void OnSavedChanges(object? sender, SavedChangesEventArgs e)
        {
            if ((_updateValue_MetaParamNames.Count > 0 || _project_ChangedMessages.Count > 0) &&
                    DbContextFactory is not null)
            {
                using PazCheckDbContext secondaryDbContext = DbContextFactory.CreateDbContext();
                secondaryDbContext.IsLastChangeFieldsUpdatingDisabled = true;
                secondaryDbContext.IsInformationSecurityEventsLoggingDisabled = true;

                bool secondaryDbContext_Changed = false;

                var metaParams = secondaryDbContext.MetaParams.ToCaseInsensitiveOrderedDictionary(mp => mp.ParamName);

                if (_updateValue_MetaParamNames.Count > 0)
                {
                    var nowUtc = DateTime.UtcNow;
                    foreach (var metaParamName in _updateValue_MetaParamNames)
                    {
                        if (metaParams.TryGetValue(metaParamName, out var metaParam))
                        {
                            metaParam.Value = Guid.NewGuid().ToString();
                            metaParam._LastChangeTimeUtc = nowUtc;
                        }
                    }

                    _updateValue_MetaParamNames.Clear();
                    secondaryDbContext_Changed = true;
                }

                if (_project_ChangedMessages.Count > 0)
                {
                    string newDataGuid = Guid.NewGuid().ToString();
                    foreach (var project_ChangedMessage in _project_ChangedMessages)
                    {
                        if (!DisableProjectGuidUpdate)
                            secondaryDbContext.Database.ExecuteSql($"UPDATE \"Projects\" SET \"DataGuid\" = {newDataGuid} WHERE \"Id\" = {project_ChangedMessage.ProjectId}");

                        PazCheckDbHelper.AddOrUpdateMetaParam_Project(
                            secondaryDbContext,
                            metaParams,
                            project_ChangedMessage);
                    }
                    _project_ChangedMessages.Clear();
                    secondaryDbContext_Changed = true;
                }

                if (secondaryDbContext_Changed)
                    try
                    {
                        secondaryDbContext.SaveChanges();
                    }
                    catch
                    {
                    }
            }
        }

        private static ProjectVersionedEntityBase? LoadParentProjectVersionedEntityExtended(
            PazCheckDbContext? secondaryDbContext, 
            Type parentProjectVersionedEntityType,
            int parentProjectVersionedEntity_Id)
        {
            if (secondaryDbContext is null)
                return null;

            switch (parentProjectVersionedEntityType.Name)
            {
                case nameof(CeMatrix):
                    return secondaryDbContext.CeMatrices
                        .Include(m => m.Project)
                        .ThenInclude(p => p.Unit)
                        .FirstOrDefault(m => m.Id == parentProjectVersionedEntity_Id);                    
                case nameof(Tag):
                    return secondaryDbContext.Tags
                        .Include(m => m.Project)
                        .ThenInclude(p => p.Unit)
                        .FirstOrDefault(m => m.Id == parentProjectVersionedEntity_Id);                    
                case nameof(BaseActuator):
                    return secondaryDbContext.BaseActuators
                        .Include(m => m.Project)
                        .ThenInclude(p => p.Unit)
                        .FirstOrDefault(m => m.Id == parentProjectVersionedEntity_Id);                    
                case nameof(SafetyController):
                    return secondaryDbContext.SafetyControllers
                        .Include(m => m.Project)
                        .ThenInclude(p => p.Unit)
                        .FirstOrDefault(m => m.Id == parentProjectVersionedEntity_Id);                    
                case nameof(Legend):
                    return secondaryDbContext.Legends
                        .Include(m => m.Project)
                        .ThenInclude(p => p.Unit)
                        .FirstOrDefault(m => m.Id == parentProjectVersionedEntity_Id);                    
                default:
                    throw new InvalidOperationException();
            }
        }

        #endregion

        #region private fields        

        private readonly HashSet<string> _updateValue_MetaParamNames = new();

        private readonly HashSet<Project_ChangedMessage> _project_ChangedMessages = new(Project_ChangedMessage_EqualityComparer.Instance);

        #endregion
    }    
}


//public class JsonApiPazCheckDbContext : PazCheckDbContext
//{
//    #region construction and destruction       

//    /// <summary>
//    ///     Nullable params for design-time tools.
//    /// </summary>
//    /// <param name="options"></param>
//    /// <param name="configuration"></param>
//    /// <param name="callback"></param>
//    /// <param name="httpContextAccessor"></param>
//    /// <param name="dbContextFactory"></param>
//    /// <param name="informationSecurityEventsLogger"></param>
//    /// <param name="dbCache"></param>
//    public JsonApiPazCheckDbContext(
//        DbContextOptions<JsonApiPazCheckDbContext>? options = null,
//        IConfiguration? configuration = null,
//        ICallback? callback = null,
//        IHttpContextAccessor? httpContextAccessor = null,
//        IDbContextFactory<PazCheckDbContext>? dbContextFactory = null,
//        IInformationSecurityEventsLogger? informationSecurityEventsLogger = null,
//        DbCache? dbCache = null
//        ) :
//            base(
//                options,
//                configuration,
//                callback,
//                httpContextAccessor,
//                dbContextFactory,
//                informationSecurityEventsLogger,
//                dbCache)
//    {
//    }

//    #endregion

//    #region protected functions

//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        base.OnModelCreating(modelBuilder);

//        if (DbCache is null || !DbCache.CheckAccess(HttpContextHelper.GetRoles(HttpContextAccessor?.HttpContext), @"Entity." + nameof(AddonStatus) + ".Read"))
//        {
//            modelBuilder.Entity<PcObject>().HasQueryFilter(po =>
//                po.BasePcObject.Identifier != PazCheckCentralServerConstants.BasePcObject_SystemArea &&
//                po.BasePcObject.Identifier != PazCheckCentralServerConstants.BasePcObject_SystemItem);
//        }
//    }

//    #endregion
//}


//if (isDeleted)
//{
//    var childrenForIsDeleted = lastChangeEntity.GetChildrenForIsDeleted();
//    if (childrenForIsDeleted is not null)
//        foreach (var childForIsDeleted in childrenForIsDeleted)
//        {
//            childForIsDeleted._IsDeleted == true;
//            childForIsDeleted._HasUnversionedChanges = true;
//            childForIsDeleted._LastChangeUser = lastChangeEntity._LastChangeUser;
//            childForIsDeleted._LastChangeTimeUtc = lastChangeEntity._LastChangeTimeUtc;
//        }
//}


//[DefaultEntity_RoleBusinessFunctions(
//            // Read
//            new string[] { nameof(DefaultRoleBusinessFunctions.View) },
//            // Create
//            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
//                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
//            // Update
//            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
//                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) },
//            // Delete
//            new string[] { nameof(DefaultRoleBusinessFunctions.Administrating),
//                           nameof(DefaultRoleBusinessFunctions.Projects_Edit) }
//        )]
//public DbSet<TagConditionTypeInfo> TagConditionTypeInfos { get; set; } = null!;