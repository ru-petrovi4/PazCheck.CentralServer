using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;

namespace Simcode.PazCheck.Addons.CentralServer.DummyExperionTagsImporter
{
    public class QdbTagsLoader
    {
        private readonly PazCheckDbContext _context;
        private readonly ILogger _logger;

        public QdbTagsLoader(PazCheckDbContext context, ILogger<QdbTagsLoader> logger)
        {
            _context = context;
            _logger = logger;
        }

        public TagImportInfo LoadTags(StreamReader reader, Project project, string user)
        {
            var st = new Stopwatch();
            st.Start();
            var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                IgnoreBlankLines = false
            };
            using var csv = new CsvReader(reader, cfg);
            var isHeader = true;
            var ret = new TagImportInfo();
            int count = 0;
            while (csv.Read())
            {
                if (isHeader)
                {
                    csv.ReadHeader();
                    isHeader = false;
                    continue;
                }

                if (string.IsNullOrEmpty(csv.GetField(0)))
                {
                    isHeader = true;
                    continue;
                }

                if (Enum.TryParse(csv.GetField(csv.GetFieldIndex("Class")), out TagType tagType))
                {
                    var tag = new Tag() { _CreateTimeUtc = DateTime.UtcNow, _CreateUser = user, Project = project };
                    switch (tagType)
                    {
                        case TagType.StatusPoint:
                            {
                                ret.TotalTags++;
                                var tmpTag = csv.GetRecord<QdbStatusTag>();
                                tag.TagName = tmpTag.Name;
                                tag.Desc = tmpTag.Descr;
                                int numIdentities = new Any(csv.GetField(csv.GetFieldIndex(TagFields.StatusNumStates.ToString()))).ValueAsInt32(false);
                                for (var i = numIdentities - 1; i >= 0; i--)
                                {
                                    bool isAlarm = new Any(csv.GetField(
                                            csv.GetFieldIndex(TagFields.AlarmEnableState.ToString() + i.ToString()))).ValueAsBoolean(false);                                    
                                    if (isAlarm)
                                    {
                                        var tmpValue = csv.GetField(
                                            csv.GetFieldIndex(TagFields.DescriptorState.ToString() + i.ToString()));
                                        var tagCondition = new TagCondition()
                                        {
                                            _CreateTimeUtc = DateTime.UtcNow,
                                            ElementName = "ALARM", //ToDo: Configuration! Move to config varibles
                                            Type = "Alarm", //ToDo: Configuration! Move to config varibles
                                            Value = tmpValue
                                        };
                                        tagCondition.Tag = tag;
                                        tag.TagConditions.Add(tagCondition);
                                    }
                                }
                                if (tag.TagConditions.Count > 0)
                                {
                                    ret.ImportedTags++;
                                    _context.Tags.Add(tag);
                                    count += 1;
                                }
                                break;
                            }
                        case TagType.AnalogPoint:
                            {
                                ret.TotalTags++;
                                var tmpTag = csv.GetRecord<QdbAnalogPoint>();
                                tag.TagName = tmpTag.Name;
                                tag.Desc = tmpTag.Descr;
                                for (var i = 1; i < 9; i++)
                                {
                                    var tmpAlarm = csv.GetField(
                                        csv.GetFieldIndex(TagFields.AlarmType.ToString() + i.ToString()));
                                    //if (!tmpAlarm.Equals("None") && tmpAlarm.Length > 0) //ToDo: Configuration! This must be configuration value
                                    //{
                                    //    Enum.TryParse(tmpAlarm, out AlarmTypes alrmType);
                                    //    var alarmValues = new[] { "PVLOLO", "PVLO", "PVHI", "PVHIHI" };
                                    //    var tmpIdentity = new Identity()
                                    //    {
                                    //        Identifier =
                                    //            alarmValues[(int)alrmType], //ToDo: Configuration! Move to config varibles
                                    //        EventType = "Alarm", //ToDo: Configuration! Move to config varibles
                                    //        Value = ""
                                    //    };
                                    //    tmpIdentity.Tag = tmpProjectTag;
                                    //    tmpProjectTag.Identities.Add(tmpIdentity);
                                    //}
                                    if (!tmpAlarm.Equals("None") && tmpAlarm.Length > 0)
                                    {
                                        var tagCondition = new TagCondition()
                                        {
                                            _CreateTimeUtc = DateTime.UtcNow,
                                            ElementName = tmpAlarm,
                                            Type = "Alarm", //ToDo: Configuration! Move to config varibles
                                            Value = ""
                                        };
                                        tagCondition.Tag = tag;
                                        tag.TagConditions.Add(tagCondition);
                                    }
                                }
                                if (tag.TagConditions.Count > 0)
                                {
                                    ret.ImportedTags++;
                                    _context.Tags.Add(tag);
                                    count += 1;
                                }
                                break;
                            }
                    }
                }
            }
            
            _context.SaveChanges();
            st.Stop();
            _logger.LogInformation($"LoadTags: Count={count}; Millseconds={st.ElapsedMilliseconds}");            
            return ret;
        }

        public async Task ImportTags(Project project, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Staring to load tags for project {project.Title}");
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            _logger.LogInformation($"Finished to load tags for project {project.Title}");
        }
    }

    public class TagImportInfo
    {
        public int TotalTags { get; set; }
        public int ImportedTags { get; set; }
    }
}
