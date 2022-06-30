using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Ssz.Utils;
using Simcode.PazCheck.CentralServer.Common;

namespace Simcode.PazCheck.Addons.DummyExperionEventsImporter
{
    public class DummyExperionLogLoader
    {        
        private readonly ILogger _logger;

        public DummyExperionLogLoader(ILogger<DummyExperionLogLoader> logger)
        {            
            _logger = logger;
        }        

        public async Task ImportFileLogAsync(CsvDb csvDb, Stream stream, string logName, PazCheckDbContext context, int unitId, CancellationToken cancellationToken, IJobProgress jobProgress)
        {            
            var logevents = new List<UnitEvent>();
            var unit = await context.Units
                .FirstAsync(u => u.Id == unitId, cancellationToken: cancellationToken);

            var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = header => header.Header.ToLower()
            };

            using var streamReader = new StreamReader(stream);
            using var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture, true);
            //int count = (int)(new FileInfo(fileFullName).Length / 200); // Results from avarage file.
            int i = 0;
            await foreach (var logRecord in csv.GetRecordsAsync<DummyExperionLogRecord>(cancellationToken))
            {
                i++;
                if (i % 100 == 0) 
                    await jobProgress.ReportAsync(100 * streamReader.BaseStream.Position / streamReader.BaseStream.Length, null, null, false);

                bool alarmConditionIsActive;
                switch (logRecord.EventType.ToUpperInvariant())
                {
                    case "ALARM":
                        alarmConditionIsActive = true;
                        break;
                    case "RETURNTONORMAL":
                        alarmConditionIsActive = false;
                        break;
                    default:
                        continue;
                }

                string alarmCondition = logRecord.Identifier;
                switch (logRecord.Identifier.ToUpperInvariant())
                {
                    case "NONE":
                        continue; // ignore when no alarm.
                    case "ALARM":
                        if (!alarmConditionIsActive) continue; // ALARM can not return to normal.
                        alarmCondition += "=" + logRecord.Value;
                        break;
                }

                DateTime dateTimeUtc = logRecord.RawEventTimeUtc;
                string tagAndCondition = logRecord.Tag + "." + alarmCondition;

                var logevent = new UnitEvent
                {
                    EventTimeUtc = dateTimeUtc,
                    TagName = logRecord.Tag,
                    TagConditionString = tagAndCondition,
                    ConditionIsActive = alarmConditionIsActive,
                    EventSource = "Experion",
                    OriginalEvent = @"" // TODO
                };

                logevents.Add(logevent);
            }

            if (logevents.Count > 0)
            {
                logevents = logevents.OrderBy(l => l.EventTimeUtc).ToList();
                var log = new UnitEventsInterval
                {
                    Title = logName,
                    Unit = unit,
                    StartTimeUtc = logevents.First().EventTimeUtc,
                    EndTimeUtc = logevents.Last().EventTimeUtc,
                    UnitEvents = logevents
                };
                context.UnitEventsIntervals.Add(log);

                await context.SaveChangesAsync(cancellationToken);
            }

            await jobProgress.ReportAsync(100, null, null, false);
        }

        private class DummyExperionLogRecord
        {
            private DateTime _rawEventTimeUtc;

            [CsvHelper.Configuration.Attributes.Index(1)]
            public string EventTime
            {
                get => _rawEventTimeUtc.ToString(CultureInfo.CreateSpecificCulture("ru-RU"));
                set
                {
                    //var ci = CultureInfo.CreateSpecificCulture("ru-RU");
                    var ci = CultureInfo.InvariantCulture;
                    _rawEventTimeUtc = DateTime.ParseExact(value, "dd MMM yyyy HH:mm:ss.fff", ci).ToUniversalTime();
                }
            }

            [CsvHelper.Configuration.Attributes.Index(2)]
            public string Tag { get; set; } = @"";

            [CsvHelper.Configuration.Attributes.Index(3)]
            public string Identifier { get; set; } = @"";

            [CsvHelper.Configuration.Attributes.Index(9)]
            public string EventType { get; set; } = @"";

            [CsvHelper.Configuration.Attributes.Index(22)]
            public string Value { get; set; } = @"";

            public DateTime RawEventTimeUtc => _rawEventTimeUtc;
        }
    }
}


//public int ProccessLog(StreamReader reader, Log log)
//{
//    throw new NotImplementedException();
//}

//public Task ImportLogAsync(Log log, CancellationToken cancellationToken, IProgress<int> progress)
//{
//    throw new NotImplementedException();
//}