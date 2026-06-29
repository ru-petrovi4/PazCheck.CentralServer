using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common
{
    /// <summary>
    ///     :Template is not processed.
    /// </summary>
    public class ProjectAllParamValues
    {
        /// <summary>
        ///     Project.DataGuid for unsaved changes
        /// </summary>
        public string ProjectDataGuid { get; set; } = @"";

        public ProjectVersion? ProjectVersion { get; set; }

        /// <summary>
        ///     ['CeMatrix.Identifier'.'Param.Name', Value]
        ///     CeMatrix:Template is not processed.
        /// </summary>
        public FrozenDictionary<string, string> CeMatricesParamValues { get; set; } = null!;

        /// <summary>
        ///     [CeMatrix.Identifier, [VersionedParamBase]]
        ///     CeMatrix:Template is not processed.
        /// </summary>
        public FrozenDictionary<string, List<VersionedParamBase>> CeMatricesParams { get; set; } = null!;

        /// <summary>        
        ///     ['Tag.Identifier'.'Param.Name', Value]
        ///     Tag:Template is not processed.
        /// </summary>
        public FrozenDictionary<string, string> TagsParamValues { get; set; } = null!;

        /// <summary>
        ///     [Tag.Identifier, [VersionedParamBase]]
        ///     Tag:Template is not processed.
        /// </summary>
        public FrozenDictionary<string, List<VersionedParamBase>> TagsParams { get; set; } = null!;

        /// <summary>
        ///     [TagName, List[TagCondition]]
        ///     Tag:Template is not processed.
        /// </summary>
        public FrozenDictionary<string, List<TagCondition>> TagConditions { get; set; } = null!;

        /// <summary>
        ///     ['BaseActuator.Identifier'.'Param.Name', Value]
        ///     Actuator:Template is not processed.
        /// </summary>
        public FrozenDictionary<string, string> BaseActuatorsParamValues { get; set; } = null!;

        /// <summary>
        ///     [BaseActuator.Identifier, [VersionedParamBase]]
        ///     Actuator:Template is not processed.
        /// </summary>
        public FrozenDictionary<string, List<VersionedParamBase>> BaseActuatorsParams { get; set; } = null!;

        /// <summary>
        ///     ['SafetyController.Identifier'.'Param.Name', Value]
        ///     SafetyController:Template is not processed.
        /// </summary>
        public FrozenDictionary<string, string> SafetyControllersParamValues { get; set; } = null!;

        /// <summary>
        ///     [SafetyController.Identifier, [VersionedParamBase]]
        ///     SafetyController:Template is not processed.
        /// </summary>
        public FrozenDictionary<string, List<VersionedParamBase>> SafetyControllersParams { get; set; } = null!;

        /// <summary>
        ///     ['Legend.Identifier'.'Param.Name', Value]
        ///     Legend:Template is not processed.
        /// </summary>
        public FrozenDictionary<string, string> LegendsParamValues { get; set; } = null!;

        /// <summary>
        ///     [Legend.Identifier, [VersionedParamBase]]
        ///     Legend:Template is not processed.
        /// </summary>
        public FrozenDictionary<string, List<VersionedParamBase>> LegendsParams { get; set; } = null!;
    }

    /// <summary>
    ///     :Template is not processed.
    /// </summary>
    public class ProjectAllParamValuesTemp
    {
        /// <summary>
        ///     [CeMatrix.Param, Value]
        ///     CeMatrix:Template is not processed.
        /// </summary>
        public CaseInsensitiveOrderedDictionary<string> CeMatricesParamValues { get; } = new();

        /// <summary>
        ///     [CeMatrix, [VersionedParamBase]]
        ///     <para>CeMatrix:Template is not processed. List can be empty.</para>
        /// </summary>
        public CaseInsensitiveOrderedDictionary<List<VersionedParamBase>> CeMatricesParams { get; } = new();

        /// <summary>
        ///     [Tag.Param, Value]
        ///     Tag:Template is not processed.
        /// </summary>
        public CaseInsensitiveOrderedDictionary<string> TagsParamValues { get; } = new();

        /// <summary>
        ///     [Tag, [VersionedParamBase]]
        ///     <para>Tag:Template is not processed. List can be empty.</para>
        /// </summary>
        public CaseInsensitiveOrderedDictionary<List<VersionedParamBase>> TagsParams { get; } = new();

        /// <summary>
        ///     [TagName, List[TagCondition]]
        ///     Tag:Template is not processed.
        /// </summary>
        public CaseInsensitiveOrderedDictionary<List<TagCondition>> TagConditions { get; } = new();

        /// <summary>
        ///     [BaseActuator.Param, Value]
        ///     Actuator:Template is not processed.
        /// </summary>
        public CaseInsensitiveOrderedDictionary<string> BaseActuatorsParamValues { get; } = new();

        /// <summary>
        ///     [BaseActuator, [VersionedParamBase]]
        ///     <para>Actuator:Template is not processed. List can be empty.</para>
        /// </summary>
        public CaseInsensitiveOrderedDictionary<List<VersionedParamBase>> BaseActuatorsParams { get; } = new();

        /// <summary>
        ///     [SafetyController.Param, Value]
        ///     SafetyController:Template is not processed.
        /// </summary>
        public CaseInsensitiveOrderedDictionary<string> SafetyControllersParamValues { get; } = new();

        /// <summary>
        ///     [SafetyController, [VersionedParamBase]]
        ///     <para>PcObject:Template is not processed. List can be empty.</para>
        /// </summary>
        public CaseInsensitiveOrderedDictionary<List<VersionedParamBase>> SafetyControllersParams { get; } = new();

        /// <summary>
        ///     [Legend.Param, Value]
        ///     <para>Legend:Template is not processed.</para>
        /// </summary>
        public CaseInsensitiveOrderedDictionary<string> LegendsParamValues { get; } = new();

        /// <summary>
        ///     [Legend, [VersionedParamBase]]
        ///     <para>Legend:Template is not processed. List can be empty.</para>
        /// </summary>
        public CaseInsensitiveOrderedDictionary<List<VersionedParamBase>> LegendsParams { get; } = new();
    }
}
