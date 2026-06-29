using ClosedXML.Excel;
using Microsoft.Extensions.Configuration;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using Ssz.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.Helpers
{
    public static class EngUnitsHelper
    {
        public static string? GetParamValue_ToDisplay(string? value, string paramName, DbCache dbCache)
        {
            if (String.IsNullOrEmpty(value))
                return value;

            //int i = paramName.IndexOf('[');
            //if (i >= 0)
            //    paramName = paramName.Substring(0, i).Trim();

            ParamDesc? paramDesc;
            if (!dbCache.ParamDescs.TryGetValue(paramName, out paramDesc))
                return value;
            
            //paramName = PazCheckDbHelper.RemoveParamNamePrefixIfAny(paramName);
            //if (!dbCache.ParamDescs.GetValueOrDefault(paramName, out paramDesc))
            //    return value;            

            switch (paramDesc.DataType)
            {                
                case PazCheckConstants.DataType_Enum:
                    return paramDesc.MetadataFieldsDictionary.TryGetValue(value) ?? value;
                case PazCheckConstants.DataType_Single:
                    return new Any(new Any(value).ValueAsSingle(false)).ValueAsString(true, paramDesc.MetadataFieldsDictionary.TryGetValue(PazCheckConstants.MetadataParamName_Format));
                case PazCheckConstants.DataType_Int32:
                    return new Any(new Any(value).ValueAsInt32(false)).ValueAsString(true, paramDesc.MetadataFieldsDictionary.TryGetValue(PazCheckConstants.MetadataParamName_Format));                
                case PazCheckConstants.DataType_Boolean:
                    return paramDesc.MetadataFieldsDictionary.TryGetValue(value) ?? value;
                case PazCheckConstants.DataType_TimeSpan:
                    return new Any(new Any(value).ValueAs<TimeSpan>(false)).ValueAsString(true, paramDesc.MetadataFieldsDictionary.TryGetValue(PazCheckConstants.MetadataParamName_Format));
                case PazCheckConstants.DataType_DateTime:
                    return new Any(new Any(value).ValueAs<DateTimeOffset>(false)).ValueAsString(true, paramDesc.MetadataFieldsDictionary.TryGetValue(PazCheckConstants.MetadataParamName_Format));
                default:
                    return value;
            }
        }

        public static void SetCellValue_ToDisplay(IXLCell cell, string? paramValue, string paramName, DbCache dbCache)
        {
            ParamDesc? paramDesc;
            if (!dbCache.ParamDescs.TryGetValue(paramName, out paramDesc))
            {
                cell.Value = paramValue;
                return;
            }

            if (paramValue is null)
                paramValue = @"";

            switch (paramDesc.DataType)
            {
                case PazCheckConstants.DataType_Enum:
                    cell.Value = paramDesc.MetadataFieldsDictionary.TryGetValue(paramValue) ?? paramValue;
                    break;
                case PazCheckConstants.DataType_Single:
                    cell.Value = new Any(paramValue).ValueAsSingle(false);
                    break;
                case PazCheckConstants.DataType_Int32:
                    cell.Value = new Any(paramValue).ValueAsInt32(false);
                    break;
                case PazCheckConstants.DataType_Boolean:
                    cell.Value = new Any(paramValue).ValueAsBoolean(false);
                    break;
                case PazCheckConstants.DataType_TimeSpan:
                    cell.Value = new Any(paramValue).ValueAs<TimeSpan>(false);
                    break;
                case PazCheckConstants.DataType_DateTime:
                    cell.Value = new Any(paramValue).ValueAs<DateTimeOffset>(false).LocalDateTime;
                    break;
                default:
                    cell.Value = paramValue;
                    break;
            }
        }

        public static string GetStringLocalTimeWithMilliseconds(DateTime? dateTimeUtc, CultureInfo cultureInfo)
        {
            if (dateTimeUtc is null)
                return @"";

            // Переводим в локальное время
            DateTime localDateTime = dateTimeUtc.Value.ToLocalTime();

            // Получаем смещение (offset) от UTC в текущей временной зоне
            TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(localDateTime);

            // Оборачиваем во DateTimeOffset, чтобы использовать встроенное форматирование со сдвигом
            var localWithOffset = new DateTimeOffset(localDateTime, offset);

            // Форматируем строку согласно текущей культуре
            return localWithOffset.ToString("G", cultureInfo) // G = стандартная дата и время
                               + localWithOffset.ToString(".fff zzz"); // добавляем миллисекунды и offset
        }
    }
}


//// remove
//public static string GetTimeSpanDesc(TimeSpan timeSpan)
//{
//    var cultureInfo = CultureInfo.CurrentUICulture;
//    if (cultureInfo.TwoLetterISOLanguageName == @"ru")
//    {
//        var totalMilliseconds = timeSpan.TotalMilliseconds;
//        if (totalMilliseconds < 0)
//            return @"0 сек";
//        if (totalMilliseconds < 60 * 1000)
//            return $"{timeSpan:s','f} сек";
//        return $"{(int)timeSpan.TotalMinutes} мин {timeSpan:s','f} сек";
//    }
//    else
//    {
//        var totalMilliseconds = timeSpan.TotalMilliseconds;
//        if (totalMilliseconds < 0)
//            return @"0 sec.";
//        if (totalMilliseconds < 60 * 1000)
//            return $"{timeSpan:s'.'f} sec.";
//        return $"{(int)timeSpan.TotalMinutes} min. {timeSpan:s'.'f} sec.";
//    }
//}

//// remove
//public static string? Normalize(string? timeSpanString)
//{
//    if (String.IsNullOrEmpty(timeSpanString))
//        return timeSpanString;

//    var cultureInfo = CultureInfo.CurrentUICulture;
//    if (cultureInfo.TwoLetterISOLanguageName == @"ru")
//    {
//        timeSpanString = timeSpanString.Replace(@"мин", @"m");
//        timeSpanString = timeSpanString.Replace(@"сек", @"s");
//        timeSpanString = timeSpanString.Replace(',', '.');
//    }
//    else
//    {
//        timeSpanString = timeSpanString.Replace(@"min.", @"m");
//        timeSpanString = timeSpanString.Replace(@"sec.", @"s");
//    }

//    return timeSpanString;
//}

//if (totalMilliseconds < 60 * 1000)
//    return timeSpan.ToString(@"s','f 'сек.'");
//if (totalMilliseconds < 60 * 60 * 1000)
//    return timeSpan.ToString("'0,'f 'сек.'");


//public static TimeSpan GetActuatorTimeSpan(string timeSpanToDisplay)
//{
//    var cultureInfo = CultureInfo.CurrentUICulture;
//    if (cultureInfo.TwoLetterISOLanguageName == @"ru")
//    {
//        var totalMilliseconds = timeSpan.TotalMilliseconds;
//        if (totalMilliseconds < 0)
//            return @"0 сек";
//        if (totalMilliseconds < 60 * 1000)
//            return $"{timeSpan:s','f} сек";
//        return $"{(int)timeSpan.TotalMinutes} мин {timeSpan:s','f} сек";
//    }
//    else
//    {
//        var totalMilliseconds = timeSpan.TotalMilliseconds;
//        if (totalMilliseconds < 0)
//            return @"0 sec.";
//        if (totalMilliseconds < 60 * 1000)
//            return $"{timeSpan:s'.'f} sec.";
//        return $"{(int)timeSpan.TotalMinutes} min. {timeSpan:s'.'f} sec.";
//    }
//}