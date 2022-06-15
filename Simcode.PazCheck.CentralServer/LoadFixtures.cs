using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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

        public static async Task Fixtures(IServiceProvider serviceProvider, IConfiguration configuration, AddonsManager addonsManager, bool loadFromDumpFile)
        {
            try
            {
                using var dbContext = new PazCheckDbContext();
                if (dbContext.Units.Any())
                    return;
            }
            catch
            {
            }

            if (loadFromDumpFile)
            {
                var startInfo = new ProcessStartInfo();                
                // All environment variables of the created process are inherited from the
                // current process
                startInfo.EnvironmentVariables["PGPASSWORD"] = @"postgres";
                // Required for EnvironmentVariables to be set
                startInfo.UseShellExecute = false;     
                // The executable will be search in directories that are specified
                // in the PATH variable of the current process
                startInfo.FileName = @"psql.exe";

                startInfo.Arguments = @"-U postgres -w -c ""DROP DATABASE pazcheck""";
                // Starts process
                var process = Process.Start(startInfo);
                process!.WaitForExit();

                startInfo.Arguments = @"-U postgres -w -c ""CREATE DATABASE pazcheck""";
                // Starts process
                process = Process.Start(startInfo);
                process!.WaitForExit();
                
                startInfo.Arguments = @"-U postgres -w -d pazcheck -f PazCheck.sql";
                // Starts process
                process = Process.Start(startInfo);
                process!.WaitForExit();

                return;
            }

            using (var dbContext = new PazCheckDbContext())
            {
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();

                // Users and offices
                var officeZavod = new Office
                {
                    Title = "Завод",
                };
                dbContext.Offices.Add(officeZavod);
                var office = new Office
                {
                    Title = "Отдел АСУ ТП",
                };
                dbContext.Offices.Add(office);
                dbContext.SaveChanges();
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
                dbContext.SimUsers.Add(userMngr);
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
                dbContext.SaveChanges();

                Simuser = userMngr;

                // Units
                var avtUnit = new Unit { Title = "АВТ-6", Desc = "Установка АВТ-6" };
                dbContext.Units.Add(avtUnit);
                var someUnit = new Unit { Title = "Гидроочистка", Desc = "Гидроочистка дизельных топлив" };
                dbContext.Units.Add(someUnit);
                dbContext.SaveChanges();

                // Projects
                var mainProject = new Project { Guid = Guid.NewGuid().ToString(), Title = "Основной", Desc = "Установка АВТ-6", Unit = avtUnit };
                var p2Project = new Project { Guid = Guid.NewGuid().ToString(), Title = "Второй", Desc = "Гидроочистка дизельных топлив", Unit = avtUnit };
                var p3Project = new Project { Guid = Guid.NewGuid().ToString(), Title = "Третий", Desc = "Производство элементарной серы", Unit = avtUnit };
                dbContext.Projects.Add(mainProject);
                dbContext.Projects.Add(p2Project);
                dbContext.Projects.Add(p3Project);
                dbContext.SaveChanges();

                avtUnit.ActiveProject = mainProject;
                dbContext.SaveChanges();

                // Base Actuators
                var baseActuator = new BaseActuator { Title = "Отсечной клапан", Project = mainProject };
                var param = new BaseActuatorParam { ParamName = "MaxSafeSpeed", Value = "5", _CreateTimeUtc = DateTime.UtcNow };
                baseActuator.BaseActuatorParams.Add(param);
                param = new BaseActuatorParam { ParamName = "SafeSpeed", Value = "2" };
                baseActuator.BaseActuatorParams.Add(param);
                dbContext.BaseActuators.Add(baseActuator);
                //var actuator = new Actuator { Title = "Отсечной клапан", BaseActuator = baseActuator, Project = mainProject };
                //var param = new ActuatorParam { Name = "MaxSafeSpeed", Value = "5" };
                //baseActuator.ActuatorParams.Add(param);
                //param = new ActuatorParam { Name = "SafeSpeed", Value = "2" };
                //baseActuator.ActuatorParams.Add(param);
                //context.Actuators.Add(baseActuator);
                dbContext.SaveChanges();

                var examplesDirectoryFullName = ServerConfigurationHelper.GetExamplesDirectoryFullName(configuration);

                // TAGs                
                var tagsImporterAddon = addonsManager.GetInitializedAddons<TagsImporterAddonBase>(null).FirstOrDefault();
                if (tagsImporterAddon is not null)
                {
                    foreach (string fileFullName in Directory.EnumerateFiles(examplesDirectoryFullName))
                    {
                        string fileName = Path.GetFileName(fileFullName);
                        if (fileName.EndsWith("tags.csv", StringComparison.InvariantCultureIgnoreCase))
                        {
                            try
                            {
                                using (var stream = File.OpenRead(fileFullName))
                                {
                                    tagsImporterAddon.ImportTags(stream, dbContext, mainProject, Simuser!.User);
                                }
                            }
                            catch
                            {
                            }
                        }
                    }
                }

                // Logs                
                var logsImporterAddon = addonsManager.GetInitializedAddons<LogsImporterAddonBase>(null).FirstOrDefault();
                if (logsImporterAddon is not null)
                {
                    foreach (string fileFullName in Directory.EnumerateFiles(examplesDirectoryFullName))
                    {
                        string fileName = Path.GetFileName(fileFullName);
                        if (fileName.EndsWith("logs.csv", StringComparison.InvariantCultureIgnoreCase))
                        {
                            try
                            {
                                using var stream = System.IO.File.OpenRead(fileFullName);
                                await logsImporterAddon.ImportLogsAsync(stream, fileName, dbContext, avtUnit.Id, CancellationToken.None, DummyJobProgress.Dafault);
                            }
                            catch
                            {
                            }
                        }
                    }
                }

                // CE Matrices, CE Matrices Runtimes
                var ceMatrixRuntimeAddon = addonsManager.GetInitializedAddons<CeMatrixRuntimeAddonBase>(null).FirstOrDefault();
                if (ceMatrixRuntimeAddon is not null)
                {
                    await ceMatrixRuntimeAddon.LoadFixturesAsync(configuration, serviceProvider, dbContext, mainProject);
                }
            }                
        }

        #endregion

        #region private functions

        private static Simuser? Simuser { get; set; }

        #endregion        
    }

    public static class SimRole
    {
        public const string RoleAdmin = "RoleAdmin";
        public const string RoleEng = "RoleEng";
        public const string RoleUser = "RoleUser";
    }
}


//private static void LoadDiagram(PazCheckDbContext context, Project project, string name)
//{
//    var diagram01 = new CeMatrix { Title = name + " " + project.Title, Project = project, ProjectId = project.Id };
//    context.Diagrams.Add(diagram01);
//    context.SaveChanges();
//}

//private static Tag LoadTag(PazCheckDbContext context, Project project, string name)
//{
//    var tag = new Tag() { TagName = name, Project = project };
//    project.Tags.Add(tag);
//    var ident = new TagCondition
//    {
//        ElementName = "Alarm",
//        Type = "ALARM",
//        Value = "CLOSE"
//    };
//    tag.TagConditions.Add(ident);
//    context.SaveChanges();
//    return tag;
//}        


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