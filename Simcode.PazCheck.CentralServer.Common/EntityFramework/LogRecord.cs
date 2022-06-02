using System;
using System.Globalization;
using CsvHelper.Configuration.Attributes;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public class LogRecord
    {
        private DateTime _eventTime;

        [Index(1)]
        public string EventTime {
            get => _eventTime.ToString(CultureInfo.CreateSpecificCulture("ru-RU"));
            set
            {
                var ci = CultureInfo.CreateSpecificCulture("ru-RU");
                var dtfi = ci.DateTimeFormat;
                dtfi.AbbreviatedMonthNames = new[] {"Янв", "Фев", "Мар",
                    "Апр", "Май", "Июн", "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек", ""};
                dtfi.AbbreviatedMonthGenitiveNames = dtfi.AbbreviatedMonthNames;
                _eventTime = DateTime.ParseExact(value, "dd MMM yyyy HH:mm:ss.fff", ci);
            }
        }
        [Index(2)]
        public string Tag { get; set; } = @"";
        [Index(3)]
        public string Identifier { get; set; } = @"";
        [Index(9)]
        public string EventType { get; set; } = @"";
        [Index(22)]
        public string Value { get; set; } = @"";
        public DateTime RawEventTime => _eventTime;
    }
}
