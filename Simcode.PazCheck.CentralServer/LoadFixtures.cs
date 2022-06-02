using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.Common;
using Ssz.Utils;

namespace Simcode.PazCheck.CentralServer
{
    public partial class LoadFixtures
    {
        #region public functions

        public static void Fixtures(IServiceProvider serviceProvider, IConfiguration configuration, AddonsManager addonsManager)
        {
            var serviceScope = serviceProvider.CreateScope();
            var context = serviceScope.ServiceProvider.GetRequiredService<PazCheckDbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            LoadUsers(context);
            LoadUnits(serviceProvider, context, configuration, addonsManager);
        }

        #endregion

        #region private functions

        private static Simuser? Simuser { get; set; }

        private static void LoadUsers(PazCheckDbContext context)
        {
            if (!context.SimUsers.Any())
            {
                var officeZavod = new Office
                {
                    Name = "Завод",
                };
                context.Offices.Add(officeZavod);
                var office = new Office
                {
                    Name = "Отдел АСУ ТП",
                };
                context.Offices.Add(office);
                context.SaveChanges();
                var userMngr = new Simuser
                {
                    Username = "mngr",
                    Password = "mngr1",
                    FirstName = "Александр",
                    MiddleName = "Сергеевич",
                    LastName = "Пушкин",
                    PersonnelNumber = "27",
                    Office = officeZavod,
                    Role = SimRole.RoleAdmin
                };
                context.SimUsers.Add(userMngr);
                //var userEng = new Simuser
                //{
                //    Username = "eng",
                //    Password = "eng1",
                //    FirstName = "Иван",
                //    MiddleName = "Иванович",
                //    LastName = "Иванов",
                //    PersonnelNumber = "51",
                //    Office = office,
                //    Role = SimRole.RoleEng
                //};
                //context.SimUsers.Add(userEng);
                //var userOrd = new Simuser
                //{
                //    Username = "user",
                //    Password = "user1",
                //    FirstName = "Петр",
                //    MiddleName = "Петрович",
                //    LastName = "Петров",
                //    PersonnelNumber = "62",
                //    Office = office,
                //    Role = SimRole.RoleUser
                //};
                //context.SimUsers.Add(userOrd);
                context.SaveChanges();
                Simuser = userMngr;
            }
        }

        private static void LoadUnits(IServiceProvider serviceProvider, PazCheckDbContext context, IConfiguration configuration, AddonsManager addonsManager)
        {
            if (!context.Units.Any())
            {
                var avtUnit = new Unit { Name = "АВТ-6", Descr = "Установка АВТ-6" };
                context.Units.Add(avtUnit);
                var someUnit = new Unit { Name = "Гидроочистка", Descr = "Гидроочистка дизельных топлив" };
                context.Units.Add(someUnit);
                context.SaveChanges();
                var mlspProject = new Project { Name = "Основной", Descr = "Установка АВТ-6", Unit = avtUnit, ByUser = Simuser };
                var uralProject = new Project { Name = "Второй", Descr = "Гидроочистка дизельных топлив", Unit = avtUnit, ByUser = Simuser };
                var saratovProject = new Project { Name = "Третий", Descr = "Производство элементарной серы", Unit = avtUnit, ByUser = Simuser };
                context.Projects.Add(mlspProject);
                context.Projects.Add(uralProject);
                context.Projects.Add(saratovProject);
                context.SaveChanges();
                var act = new Actuator { Name = "Отсечной клапан", Type = ActuatorTypes.Valve.ToString(), Project = mlspProject };
                var param = new Actuatorparams { Name = "MaxSafeSpeed", Value = "5" };
                act.Actuatorparams.Add(param);
                param = new Actuatorparams { Name = "SafeSpeed", Value = "2" };
                act.Actuatorparams.Add(param);
                context.Actuators.Add(act);
                context.SaveChanges();
                avtUnit.ActiveProject = mlspProject;
                avtUnit.ByUser = Simuser;
                avtUnit.LoadedDate = DateTime.UtcNow;
                context.SaveChanges();
                ImportQdbTags(configuration, serviceProvider, context, mlspProject);

                var ceMatrixRuntimeAddon = addonsManager.GetInitializedAddons<CeMatrixRuntimeAddonBase>(null).FirstOrDefault();
                if (ceMatrixRuntimeAddon is not null)
                {
                    ceMatrixRuntimeAddon.LoadFixtures(configuration, serviceProvider, context, mlspProject);
                }

                //var directoryInfo = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "Resource"));
                //var dataServerSection = configuration.GetSection(@"DataServer");
                //if (dataServerSection == null) return;
                //var emseventsDbConnectionString = dataServerSection[@"EmseventsDbConnectionString"];     

                //LogTimeInterval? logTimeInterval = ExperionHelper.GetLogFromExprionDb(NullLogger.Instance, emseventsDbConnectionString,
                //    TimeSpan.FromDays(365),
                //    Path.Combine(directoryInfo.FullName, @"ExperionAllLog_Raw.csv")); // SRVEPKS01B
                //if (logTimeInterval != null)
                //    LogHelper.SaveLogToCsv(logTimeInterval,
                //        Path.Combine(directoryInfo.FullName, @"ExperionAllLog.csv"));

                // var tmpTag = LoadTag(context, mlspProject, "FI001");
                // var tmpCause = new Cause { Name = "FI"};
                // context.SaveChanges();
                // tmpCause.Identities.Add(tmpTag.Identities[0]);
                // context.SaveChanges();
                //ConstructDiagram(context, mlspProject);
                //ConstructDiagram(context, uralProject);
                //ConstructComplexCause(context, mlspProject);
                //ConstructComplexCause(context, uralProject);
                //var mlspLog = new Log {Name = "First log", Project = mlspProject};
                //context.Logs.Add(mlspLog);
                //context.SaveChanges();
                //ImportLog(context, mlspLog);
                //var settings = new Settings
                //{
                //    DeltaTime = TimeSpan.FromMinutes(1)
                //};
            }
        }

        private static void LoadDiagram(PazCheckDbContext context, Project project, string name)
        {
            var diagram01 = new Diagram { Name = name + " " + project.Name, Project = project, ProjectId = project.Id };
            context.Diagrams.Add(diagram01);
            context.SaveChanges();
        }

        private static Tag LoadTag(PazCheckDbContext context, Project project, string name)
        {
            var tag = new Tag() { Name = name, Project = project, ProjectId = project.Id };
            project.Tags.Add(tag);
            var ident = new Identity
            {
                Identifier = "Alarm",
                EventType = "ALARM",
                Value = "CLOSE"
            };
            tag.Identities.Add(ident);
            context.SaveChanges();
            return tag;
        }

        private static void ImportQdbTags(IConfiguration configuration, IServiceProvider serviceProvider, PazCheckDbContext context, Project project)
        {
            var addonsManager = serviceProvider.GetRequiredService<AddonsManager>();

            var tagsImporterAddon = addonsManager.GetInitializedAddons<TagsImporterAddonBase>(null, a => String.Equals(a.Name, "ExperionTagsImporter", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (tagsImporterAddon is not null)
            {
                using (var stream = File.OpenRead(Path.Combine(ServerConfigurationHelper.GetExamplesDirectoryFullName(configuration), @"ExampleExperionTags.csv")))
                {
                    tagsImporterAddon.ImportTags(stream, context, project);
                }

                using (var stream = File.OpenRead(Path.Combine(ServerConfigurationHelper.GetExamplesDirectoryFullName(configuration), @"ExampleDeltaSimTags.csv")))
                {
                    tagsImporterAddon.ImportTags(stream, context, project);
                }
            }
        }

        #endregion        
    }

    public static class SimRole
    {
        public const string RoleAdmin = "RoleAdmin";
        public const string RoleEng = "RoleEng";
        public const string RoleUser = "RoleUser";
    }
}
