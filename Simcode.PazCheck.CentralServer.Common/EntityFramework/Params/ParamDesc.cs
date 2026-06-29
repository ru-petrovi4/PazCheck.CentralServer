using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Ssz.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class ParamDesc : Identifiable<string>
    {
        /// <summary>
        ///     Имя свойства (ParamName)
        /// </summary>        
        public override string Id { get; set; } = @"";

        public string IdLower { get; set; } = @"";

        /// <summary>
        ///     Описание свойства
        /// </summary>
        [Attr]
        public string Desc { get; set; } = @"";

        /// <summary>
        ///     Подробное описание свойства
        /// </summary>
        [Attr]
        public string Details { get; set; } = @"";

        /// <summary>
        ///     Приоритет свойства
        /// </summary>
        /// <remarks>
        ///     Чем больше приоритет, тем выше свойство в списке свойств.
        /// </remarks>
        [Attr]
        public int Priority { get; set; }

        /// <summary>
        ///     Тип данных значения
        /// </summary>
        /// <remarks>
        ///     Возможные значения 'Enum','Single','Int32','Boolean','TimeSpan','DateTime'.
        /// </remarks>
        [Attr]
        public string DataType { get; set; } = @"";

        /// <summary>
        ///     Поля с мета-данными параметра
        /// </summary>
        /// <remarks>
        ///     (Url encoded name-values collection).
        /// </remarks>        
        public string MetadataFields { get; set; } = @"";

        /// <summary>
        ///     Поля с мета-данными параметра
        /// </summary>
        [Attr]
        [NotMapped]
        public CaseInsensitiveOrderedDictionary<string?> MetadataFieldsDictionary
        {
            get
            {
                return NameValueCollectionHelper.Parse(MetadataFields);
            }
            set
            {
                MetadataFields = NameValueCollectionHelper.GetNameValueCollectionString(value);
            }
        }

        public override string ToString()
        {
            return Id;
        }
    }
}


///// <summary>
/////     Связанные ParamInfo
///// </summary>
//[HasMany]
//public List<ParamInfo> ParamInfos { get; set; } = new();

///// <summary>
/////     Связанные CeMatrixParam
///// </summary>
//[HasMany]
//public List<CeMatrixParam> CeMatrixParams { get; set; } = new();

///// <summary>
/////     Связанные TagParam
///// </summary>
//[HasMany]
//public List<TagParam> TagParams { get; set; } = new();

///// <summary>
/////     Связанные ActuatorParam
///// </summary>
//[HasMany]
//public List<ActuatorParam> ActuatorParams { get; set; } = new();

///// <summary>
/////     Связанные BaseActuatorParam
///// </summary>
//[HasMany]
//public List<BaseActuatorParam> BaseActuatorParams { get; set; } = new();

///// <summary>
/////     Связанные SafetyControllerParam
///// </summary>
//[HasMany]
//public List<SafetyControllerParam> SafetyControllerParams { get; set; } = new();
