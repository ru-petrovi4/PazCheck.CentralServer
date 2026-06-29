using Simcode.PazCheck.CentralServer.Common.Properties;
using Ssz.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Simcode.PazCheck.CentralServer.Common.Serialization
{    
    public class SerializationRootObject
    {
        [PcDisplayName(ResourceStrings.Units)]
        public List<Unit>? Units { get; set; }

        [PcDisplayName(ResourceStrings.DbFiles)]
        public List<DbFile>? DbFiles { get; set; }

        [PcDisplayName(ResourceStrings.ParamDescs)]
        public List<ParamDesc>? ParamDescs { get; set; }

        [PcDisplayName(ResourceStrings.ProjectVersionTypes)]
        public List<ProjectVersionType>? ProjectVersionTypes { get; set; }

        [PcDisplayName(ResourceStrings.PcObjectEventTypes)]
        public List<PcObjectEventType>? PcObjectEventTypes { get; set; }

        [PcDisplayName(ResourceStrings.CeMatrices)]
        public List<CeMatrix>? CeMatrices { get; set; }

        [PcDisplayName(ResourceStrings.Tags)]
        public List<Tag>? Tags { get; set; }

        [PcDisplayName(ResourceStrings.BaseActuators)]
        public List<Actuator>? Actuators { get; set; }

        [PcDisplayName(ResourceStrings.SafetyControllers)]
        public List<MonitoringObject>? MonitoringObjects { get; set; }

        [PcDisplayName(ResourceStrings.Legends)]
        public List<Legend>? Legends { get; set; }

        [PcDisplayName(ResourceStrings.BasePcObjects)]
        public List<BasePcObject>? BasePcObjects { get; set; }

        [PcDisplayName(ResourceStrings.PcObjects)]
        public List<PcObject>? PcObjects { get; set; }

        public List<string> GetLocalizedInfo()
        {
            List<string> result = new(12);

            foreach (var propertyInfo in typeof(SerializationRootObject).GetProperties()
                       .Where(pi => pi.PropertyType.Name == @"List`1"))
            {
                var value = propertyInfo.GetValue(this, null);
                if (value is not null)
                {
                    string displayName = propertyInfo.GetCustomAttribute<PcDisplayNameAttribute>()!.DisplayName;
                    result.Add(displayName); 
                }
            }

            return result;
        }
    }

    public class ImportSerializationRootObjectResult
    {
        public ImportSerializationResult<EntityFramework.CeMatrix>? CeMatrices_ImportSerializationResult { get; set; } = null;

        public ImportSerializationResult<EntityFramework.Tag>? Tags_ImportSerializationResult { get; set; } = null;

        public ImportSerializationResult<EntityFramework.BaseActuator>? BaseActuators_ImportSerializationResult { get; set; } = null;

        public ImportSerializationResult<EntityFramework.SafetyController>? SafetyControllers_ImportSerializationResult { get; set; } = null;

        public ImportSerializationResult<EntityFramework.Legend>? Legends_ImportSerializationResult { get; set; } = null;

        public ImportSerializationResultEx<EntityFramework.BasePcObject>? BasePcObjects_ImportSerializationResult { get; set; } = null;

        public ImportSerializationResultEx<EntityFramework.PcObject>? PcObjects_ImportSerializationResult { get; set; } = null;

        public string GetLocalizedInfo_Html()
        {
            string result = @"";

            string totalResult = @"";
            if (CeMatrices_ImportSerializationResult is not null)
                totalResult += Properties.Resources.CeMatrices + ": " + (CeMatrices_ImportSerializationResult.Added.Count + CeMatrices_ImportSerializationResult.Updated.Count) + "<br>";
            if (Tags_ImportSerializationResult is not null)
                totalResult += Properties.Resources.Tags + ": " + (Tags_ImportSerializationResult.Added.Count + Tags_ImportSerializationResult.Updated.Count) + "<br>";
            if (BaseActuators_ImportSerializationResult is not null)
                totalResult += Properties.Resources.BaseActuators + ": " + (BaseActuators_ImportSerializationResult.Added.Count + BaseActuators_ImportSerializationResult.Updated.Count) + "<br>";
            if (SafetyControllers_ImportSerializationResult is not null)
                totalResult += Properties.Resources.SafetyControllers + ": " + (SafetyControllers_ImportSerializationResult.Added.Count + SafetyControllers_ImportSerializationResult.Updated.Count) + "<br>";
            if (Legends_ImportSerializationResult is not null)
                totalResult += Properties.Resources.Legends + ": " + (Legends_ImportSerializationResult.Added.Count + Legends_ImportSerializationResult.Updated.Count) + "<br>";
            if (BasePcObjects_ImportSerializationResult is not null)
                totalResult += Properties.Resources.BasePcObjects + ": " + (BasePcObjects_ImportSerializationResult.Added.Count + BasePcObjects_ImportSerializationResult.Updated.Count) + "<br>";
            if (PcObjects_ImportSerializationResult is not null)
                totalResult += Properties.Resources.PcObjects + ": " + (PcObjects_ImportSerializationResult.Added.Count + PcObjects_ImportSerializationResult.Updated.Count) + "<br>";
            if (totalResult != @"")
                result += "<strong>" + Properties.Resources.Total + ":</strong><br>" + totalResult + "<br>";

            string addedResult = @"";
            if (CeMatrices_ImportSerializationResult is not null)
                addedResult += Properties.Resources.CeMatrices + ": " + CeMatrices_ImportSerializationResult.Added.Count + "<br>";
            if (Tags_ImportSerializationResult is not null)
                addedResult += Properties.Resources.Tags + ": " + Tags_ImportSerializationResult.Added.Count + "<br>";
            if (BaseActuators_ImportSerializationResult is not null)
                addedResult += Properties.Resources.BaseActuators + ": " + BaseActuators_ImportSerializationResult.Added.Count + "<br>";
            if (SafetyControllers_ImportSerializationResult is not null)
                addedResult += Properties.Resources.SafetyControllers + ": " + SafetyControllers_ImportSerializationResult.Added.Count + "<br>";
            if (Legends_ImportSerializationResult is not null)
                addedResult += Properties.Resources.Legends + ": " + Legends_ImportSerializationResult.Added.Count + "<br>";
            if (BasePcObjects_ImportSerializationResult is not null)
                addedResult += Properties.Resources.BasePcObjects + ": " + BasePcObjects_ImportSerializationResult.Added.Count + "<br>";
            if (PcObjects_ImportSerializationResult is not null)
                addedResult += Properties.Resources.PcObjects + ": " + PcObjects_ImportSerializationResult.Added.Count + "<br>";
            if (addedResult != @"")
                result += "<strong>" + Properties.Resources.Added + ":</strong><br>" + addedResult + "<br>";

            string updatedResult = @"";
            if (CeMatrices_ImportSerializationResult is not null)
                updatedResult += Properties.Resources.CeMatrices + ": " + CeMatrices_ImportSerializationResult.Updated.Count + "<br>";
            if (Tags_ImportSerializationResult is not null)
                updatedResult += Properties.Resources.Tags + ": " + Tags_ImportSerializationResult.Updated.Count + "<br>";
            if (BaseActuators_ImportSerializationResult is not null)
                updatedResult += Properties.Resources.BaseActuators + ": " + BaseActuators_ImportSerializationResult.Updated.Count + "<br>";
            if (SafetyControllers_ImportSerializationResult is not null)
                updatedResult += Properties.Resources.SafetyControllers + ": " + SafetyControllers_ImportSerializationResult.Updated.Count + "<br>";
            if (Legends_ImportSerializationResult is not null)
                updatedResult += Properties.Resources.Legends + ": " + Legends_ImportSerializationResult.Updated.Count + "<br>";
            if (BasePcObjects_ImportSerializationResult is not null)
                updatedResult += Properties.Resources.BasePcObjects + ": " + BasePcObjects_ImportSerializationResult.Updated.Count + "<br>";
            if (PcObjects_ImportSerializationResult is not null)
                updatedResult += Properties.Resources.PcObjects + ": " + PcObjects_ImportSerializationResult.Updated.Count + "<br>";
            if (updatedResult != @"")
                result += "<strong>" + Properties.Resources.Updated + ":</strong><br>" + updatedResult + "<br>";

            string deletedResult = @"";
            if (CeMatrices_ImportSerializationResult is not null)
                deletedResult += Properties.Resources.CeMatrices + ": " + CeMatrices_ImportSerializationResult.Deleted.Count + "<br>";
            if (Tags_ImportSerializationResult is not null)
                deletedResult += Properties.Resources.Tags + ": " + Tags_ImportSerializationResult.Deleted.Count + "<br>";
            if (BaseActuators_ImportSerializationResult is not null)
                deletedResult += Properties.Resources.BaseActuators + ": " + BaseActuators_ImportSerializationResult.Deleted.Count + "<br>";
            if (SafetyControllers_ImportSerializationResult is not null)
                deletedResult += Properties.Resources.SafetyControllers + ": " + SafetyControllers_ImportSerializationResult.Deleted.Count + "<br>";
            if (Legends_ImportSerializationResult is not null)
                deletedResult += Properties.Resources.Legends + ": " + Legends_ImportSerializationResult.Deleted.Count + "<br>";
            if (BasePcObjects_ImportSerializationResult is not null)
                deletedResult += Properties.Resources.BasePcObjects + ": " + BasePcObjects_ImportSerializationResult.Deleted.Count + "<br>";
            if (PcObjects_ImportSerializationResult is not null)
                deletedResult += Properties.Resources.PcObjects + ": " + PcObjects_ImportSerializationResult.Deleted.Count + "<br>";
            if (deletedResult != @"")
                result += "<strong>" + Properties.Resources.Deleted + ":</strong><br>" + deletedResult + "<br>";

            return result;
        }        
    }

    /// <summary>
    ///     !!!Warning!!! Entities can be from different dbContexts !!!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ImportSerializationResult<T>       
    {
        /// <summary>
        ///     All current entities.         
        /// </summary>
        public Dictionary<string, T> All { get; set; } = null!;        

        public CaseInsensitiveOrderedDictionary<T> Added { get; } = new();

        public CaseInsensitiveOrderedDictionary<T> Updated { get; } = new();

        public CaseInsensitiveOrderedDictionary<T> Deleted { get; } = new();
    }

    public class ImportSerializationResultEx<T>
    {
        /// <summary>
        ///     All current entities.
        ///     [Entity Identifier, Entities List (possible deleted)]
        /// </summary>
        public Dictionary<string, List<T>> All { get; set; } = null!;

        public CaseInsensitiveOrderedDictionary<T> Added { get; } = new();

        public CaseInsensitiveOrderedDictionary<T> Updated { get; } = new();

        public CaseInsensitiveOrderedDictionary<T> Deleted { get; } = new();
    }

    public class ImportMetadata
    {
        public CollectionMode RootCollectionMode { get; set; }

        /// <summary>
        ///     Applies to: 
        ///     pcObject.Params
        ///     pcObject.PcObjectEvents.Params   
        ///     pcObject.JournalParamValuesCollection
        /// </summary>
        public CollectionMode ChildCollectionMode { get; set; }

        /// <summary>
        ///     Applies to: 
        ///     pcObject.JournalParamValuesCollections.FloatValues
        ///     pcObject.PcObjectEvents
        /// </summary>
        public CollectionMode DataCollectionMode { get; set; }
    }

    public enum CollectionMode
    {        
        /// <summary>
        ///     Adds or updates existing item (DEFAULT)
        /// </summary>
        Update = 0,
        /// <summary>
        ///     Adds or updates existing item, other items are removed form list
        /// </summary>
        Replace,
        /// <summary>
        ///     Adds to empty list (for internal use only).
        /// </summary>
        AddToEmpty,
        /// <summary>
        ///     Only adds objects to collection.
        /// </summary>
        Add
    }
}


//public string GetLocalizedInfo()
//{
//    string result = @"";

//    string addedResult = @"";
//    if (CeMatrices_ImportSerializationResult is not null)
//        addedResult += Properties.Resources.CeMatrices + ": " + CeMatrices_ImportSerializationResult.Added.Count + "; ";
//    if (Tags_ImportSerializationResult is not null)
//        addedResult += Properties.Resources.Tags + ": " + Tags_ImportSerializationResult.Added.Count + "; ";
//    if (BaseActuators_ImportSerializationResult is not null)
//        addedResult += Properties.Resources.BaseActuators + ": " + BaseActuators_ImportSerializationResult.Added.Count + "; ";
//    if (SafetyControllers_ImportSerializationResult is not null)
//        addedResult += Properties.Resources.SafetyControllers + ": " + SafetyControllers_ImportSerializationResult.Added.Count + "; ";
//    if (Legends_ImportSerializationResult is not null)
//        addedResult += Properties.Resources.Legends + ": " + Legends_ImportSerializationResult.Added.Count + "; ";
//    if (BasePcObjects_ImportSerializationResult is not null)
//        addedResult += Properties.Resources.BasePcObjects + ": " + BasePcObjects_ImportSerializationResult.Added.Count + "; ";
//    if (PcObjects_ImportSerializationResult is not null)
//        addedResult += Properties.Resources.PcObjects + ": " + PcObjects_ImportSerializationResult.Added.Count + "; ";
//    if (addedResult != @"")
//        result += Properties.Resources.Added + " - " + addedResult;

//    string updatedResult = @"";
//    if (CeMatrices_ImportSerializationResult is not null)
//        updatedResult += Properties.Resources.CeMatrices + ": " + CeMatrices_ImportSerializationResult.Updated.Count + "; ";
//    if (Tags_ImportSerializationResult is not null)
//        updatedResult += Properties.Resources.Tags + ": " + Tags_ImportSerializationResult.Updated.Count + "; ";
//    if (BaseActuators_ImportSerializationResult is not null)
//        updatedResult += Properties.Resources.BaseActuators + ": " + BaseActuators_ImportSerializationResult.Updated.Count + "; ";
//    if (SafetyControllers_ImportSerializationResult is not null)
//        updatedResult += Properties.Resources.SafetyControllers + ": " + SafetyControllers_ImportSerializationResult.Updated.Count + "; ";
//    if (Legends_ImportSerializationResult is not null)
//        updatedResult += Properties.Resources.Legends + ": " + Legends_ImportSerializationResult.Updated.Count + "; ";
//    if (BasePcObjects_ImportSerializationResult is not null)
//        updatedResult += Properties.Resources.BasePcObjects + ": " + BasePcObjects_ImportSerializationResult.Updated.Count + "; ";
//    if (PcObjects_ImportSerializationResult is not null)
//        updatedResult += Properties.Resources.PcObjects + ": " + PcObjects_ImportSerializationResult.Updated.Count + "; ";
//    if (updatedResult != @"")
//        result += Properties.Resources.Updated + " - " + updatedResult;

//    string deletedResult = @"";
//    if (CeMatrices_ImportSerializationResult is not null)
//        deletedResult += Properties.Resources.CeMatrices + ": " + CeMatrices_ImportSerializationResult.Deleted.Count + "; ";
//    if (Tags_ImportSerializationResult is not null)
//        deletedResult += Properties.Resources.Tags + ": " + Tags_ImportSerializationResult.Deleted.Count + "; ";
//    if (BaseActuators_ImportSerializationResult is not null)
//        deletedResult += Properties.Resources.BaseActuators + ": " + BaseActuators_ImportSerializationResult.Deleted.Count + "; ";
//    if (SafetyControllers_ImportSerializationResult is not null)
//        deletedResult += Properties.Resources.SafetyControllers + ": " + SafetyControllers_ImportSerializationResult.Deleted.Count + "; ";
//    if (Legends_ImportSerializationResult is not null)
//        deletedResult += Properties.Resources.Legends + ": " + Legends_ImportSerializationResult.Deleted.Count + "; ";
//    if (BasePcObjects_ImportSerializationResult is not null)
//        deletedResult += Properties.Resources.BasePcObjects + ": " + BasePcObjects_ImportSerializationResult.Deleted.Count + "; ";
//    if (PcObjects_ImportSerializationResult is not null)
//        deletedResult += Properties.Resources.PcObjects + ": " + PcObjects_ImportSerializationResult.Deleted.Count + "; ";
//    if (deletedResult != @"")
//        result += Properties.Resources.Deleted + " - " + deletedResult;

//    return result;
//}