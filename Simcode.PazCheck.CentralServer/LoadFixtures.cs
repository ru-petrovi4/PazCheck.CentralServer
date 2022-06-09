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
                    User = "mngr",
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
                var avtUnit = new Unit { Title = "АВТ-6", Desc = "Установка АВТ-6" };
                context.Units.Add(avtUnit);
                var someUnit = new Unit { Title = "Гидроочистка", Desc = "Гидроочистка дизельных топлив" };
                context.Units.Add(someUnit);
                context.SaveChanges();

                var mainProject = new Project { Title = "Основной", Desc = "Установка АВТ-6", Unit = avtUnit, _CreateUser = Simuser!.User, _CreateTimeUtc = DateTime.UtcNow };
                var p2Project = new Project { Title = "Второй", Desc = "Гидроочистка дизельных топлив", Unit = avtUnit, _CreateUser = Simuser!.User, _CreateTimeUtc = DateTime.UtcNow };
                var p3Project = new Project { Title = "Третий", Desc = "Производство элементарной серы", Unit = avtUnit, _CreateUser = Simuser!.User, _CreateTimeUtc = DateTime.UtcNow };
                context.Projects.Add(mainProject);
                context.Projects.Add(p2Project);
                context.Projects.Add(p3Project);
                context.SaveChanges();

                var baseActuator = new BaseActuator { Title = "Отсечной клапан", Project = mainProject };
                var param = new BaseActuatorParam { ParamName = "MaxSafeSpeed", Value = "5" };
                baseActuator.BaseActuatorParams.Add(param);
                param = new BaseActuatorParam { ParamName = "SafeSpeed", Value = "2" };
                baseActuator.BaseActuatorParams.Add(param);
                context.BaseActuators.Add(baseActuator);

                //var actuator = new Actuator { Title = "Отсечной клапан", BaseActuator = baseActuator, Project = mainProject };
                //var param = new ActuatorParam { Name = "MaxSafeSpeed", Value = "5" };
                //baseActuator.ActuatorParams.Add(param);
                //param = new ActuatorParam { Name = "SafeSpeed", Value = "2" };
                //baseActuator.ActuatorParams.Add(param);
                //context.Actuators.Add(baseActuator);
                context.SaveChanges();

                avtUnit.ActiveProject = mainProject;               
                
                context.SaveChanges();

                ImportQdbTags(configuration, serviceProvider, context, mainProject);

                var ceMatrixRuntimeAddon = addonsManager.GetInitializedAddons<CeMatrixRuntimeAddonBase>(null).FirstOrDefault();
                if (ceMatrixRuntimeAddon is not null)
                {
                    ceMatrixRuntimeAddon.LoadFixtures(configuration, serviceProvider, context, mainProject);
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
            var diagram01 = new CeMatrix { Title = name + " " + project.Title, Project = project, ProjectId = project.Id };
            context.Diagrams.Add(diagram01);
            context.SaveChanges();
        }

        private static Tag LoadTag(PazCheckDbContext context, Project project, string name)
        {
            var tag = new Tag() { TagName = name, Project = project };
            project.Tags.Add(tag);
            var ident = new TagCondition
            {
                ElementName = "Alarm",
                Type = "ALARM",
                Value = "CLOSE"
            };
            tag.TagConditions.Add(ident);
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
                    tagsImporterAddon.ImportTags(stream, context, project, Simuser!.User);
                }

                using (var stream = File.OpenRead(Path.Combine(ServerConfigurationHelper.GetExamplesDirectoryFullName(configuration), @"ExampleDeltaSimTags.csv")))
                {
                    tagsImporterAddon.ImportTags(stream, context, project, Simuser!.User);
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
