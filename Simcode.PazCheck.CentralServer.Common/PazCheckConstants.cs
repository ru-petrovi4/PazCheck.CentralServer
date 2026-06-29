using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common
{
    public static class PazCheckConstants
    {
        public const string MetaParamValue_CsvDbVersion = "10";

        public const string SystemUnitIdentifier = "system";
        public const string SystemUnitIdentifier_LowerCase = "system";

        /// <summary>
        ///     Change this value to reset roles permissions to defaults (except AD Group names).
        /// </summary>
        public const string MetaParamValue_RolesDataGuid = "75A65398-FF81-4154-A986-850AB8A1A36D";        

        public const string TypeIdentifier_Std_File = @"Std_File"; // This identifier is hard coded in front-end.
        public const string TypeIdentifier_Std_CsvFile = @"Std_CsvFile";
        public const string TypeIdentifier_Std_ExcelFile = @"Std_ExcelFile";  // CentralServer.Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile        
        public const string TypeIdentifier_Std_ExcelFile_Extended = @"Std_ExcelFile_Extended"; // CentralServer.Common.Properties.Resources.DestinationTypeDesc_Std_ExcelFile_Extended
        public const string TypeIdentifier_HumanReadable_ExcelFile = @"HumanReadable_ExcelFile"; // CentralServer.Common.Properties.Resources.DestinationTypeDesc_HumanReadable_ExcelFile

        public const string ContentDirective_FileFormat = @"!FileFormat";
        public const string ContentDirective_FormatVersion = @"!FormatVersion";
        public const string ContentDirective_RootCollectionMode = @"!RootCollectionMode";
        public const string ContentDirective_ChildCollectionMode = @"!ChildCollectionMode";
        public const string ContentDirective_DataCollectionMode = @"!DataCollectionMode";
        public const string ContentDirective_Collection = @"!Collection";

        public const string StdFileFormat_JsonObjects = @"JsonObjects";
        public const string StdFileFormat_TableObjects = @"TableObjects";
        public const string StdFileFormat_JournalParamValues = @"JournalParamValues";
        public const string StdFileFormat_TableCeMatrix = @"TableCeMatrix";

        public const string Constant_CauseCondition = "%(CauseCondition)";
        public const string Constant_EffectCondition = "%(EffectCondition)";

        public const char AeCondition_FieldsSeparator = '|';

        public const string ParamNamePrefixSeparator = @":";        

        public const string BasePcObject_Overview_Template = @"Overview_" + IdentifierEnding_Template;

        public const string BasePcObject_Default_Template = @"Default_" + IdentifierEnding_Template;

        public const string BasePcObject_Unit_Template = @"Unit_" + IdentifierEnding_Template;

        public const string BasePcObject_OtherArea_Template = @"OtherArea_" + IdentifierEnding_Template;

        public const string BasePcObject_OtherItem_Template = @"OtherItem_" + IdentifierEnding_Template;

        public const string BasePcObject_SystemArea_Template = @"SystemArea_" + IdentifierEnding_Template;

        public const string BasePcObject_SystemProcess_Template = @"SystemProcess_" + IdentifierEnding_Template;

        public const string BasePcObject_SystemAddon_Template = @"SystemAddon_" + IdentifierEnding_Template;

        public const string PcObject_SystemArea = @"SystemArea";

        public const string PcObject_OtherArea = @"OtherArea";

        public const string PcEntityType_Main = @"Main";        

        public const string PcObjectEventType_ActuatorAction = @"ActuatorAction";        

        public const string PcObjectEventType_EmergencyShutdown = @"EmergencyShutdown";        

        public const string PcObjectEventType_ManualCheck = @"ManualCheck";

        public const string PcObjectEventType_DBK = @"DBK";

        public const string PcObjectEventType_ForcedVariable = @"ForcedVariable";

        public const string TypeIdentifier_CeMatrix = @"CeMatrix";

        public const string TypeIdentifier_Tag = @"Tag";

        public const string TypeIdentifier_Actuator = @"Actuator";

        public const string TypeIdentifier_PcObject = @"PcObject";

        public const string TypeIdentifier_Legend = @"Legend";

        public const string TypeIdentifier_ProjectVersion = @"ProjectVersion";        

        /// <summary>
        ///     Является ли тэг локальной переменной.
        /// </summary>
        public const string ParamName_IsVariable = @"IsVariable";

        /// <summary>
        ///     Название.
        /// </summary>
        public const string ParamName_Title = @"Title";

        /// <summary>
        ///     Описание.
        /// </summary>
        public const string ParamName_Desc = @"Desc";        

        /// <summary>
        ///     Производитель.
        /// </summary>
        public const string ParamName_Manufacturer = @"Manufacturer";

        /// <summary>
        ///     Примечание.
        /// </summary>
        public const string ParamName_Comment = @"Comment";

        /// <summary>
        ///     Примечание.
        /// </summary>
        public const string JournalParamName_AddonStatus = @"AddonStatus";

        /// <summary>
        ///     Может ли быть причиной.
        /// </summary>
        public const string ParamName_CanBeCause = @"CanBeCause";

        /// <summary>
        ///     Может ли быть следствием.
        /// </summary>
        public const string ParamName_CanBeEffect = @"CanBeEffect";

        /// <summary>
        ///     Шаблон.
        /// </summary>
        public const string ParamNameBase_Template = @"Template";

        /// <summary>
        ///     Префикс параметра данных для объекта мониторинга.
        /// </summary>
        public const string ParamNamePrefix_Data = @"Data:";

        /// <summary>
        ///     Тип матрицы ПСС.
        /// </summary>
        public const string ParamName_CeMatrixTemplate = TypeIdentifier_CeMatrix + ParamNamePrefixSeparator + ParamNameBase_Template;

        /// <summary>
        ///     Шаблон тэга.
        /// </summary>
        public const string ParamName_TagTemplate = TypeIdentifier_Tag + ParamNamePrefixSeparator + ParamNameBase_Template;

        /// <summary>
        ///     Устройство тэга.
        /// </summary>
        public const string ParamName_TagActuator = TypeIdentifier_Tag + ParamNamePrefixSeparator + "Actuator";

        /// <summary>
        ///     Модель устройства.
        /// </summary>
        public const string ParamName_ActuatorTemplate = TypeIdentifier_Actuator + ParamNamePrefixSeparator + ParamNameBase_Template;

        /// <summary>
        ///     Идентификатор шаблона объекта мониторинга.
        /// </summary>
        public const string ParamName_PcObjectTemplate = TypeIdentifier_PcObject + ParamNamePrefixSeparator + ParamNameBase_Template;

        /// <summary>
        ///     Родительский объект объекта мониторинга.
        /// </summary>
        public const string ParamName_PcObjectParent = TypeIdentifier_PcObject + ParamNamePrefixSeparator + @"Parent";

        /// <summary>
        ///     Шаблон легенды.
        /// </summary>
        public const string ParamName_LegendTemplate = TypeIdentifier_Legend + ParamNamePrefixSeparator + ParamNameBase_Template;

        /// <summary>
        ///     Источник
        /// </summary>
        public const string ParamNameBase_Source = @"Source";

        /// <summary>
        ///     Источник матрицы (Локальная/АСУБ)
        /// </summary>
        public const string ParamName_CeMatrixSource = TypeIdentifier_CeMatrix + ParamNamePrefixSeparator + ParamNameBase_Source;

        /// <summary>
        ///     Источник информации о тэге (Локальный/АСУБ)
        /// </summary>
        public const string ParamName_TagSource = TypeIdentifier_Tag + ParamNamePrefixSeparator + ParamNameBase_Source;

        /// <summary>
        ///     Источник информации об исполнительном механизме
        /// </summary>
        public const string ParamName_BaseActuatorSource = TypeIdentifier_Actuator + ParamNamePrefixSeparator + ParamNameBase_Source;

        /// <summary>
        ///     Источник информации о Объект мониторинга
        /// </summary>
        public const string ParamName_SafetyControllerSource = TypeIdentifier_PcObject + ParamNamePrefixSeparator + ParamNameBase_Source;

        /// <summary>
        ///     Источник информации
        /// </summary>
        public const string ParamName_LegendSource = TypeIdentifier_Legend + ParamNamePrefixSeparator + ParamNameBase_Source;

        /// <summary>
        ///     Источник версии проекта (Локальная/АСУБ)
        /// </summary>
        public const string ParamName_ProjectVersionSource = TypeIdentifier_ProjectVersion + ParamNamePrefixSeparator + ParamNameBase_Source;

        /// <summary>
        ///     Максимально допустимое время срабатывания исполнительного механизма
        /// </summary>
        public const string ParamName_CommandExecutionDurationMax = @"CommandExecutionDurationMax";

        public const string ParamName_CommandExecutionType_NotActivated = @"NotActivated"; // nameof(TriggeredType.NotActivated);
        public const string ParamName_CommandExecutionType_SuccessFirstTriggered = @"SuccessFirstTriggered"; // nameof(TriggeredType.SuccessFirstTriggered);
        public const string ParamName_CommandExecutionType_SuccessTriggered = @"SuccessTriggered"; // nameof(TriggeredType.SuccessTriggered);
        public const string ParamName_CommandExecutionType_LateTriggered = @"LateTriggered"; // nameof(TriggeredType.LateTriggered);
        public const string ParamName_CommandExecutionType_NotTriggered = @"NotTriggered"; // nameof(TriggeredType.NotTriggered);
        public const string ParamName_CommandExecutionType_FaultTriggered = @"FaultTriggered"; // nameof(TriggeredType.FaultTriggered);

        #region Trend Config Params                

        /// <summary>
        ///     Источники данных: тэг(и) БДРВ и/или JSON запрос(ы). Разделитель ---
        /// </summary>
        public const string ParamName_In = @"In";

        /// <summary>
        ///     Конвертер значения для источника(-ов) данных
        /// </summary>
        public const string ParamName_Converter = @"Converter";

        /// <summary>
        ///     Тэг БДРВ или константа верхней границы значения
        /// </summary>
        public const string ParamName_ValueMax = @"ValueMax";

        /// <summary>
        ///     Тэг БДРВ или константа нижней границы значения
        /// </summary>
        public const string ParamName_ValueMin = @"ValueMin";

        /// <summary>
        ///     Тэг БДРВ или константа единиц измерения значения
        /// </summary>
        public const string ParamName_ValueEU = @"ValueEU";

        /// <summary>
        ///     Порог чувствительности значения в процентах
        /// </summary>
        public const string ParamName_ValueDeadbandPercentage = @"ValueDeadbandPercentage";

        /// <summary>
        ///     Порог чувствительности значения в абсолютных единицах
        /// </summary>
        public const string ParamName_ValueDeadband = @"ValueDeadband";

        /// <summary>
        ///     Период сканирования на изменения значения
        ///     <see cref="Ssz.Utils.Any" />
        /// </summary>
        public const string ParamName_ValuePeriod = @"ValuePeriod";

        /// <summary>
        ///     Порог чувствительности значения тренда в процентах
        /// </summary>
        public const string ParamName_TrendDeadbandPercentage = @"TrendDeadbandPercentage";

        /// <summary>
        ///     Порог чувствительности значения тренда в абсолютных единицах
        /// </summary>
        public const string ParamName_TrendDeadband = @"TrendDeadband";

        /// <summary>
        ///     Период сканирования на изменения значения тренда
        ///     <see cref="Ssz.Utils.Any" />
        /// </summary>
        public const string ParamName_TrendPeriod = @"TrendPeriod";

        /// <summary>
        ///     Включено ли накопление значений тренда. По умолчанию false.
        /// </summary>
        public const string ParamName_TrendEnabled = @"TrendEnabled";

        /// <summary>
        ///     Период хранения значения тренда. По умолчанию 1M.
        ///     <see cref="Ssz.Utils.Any" />
        /// </summary>
        public const string ParamName_TrendStorePeriod = @"TrendStorePeriod";

        /// <summary>
        ///     Трендовые значения вычисляются на основе других трендов. По умолчанию false.
        /// </summary>
        public const string ParamName_TrendCalculated = @"TrendCalculated";

        #endregion

        /// <summary>
        ///     Время первого сохраненного значения
        /// </summary>
        public const string ParamName_TrendBeginTimeUtc = @"TrendBeginTimeUtc";

        /// <summary>
        ///     Время последнего сохраненного значения
        /// </summary>
        public const string ParamName_TrendEndTimeUtc = @"TrendEndTimeUtc";

        public const string ParamIndex_ValveClose = @"CLOSE";

        public const string ParamIndex_ValveOpen = @"OPEN";        

        /// <summary>
        ///     Команда (для показа пользователю).
        /// </summary>
        public const string ParamName_Command_ToDisplay = @"Command_ToDisplay";        

        /// <summary>
        ///     Результат исполнения команды из матриц ПСС.
        /// </summary>
        public const string ParamName_LogicCommandResultType = @"LogicCommandResultType";

        /// <summary>
        ///     Длительность исполнения команды из матриц ПСС (секунды).
        /// </summary>
        public const string ParamName_LogicCommandExecutionDuration = @"LogicCommandExecutionDuration";

        /// <summary>
        ///     Результат исполнения команды из журнала.
        /// </summary>
        public const string ParamName_JournalCommandResultType = @"JournalCommandResultType";

        /// <summary>
        ///     Длительность исполнения команды из журнала (секунды).
        /// </summary>
        public const string ParamName_JournalCommandExecutionDuration = @"JournalCommandExecutionDuration";

        ///// <summary>
        /////     Результат исполнения команды .
        ///// </summary>
        //public const string ParamName_CommandResultType = @"CommandResultType";

        /// <summary>
        ///     Категория неисправности
        /// </summary>
        public const string ParamName_MalfunctionCategory = @"MalfunctionCategory";

        /// <summary>
        ///     Интенсивность аларма (0..1).
        /// </summary>
        public const string ParamName_AlarmIntensity = @"AlarmIntensity";

        /// <summary>
        ///     Уровень останова (1, 2A, 2B, 2C, 3, 4).
        /// </summary>
        public const string ParamName_EmergencyShutdownLevel = @"EmergencyShutdownLevel";

        /// <summary>
        ///     Время сигнала останова.
        /// </summary>
        public const string ParamName_EmergencyShutdownTime = @"EmergencyShutdownTime";

        /// <summary>
        ///     Тэг события
        /// </summary>
        public const string ParamName_ResultEventTagName = @"ResultEventTagName";

        /// <summary>
        ///     Описание события
        /// </summary>
        public const string ParamName_ResultEventDesc = @"ResultEventDesc";

        /// <summary>
        ///     Время события
        /// </summary>
        public const string ParamName_ResultEventTime = @"ResultEventTime";

        /// <summary>
        ///     Единственная первопричина инцидента, тэг
        /// </summary>
        public const string ParamName_Strict_PrimeCause_TagName = @"Strict_PrimeCause_TagName";

        ///// <summary>
        /////     Единственная первопричина инцидента, время события
        ///// </summary>
        //public const string ParamName_Strict_PrimeCause_Time = @"Strict_PrimeCause_Time";

        ///// <summary>
        /////     Единственная первопричина инцидента, событие
        ///// </summary>
        //public const string ParamName_Strict_PrimeCause_Condition = @"Strict_PrimeCause_Condition";

        ///// <summary>
        /////     Возможные первопричины инцидента (Strict или Rough), если не найдена единственная первопричина.
        ///// </summary>
        //public const string ParamName_PossibleResultSources = @"PossibleResultSources";        

        /// <summary>
        ///     Коэффициент важности
        /// </summary>
        public const string ParamName_SafetyIndexK = @"SafetyIndexK";

        /// <summary>
        ///     Коэффициент важности 2
        /// </summary>
        public const string ParamName_SafetyIndexK2 = @"SafetyIndexK2";

        /// <summary>
        /// 
        /// </summary>
        public const string ParamName_OldState = @"OldState";

        /// <summary>
        /// 
        /// </summary>
        public const string ParamName_NewState = @"NewState";        

        public const string ParamNamePrefix_Reference = @"Reference";

        /// <summary>
        ///     Ссылка на результат анализа.
        /// </summary>
        public const string ParamName_Reference_ResultId = ParamNamePrefix_Reference + ParamNamePrefixSeparator + @"Result";  // nameof(Result)

        /// <summary>
        ///     Процент, выше которого зеленый индекс безопасности.
        /// </summary>
        public const string ParamName_SafetyIndex_Green_Percentage = @"SafetyIndex_Green_Percentage";

        /// <summary>
        ///     Описание зеленого индекса безопасности.
        /// </summary>
        public const string ParamName_SafetyIndex_Green_Desc = @"SafetyIndex_Green_Desc";

        /// <summary>
        ///     Процент, выше которого желтый индекс безопасности.
        /// </summary>
        public const string ParamName_SafetyIndex_Yellow_Percentage = @"SafetyIndex_Yellow_Percentage";

        /// <summary>
        ///     Описание желтого индекса безопасности.
        /// </summary>
        public const string ParamName_SafetyIndex_Yellow_Desc = @"SafetyIndex_Yellow_Desc";

        /// <summary>
        ///     Процент, выше которого красный индекс безопасности.
        /// </summary>
        public const string ParamName_SafetyIndex_Red_Percentage = @"SafetyIndex_Red_Percentage";

        /// <summary>
        ///     Описание красного индекса безопасности.
        /// </summary>
        public const string ParamName_SafetyIndex_Red_Desc = @"SafetyIndex_Red_Desc";

        /// <summary>
        ///     Команда
        /// </summary>
        public const string ConditionCategory_Command = @"Command";

        /// <summary>
        ///     Статус исполнительного механизма
        /// </summary>
        public const string ConditionCategory_Actuator = @"Actuator";

        /// <summary>
        ///     Причина
        /// </summary>
        public const int ResultEventType_Cause = 0;
        /// <summary>
        ///     Следствие
        /// </summary>
        public const int ResultEventType_Effect = 1;
        /// <summary>
        ///     Дополнительная причина
        /// </summary>
        public const int ResultEventType_AdditionalCause = 2;
        /// <summary>
        ///     Дополнительное следствие
        /// </summary>
        public const int ResultEventType_AdditionalEffect = 3;        
        /// <summary>
        ///     Выход логики для ячейки
        /// </summary>
        public const int ResultEventType_CellLogicOutput = 5;
        /// <summary>
        ///     Выход логики для столбца (итоговый выход логики)
        /// </summary>
        public const int ResultEventType_ColumnLogicOutput = 6;
        
        public const string ValueType_DateTime = @"DateTime";

        //public const string ValueType_Boolean = @"Boolean"; // Use ValuesList = [ "true", "false" ]

        public const string ConfigurationKey_Process_SourceId = @"Process_SourceId";

        public const string ConfigurationKey_Process_SourceIdToDisplay = @"Process_SourceIdToDisplay";

        public const string ConfigurationKey_GrafanaUrl = @"GrafanaUrl";

        public const string ConfigurationKey_PostgreCryptoPasswordFileName = @"PostgreCryptoPasswordFileName";

        public const string DirectoryName_CsvDb = @"CsvDb";        

        public const string ParamName_EventTypeTitle = @"EventTypeTitle";

        public const string ParamName_EventTypeDesc = @"EventTypeDesc";

        public const string ParamName_EventTitle = @"EventTitle";

        public const string ParamName_EventDesc = @"EventDesc";

        public const string ParamName_UnitIdentifier = @"UnitIdentifier";

        public const string ParamName_ProjectTitle = @"ProjectTitle";

        public const char MetaParamName_FieldsSeparator = '|';

        /// <summary>
        ///     API Structure GUID
        ///     Always exists.
        /// </summary>
        public const string MetaParamName_RolesDataGuid = "RolesDataGuid";

        public const string MetaParamName_CsvDbVersion = "CsvDbVersion";

        /// <summary>
        ///     PcObjects Tree GUID
        ///     Always exists.
        /// </summary>
        public const string MetaParamNameBase_Monitoring_Config = "Monitoring_Config";

        /// <summary>
        ///     PcObjects Tree SafetyIndices GUID
        ///     Always exists.
        /// </summary>
        public const string MetaParamNameBase_Monitoring_Data = "Monitoring_Data";

        /// <summary>
        ///     ParamDescs, Units, BasePcObjects, PcObjects, PcObjectEvents, PcObjectEventTypes all data GUID, except (JournalParamValuesCollection, SafetyIndex, SafetyIndexDesc, SafetyIndex_LastChangeTimeUtc)
        ///     Always exists.
        /// </summary>
        public const string MetaParamNameBase_ParamDescs_Units_BasePcObjects_PcObjects_Guid = "ParamDescs_Units_BasePcObjects_PcObjects_Guid";

        /// <summary>
        ///     JournalParamValuesCollection
        ///     Always exists.
        /// </summary>
        public const string MetaParamNameBase_JournalParamValuesCollection_Guid = "JournalParamValuesCollection_Guid";

        /// <summary>
        ///     Project data in current version chaged. ProjectId is arg.
        /// </summary>
        public const string MetaParamNameBase_Project = @"Project";

        /// <summary>
        ///     Project data in current version chaged. ProjectId is arg.
        /// </summary>
        public const string MetaParamNameBase_Paused = @"Paused";

        //public const string MetaParamNameBase_PeriodicEvent = @"PeriodicEvent";

        public static string[] SourceEvent_TypesCollection = [
                MetaParamNameBase_ActiveProjectVersionChanged,
                MetaParamNameBase_Diagnost_UnitEventsAutoLoaded,
                MetaParamNameBase_Diagnost_UnitEventsAutoAnalyzed,
                MetaParamNameBase_Diagnost_UnitEventsUserAnalyzed                
            ];

        public const string MetaParamNameBase_ActiveProjectVersionChanged = @"ActiveProjectVersionChanged";

        public const string MetaParamNameBase_Diagnost_UnitEventsAutoLoaded = @"Diagnost_UnitEventsAutoLoaded";

        public const string MetaParamNameBase_Diagnost_UnitEventsAutoAnalyzed = @"Diagnost_UnitEventsAutoAnalyzed";

        public const string MetaParamNameBase_Diagnost_UnitEventsUserAnalyzed = @"Diagnost_UnitEventsUserAnalyzed";

        public const string MetaParamNameBase_ReportSubscription = @"ReportSubscription";

        public const string CriterionName_Id = @"Id";

        public const string CriterionName_Params = @"Params";

        public const string CriterionName_Values = @"Values";

        public const string CriterionName_EventParams = @"EventParams";

        public const string CriterionName_TagName = @"TagName";

        public const string CriterionName_Identifier = @"Identifier";

        public const string CriterionName_Unit = @"Unit";

        public const string CriterionName_PcObject_Parent = @"PcObject:Parent";

        /// <summary>
        ///     Идентификатор шаблона
        /// </summary>
        public const string CriterionName_PcObject_Template = @"PcObject:Template";      

        public const string CriterionName_ResultId = @"ResultId";

        public const string CriterionName_Priority = @"Priority";

        public const string CriterionName_Severity = @"Severity";

        public const string CriterionName_Succeeded = @"Succeeded";

        public const string CriterionName_IsTemplate = @"IsTemplate";

        public const string CriterionName_LogLevel = @"LogLevel";

        public const string CriterionName_TriggeredType = @"TriggeredType";

        public const string CriterionName_EventType = @"EventType"; // nameof(Serialization.PcObjectEvent.EventType);

        public const string CriterionName_From = @"From";

        public const string CriterionName_To = @"To";

        public const string CriterionName_Children = "Children";        

        public const string QueryPartName_QType = @"QType";

        public const string QueryPartName_QString = "QString";

        public const string QueryPartName_FilterMode = "FilterMode";

        public const string QueryPartName_Format = "Format";

        public const string FilterMode_Value_AddToFilter = "AddToFilter";

        public const string FilterMode_Value_NewFilter = "NewFilter";

        public const string QueryType_DataAccess = "DataAccess";
        public const string QueryType_DataAccess_UpperCase = "DATAACCESS";

        public const string QueryType_Events = "Events";
        public const string QueryType_Events_UpperCase = "EVENTS";

        /// <summary>
        ///     Текущий индекс безопасности 0-100.
        /// </summary>
        public const string ParamName_SafetyIndex = @"SafetyIndex";

        /// <summary>
        ///     Описание текущего индекса безопасности.
        /// </summary>
        public const string ParamName_SafetyIndexDesc = @"SafetyIndexDesc";

        public const string QueryType_InformationSecurityEvents = "InformationSecurityEvents";
        public const string QueryType_InformationSecurityEvents_UpperCase = "INFORMATIONSECURITYEVENTS";

        /// <summary>
        ///     Deatils about SafetyIndex calculation
        /// </summary>
        public const string QueryType_SafetyIndexInfo = "SafetyIndexInfo";
        public const string QueryType_SafetyIndexInfo_UpperCase = "SAFETYINDEXINFO";

        /// <summary>
        ///     Deatils about SafetyIndex calculation
        /// </summary>
        public const string QueryType_SafetyIndexInfo_UrgentOnly = "SafetyIndexInfo_UrgentOnly";
        public const string QueryType_SafetyIndexInfo_UrgentOnly_UpperCase = "SAFETYINDEXINFO_URGENTONLY";

        /// <summary>
        ///     Deatils about SafetyIndex calculation
        /// </summary>
        public const string QueryType_SafetyIndexInfoV2 = "SafetyIndexInfoV2";
        public const string QueryType_SafetyIndexInfoV2_UpperCase = "SAFETYINDEXINFOV2";

        /// <summary>
        ///     Details about SafetyIndex calculation
        /// </summary>
        public const string QueryType_SafetyIndexInfo_UrgentOnlyV2 = "SafetyIndexInfo_UrgentOnlyV2";
        public const string QueryType_SafetyIndexInfo_UrgentOnlyV2_UpperCase = "SAFETYINDEXINFO_URGENTONLYV2";

        /// <summary>
        ///     Details about SafetyIndex calculation
        /// </summary>
        public const string QueryType_DataParamsCurrentValues = "DataParamsCurrentValues";
        public const string QueryType_DataParamsCurrentValues_UpperCase = "DATAPARAMSCURRENTVALUES";

        public const string QueryType_System = "System";
        public const string QueryType_System_UpperCase = "SYSTEM";

        public const string QueryType_Values = "Values";
        public const string QueryType_Values_UpperCase = "VALUES";

        public const string QueryType_Value = "Value";
        public const string QueryType_Value_UpperCase = "VALUE";

        public const string QueryType_SafetyIndexFromChildren = "SafetyIndexFromChildren";
        public const string QueryType_SafetyIndexFromChildren_UpperCase = "SAFETYINDEXFROMCHILDREN";

        public const string QueryType_SafetyIndexFromChildrenV2 = "SafetyIndexFromChildrenV2";
        public const string QueryType_SafetyIndexFromChildrenV2_UpperCase = "SAFETYINDEXFROMCHILDRENV2";

        public const string QueryType_ObjectsCount = "ObjectsCount";
        public const string QueryType_ObjectsCount_UpperCase = "OBJECTSCOUNT";

        public const string QueryType_ActiveEventsCount = "ActiveEventsCount";
        public const string QueryType_ActiveEventsCount_UpperCase = "ACTIVEEVENTSCOUNT";

        public const string QueryType_EventsCount = "EventsCount";
        public const string QueryType_EventsCount_UpperCase = "EVENTSCOUNT";

        public const string QueryType_DateTime = "DateTime";
        public const string QueryType_DateTime_UpperCase = "DATETIME";

        public const string DataType_Enum = "Enum";

        public const string DataType_Single = "Single";

        public const string DataType_Int32 = "Int32";

        public const string DataType_Boolean = "Boolean";

        public const string DataType_TimeSpan = "TimeSpan";

        public const string DataType_DateTime = "DateTime";

        public const string MetadataParamName_Format = "Format";

        public const string ExportEntitiesName_ReferenceEntities = "ReferenceEntities";

        public const string IdentifierEnding_Template = "Template";
        public const string IdentifierEnding_Template_LowerCase = "template";

        public const string MetaParam_Type_InternalEvent = "InternalEvent";

        public const string MetaParam_Type_MainProcess_InternalEvent = "MainProcess_InternalEvent";

        public const string MetaParam_Type_MainProcess_ReportSubscription = "MainProcess_ReportSubscription";

        public const string MetaParam_Type_HubEvent = "HubEvent";

        public const string MetaParam_Type_MetaParam_Paused = "MetaParam_Paused";

        public const string HubGroup_MonitoringSubscribe = "MonitoringSubscribe";

        public const string HubMethod_Project_Changed = "Project_Changed";

        public const string HubMethod_Monitoring_ConfigChanged = "Monitoring_ConfigChanged";

        public const string HubMethod_Monitoring_DataChanged = "Monitoring_DataChanged";

        public static readonly string[] ConverterPartsSeparator = ["->"];

        public const string PartsSeparator = @"---";        
    }
}

///// <summary>
/////     Результат исполнения команды.
///// </summary>
//public const string ParamName_ActuationType = @"ActuationType";

///// <summary>
/////     Команда из матриц ПСС.
///// </summary>
//public const string CommandSource_Logic = @"Logic";

///// <summary>
/////     Команда из журнала.
///// </summary>
//public const string CommandSource_Journal = @"Journal";

///// <summary>
/////     ActiveProjectVersions all data GUID
/////     Always exists.
///// </summary>
//public const string MetaParamNameBase_ActiveProjectVersions_Guid = "Units_BasePcObjects_PcObjects_Guid";

///// <summary>
/////     Выход логики для строки (перед логикой ячеек)
///// </summary>
//public const int ResultEventType_RowLogicOutput = 4;

///// <summary>
/////     Результат исполнения команды (для показа пользователю).
///// </summary>
//public const string ParamName_CommandResult_ToDisplay = @"CommandResult_ToDisplay";