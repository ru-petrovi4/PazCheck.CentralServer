using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.MicroServices;
using Ssz.Utils;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common
{
    public class Filter
    {
        public string? SearchBy { get; set; }        

        public string? SearchString { get; set; }       

        /// <summary>
        ///     (CriterionInfo1 AND CriterionInfo2) OR (CriterionInfo3 AND CriterionInfo4)
        /// </summary>
        public Criterion[][]? CriterionCollection { get; set; }

        public string? User { get; set; }

        public int? ParentEntityId { get; set; }        

        /// <summary>
        ///     Results sort order.
        /// </summary>
        public string? OrderBy { get; set; }

        public string? IsOrderByDescending { get; set; }

        public string? Aggregation { get; set; }

        public int MaxRecordsCount { get; set; }
    }

    public class Criterion
    {
        public string CriterionName { get; set; } = null!;        

        public string? Operator { get; set; }

        public string? Options { get; set; }

        /// <summary>
        ///    Values to compare
        /// </summary>
        public string[]? ValuesList { get; set; }        

        public ParamDesc[]? Temp_ParamDescs { get; set; }

        public SszOperator Temp_SszOperator { get; set; }

        public SszOperatorOptions Temp_SszOperatorOptions { get; set; }
    }

    public static class FilterHelper
    {
        public static List<CaseInsensitiveOrderedDictionary<List<string>>> Parse(JsonElement rootElement)
        {
            // OR list of filters
            List<CaseInsensitiveOrderedDictionary<List<string>>> parsedJsonFilterInfo = new();

            // AND dictionary of filters
            CaseInsensitiveOrderedDictionary<List<string>> partParsedJsonFilterInfo = new();            

            switch (rootElement.ValueKind)
            {
                case JsonValueKind.Object:
                    partParsedJsonFilterInfo = ParseObject(rootElement);
                    parsedJsonFilterInfo.Add(partParsedJsonFilterInfo);
                    break;
                case JsonValueKind.Array:
                    foreach (var element in rootElement.EnumerateArray())
                    {
                        if (element.ValueKind == JsonValueKind.Object)
                        {
                            partParsedJsonFilterInfo = ParseObject(element);
                            parsedJsonFilterInfo.Add(partParsedJsonFilterInfo);
                        }
                    }
                    break;
            }

            return parsedJsonFilterInfo;
        }

        public static Filter Create(List<CaseInsensitiveOrderedDictionary<List<string>>>? filterInfo, Func<string, string>? transformValue = null)
        {
            // OR list of filters
            List<Criterion[]> criterionList = new();

            if (filterInfo is not null)
                foreach (var partFilterInfo in filterInfo)
                {
                    // List of AND filters
                    List<Criterion> partCriterionList = new();
                    foreach (var kvp in partFilterInfo)
                    {
                        foreach (var valueString in kvp.Value)
                        {
                            string operatorAndOptionsAndValues;
                            if (transformValue is not null)
                                operatorAndOptionsAndValues = transformValue(valueString);
                            else
                                operatorAndOptionsAndValues = valueString;

                            var (sszOperator, sszOperatorOptions, values) = SszOperatorHelper.Parse2(operatorAndOptionsAndValues);
                            if (sszOperator == SszOperator.None)
                                sszOperator = SszOperator.Equal;
                            partCriterionList.Add(new Criterion()
                            {
                                CriterionName = kvp.Key,
                                Operator = SszOperatorHelper.ToString(sszOperator),
                                Options = ((int)sszOperatorOptions).ToString(),
                                ValuesList = values
                            });
                        }
                    }
                    criterionList.Add(partCriterionList.ToArray());
                }

            Filter filter = new()
            {                
                CriterionCollection = criterionList.ToArray()
            };

            return filter;
        }

        /// <summary>
        ///     Returns unprepared filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static Filter Clone(Filter filter)
        {
            Filter cloneFilter = new Filter()
            {
                SearchBy = filter.SearchBy,
                SearchString = filter.SearchString,
                User = filter.User,                
                ParentEntityId = filter.ParentEntityId,
                OrderBy = filter.OrderBy,
                IsOrderByDescending = filter.IsOrderByDescending
            };

            if (filter.CriterionCollection is not null)
            {
                // OR list of filters
                List<Criterion[]> cloneCriterionList = new();

                foreach (var partFilterInfo in filter.CriterionCollection)
                {
                    // List of AND filters
                    List<Criterion> clonePartCriterionList = new();
                    foreach (var criterion in partFilterInfo)
                    {
                        clonePartCriterionList.Add(new Criterion()
                        {
                            CriterionName = criterion.CriterionName,
                            Operator = criterion.Operator,
                            Options = criterion.Options,
                            ValuesList = (string[]?)criterion.ValuesList?.Clone(),                            
                        });                        
                    }
                    cloneCriterionList.Add(clonePartCriterionList.ToArray());
                }

                cloneFilter.CriterionCollection = cloneCriterionList.ToArray();
            }

            return cloneFilter;
        }

        public static void AddToFilter(Filter filter, List<CaseInsensitiveOrderedDictionary<List<string>>> filterInfo, Func<string, string>? transformValue = null)
        {
            // OR list of filters
            List<Criterion[]> criterionList = new();

            if (filterInfo is not null)
                foreach (var partFilterInfo in filterInfo)
                {
                    // List of AND filters
                    List<Criterion> partCriterionList = new();
                    foreach (var kvp in partFilterInfo)
                    {
                        foreach (var valueString in kvp.Value)
                        {
                            string operatorAndOptionsAndValues;
                            if (transformValue is not null)
                                operatorAndOptionsAndValues = transformValue(valueString);
                            else
                                operatorAndOptionsAndValues = valueString;

                            var (sszOperator, sszOperatorOptions, values) = SszOperatorHelper.Parse2(operatorAndOptionsAndValues);
                            if (sszOperator == SszOperator.None)
                                sszOperator = SszOperator.Equal;
                            partCriterionList.Add(new Criterion()
                            {
                                CriterionName = kvp.Key,
                                Operator = SszOperatorHelper.ToString(sszOperator),
                                Options = ((int)sszOperatorOptions).ToString(),
                                ValuesList = values
                            });
                        }
                    }
                    criterionList.Add(partCriterionList.ToArray());
                }            

            if (filter.CriterionCollection is null || filter.CriterionCollection.Length == 0)
                filter.CriterionCollection = criterionList.ToArray();
            else if (criterionList.Count > 0)
                filter.CriterionCollection[0] = filter.CriterionCollection[0].Concat(criterionList[0]).ToArray();
        }

        public static void Prepare(Filter filter, DbCache dbCache)
        {
            if (filter.CriterionCollection is null)
            {
                filter.CriterionCollection = Array.Empty<Criterion[]>();
                return;
            }

            foreach (var orCriterionCollection in filter.CriterionCollection)            
            {
                foreach (var criterion in orCriterionCollection)
                {
                    Prepare(criterion, dbCache);
                }
            }
        }

        public static void Prepare(Criterion criterion, DbCache dbCache)
        {
            if (String.IsNullOrEmpty(criterion?.CriterionName))
                return;

            int i = criterion.CriterionName.IndexOf('[');
            int j = criterion.CriterionName.LastIndexOf(']');
            if (i > -1 && j > i)
            {
                string[] paramNames = criterion.CriterionName.Substring(i + 1, j - i - 1).Split(',').Select(pn => pn.Trim()).ToArray();

                List<ParamDesc> paramDescs = new(paramNames.Length);

                foreach (var paramName in paramNames)
                {
                    if (String.IsNullOrEmpty(paramName))
                        continue;

                    ParamDesc? paramDesc;
                    dbCache.ParamDescs.TryGetValue(paramName, out paramDesc);

                    if (paramDesc is null)
                    {
                        paramDesc = new ParamDesc()
                        {
                            Id = paramName
                        };
                    }

                    paramDescs.Add(paramDesc);
                }

                criterion.Temp_ParamDescs = paramDescs.ToArray();
            }

            criterion.Temp_SszOperator = SszOperatorHelper.FromString(criterion.Operator, defaultSszOperator: SszOperator.Equal);

            if (!String.IsNullOrEmpty(criterion.Options))
                foreach (var option in criterion.Options.Split('|'))
                {
                    criterion.Temp_SszOperatorOptions |= new Any(option).ValueAs<SszOperatorOptions>(false);
                }
        }

        #region private fields

        /// <summary>
        ///     Precondition: element.ValueKind == JsonValueKind.Object
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static CaseInsensitiveOrderedDictionary<List<string>> ParseObject(JsonElement element)
        {
            // AND dictionary of filters
            CaseInsensitiveOrderedDictionary<List<string>> partParsedJsonFilterInfo = new();

            foreach (var jsonProperty in element.EnumerateObject())
            {
                if (!partParsedJsonFilterInfo.TryGetValue(jsonProperty.Name, out List<string>? list))
                {
                    list = new();
                    partParsedJsonFilterInfo.Add(jsonProperty.Name, list);
                }
                switch (jsonProperty.Value.ValueKind)
                {
                    case JsonValueKind.String:
                    case JsonValueKind.Number:
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        list.Add(jsonProperty.Value.ToString());
                        break;
                    case JsonValueKind.Array:
                        list.AddRange(jsonProperty.Value.EnumerateArray().Select(i => i.ToString()));
                        break;
                }
            }

            return partParsedJsonFilterInfo;
        }        

        #endregion
    }
}


//private static CaseInsensitiveOrderedDictionary<(string, SszOperator?, CaseInsensitiveOrderedDictionary<string>?)> CriterionMap = new ()
//{
//    { Common.Properties.Resources.ParamDesc_TemplateTag, ($"Params[{PazCheckCentralServerConstants.ParamName_TemplateTag}]", null, null) },
//    { Common.Properties.Resources.ParamDesc_BaseActuator, ($"Params[{PazCheckCentralServerConstants.ParamName_BaseActuator}]", null, null) },
//    { Common.Properties.Resources.ParamDesc_BasePcObject_Identifier, (PazCheckCentralServerConstants.CriterionName_BasePcObject_Identifier, null, null) },
//    { Common.Properties.Resources.ParamDesc_SafetyIndex, ($"JournalParams[{PazCheckCentralServerConstants.ParamName_SafetyIndex}]", null, new()
//    {
//        { Common.Properties.Resources.SafetyIndex_Green, @"(66..100]" },
//        { Common.Properties.Resources.SafetyIndex_Yellow, @"(33..66]" },
//        { Common.Properties.Resources.SafetyIndex_Red, @"[0..33]" },
//    })},
//    { Common.Properties.Resources.ParamDesc_MalfunctionCategory, ($"EventParams[{PazCheckCentralServerConstants.ParamName_MalfunctionCategory}]", null, null) },
//    { Common.Properties.Resources.CriterionDesc_From, (PazCheckCentralServerConstants.CriterionName_From, null, null) },
//    { Common.Properties.Resources.CriterionDesc_To, (PazCheckCentralServerConstants.CriterionName_To, null, null) },
//    { Common.Properties.Resources.CriterionDesc_EventType, (PazCheckCentralServerConstants.CriterionName_EventType, null, new()
//    {
//        { Common.Properties.Resources.PcObjectEventTypeTitle_EmergencyShutdown, PazCheckCentralServerConstants.PcObjectEventType_EmergencyShutdown },
//        { Common.Properties.Resources.PcObjectEventTypeTitle_ManualCheck, PazCheckCentralServerConstants.PcObjectEventType_ManualCheck },
//        { Common.Properties.Resources.PcObjectEventTypeTitle_ActuatorAction, PazCheckCentralServerConstants.PcObjectEventType_ActuatorAction },
//        { Common.Properties.Resources.PcObjectEventTypeTitle_DBK, PazCheckCentralServerConstants.PcObjectEventType_DBK },
//        { Common.Properties.Resources.PcObjectEventTypeTitle_ForcedVariable, PazCheckCentralServerConstants.PcObjectEventType_ForcedVariable },
//    })},
//    { Common.Properties.Resources.CriterionDesc_Event, ($"EventParams[{PazCheckCentralServerConstants.ParamName_EventTypeTitle},{PazCheckCentralServerConstants.ParamName_EventTypeDesc},{PazCheckCentralServerConstants.ParamName_EventTitle},{PazCheckCentralServerConstants.ParamName_EventDesc}]", SszOperator.Contains, null) },
//    { Common.Properties.Resources.ParamDesc_ActuationType, ($"EventParams[{PazCheckCentralServerConstants.ParamName_LogicActuationType},{PazCheckCentralServerConstants.ParamName_CommandActuationType}]", null, new()
//    {
//        { Common.Properties.Resources.ParamValue_SuccessFirstTriggered, new Any(TriggeredType.SuccessFirstTriggered).ValueAsString(false) },
//        { Common.Properties.Resources.ParamValue_SuccessTriggered, new Any(TriggeredType.SuccessTriggered).ValueAsString(false) },
//        { Common.Properties.Resources.ParamValue_LateTriggered, new Any(TriggeredType.LateTriggered).ValueAsString(false) },
//        { Common.Properties.Resources.ParamValue_NotTriggered, new Any(TriggeredType.NotTriggered).ValueAsString(false) },
//        { Common.Properties.Resources.ParamValue_FaultTriggered, new Any(TriggeredType.FaultTriggered).ValueAsString(false) },
//    }) },
//    { Common.Properties.Resources.ParamDesc_EmergencyShutdownLevel, ($"EventParams[{PazCheckCentralServerConstants.ParamName_EmergencyShutdownLevel}]", null, null) },            
//};


//public static List<CriteriaInfo> GetValues(this FilterInfo filter, string criterionName)
//{
//    if (filter.CriteriaInfosList is null)
//        return new();

//    return filter.CriteriaInfosList
//        .Where(ci => String.Equals(ci.CriteriaName, criterionName, StringComparison.InvariantCultureIgnoreCase))
//        .ToList();
//}

//public static string GetValue(this FilterInfo filter, string criterionName, string defaultValue)
//{
//    List<string> valuesList = filter.GetValues(criterionName);
//    if (valuesList.Count == 0)
//        return defaultValue;
//    else
//        return valuesList[0];
//}

//public static bool FilterParamsDicitonary(IReadOnlyDictionary<string, string?> paramsDictionary, FilterInfo filter)
//{
//    if (filter.CriteriaInfosList is null)
//        return true;

//    foreach (CriteriaInfo criterionInfo in filter.CriteriaInfosList)
//    {
//        if (criterionInfo.ValuesList is null)
//            continue;

//        paramsDictionary.TryGetValue(criterionInfo.CriteriaName, out var paramValue);
//        if (paramValue is null)
//            return false;

//        if (!criterionInfo.ValuesList.Contains(paramValue, StringComparer.InvariantCultureIgnoreCase))
//            return false;
//    }

//    return true;
//}

//    
//}

//public static bool FilterParamsDicitonary(
//    string searchString,
//    CaseInsensitiveOrderedDictionary<string?> paramsDictionary,
//    params string[] paramNames)
//{
//    if (String.IsNullOrEmpty(searchString))
//        return true;

//    foreach (string paramName in paramNames)
//    {
//        var paramValue = paramsDictionary.TryGetValue(paramName);

//        if (paramValue is not null && paramValue.Contains(searchString, StringComparison.InvariantCultureIgnoreCase))
//            return true;
//    }

//    return false;
//}

//public static bool FilterParamsDicitonary(
//    List<string> values,
//    CaseInsensitiveOrderedDictionary<string?> paramsDictionary,
//    params string[] paramNames)
//{
//    foreach (string paramName in paramNames)
//    {
//        var paramValue = paramsDictionary.TryGetValue(paramName);

//        if (paramValue is not null && values.Contains(paramValue, StringComparer.InvariantCultureIgnoreCase))
//            return true;
//    }

//    return false;
//}