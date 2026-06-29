using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Simcode.PazCheck.CentralServer.Common.Serialization;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Версионируемое свойство объекта
    /// </summary>    
    public abstract class VersionedParamBase : VersionedEntityBase
    {
        /// <summary>
        ///     Имя свойства        
        /// </summary>        
        [Attr]
        public string ParamName { get; set; } = @"";

        public string ParamNameLower { get; set; } = @"";

        /// <summary>
        ///     Значение свойства        
        /// </summary> 
        /// <remarks>
        ///     Возможно со знаками \n
        /// </remarks>
        [Attr]
        public string Value { get; set; } = @"";
    }

    /// <summary>
    ///     Свойство матрицы ПСС
    /// </summary>
    [Resource]    
    public class CeMatrixParam : VersionedParamBase
    {   
        /// <summary>
        ///     Родительская матрица ПСС
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(CeMatrixId))]
        public CeMatrix CeMatrix { get; set; } = null!;

        /// <summary>
        ///     Родительская матрица ПСС
        /// </summary>
        public int CeMatrixId { get; set; }

        public override ProjectVersionedEntityBase? TryGetParentProjectVersionedEntity() => CeMatrix;
        public override bool HasParentProjectVersionedEntity() => true;
        public override Type GetParentProjectVersionedEntity_PropertyType() => typeof(CeMatrix);
        public override int GetParentProjectVersionedEntity_Id() => CeMatrixId;
    }

    /// <summary>
    ///     Свойство тэга
    /// </summary>
    [Resource]    
    public class TagParam : VersionedParamBase
    {
        /// <summary>
        ///     Родительский тэг
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(TagId))]
        public Tag Tag { get; set; } = null!;

        /// <summary>
        ///     Родительский тэг
        /// </summary>
        public int TagId { get; set; }

        public override ProjectVersionedEntityBase? TryGetParentProjectVersionedEntity() => Tag;
        public override bool HasParentProjectVersionedEntity() => true;
        public override Type GetParentProjectVersionedEntity_PropertyType() => typeof(Tag);
        public override int GetParentProjectVersionedEntity_Id() => TagId;
    }

    /// <summary>
    ///     Свойство модели исполнительного механизма
    /// </summary>
    [Resource]    
    public class BaseActuatorParam : VersionedParamBase
    {
        /// <summary>
        ///     Родительская модель исполнительного механизма
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(BaseActuatorId))]
        public BaseActuator BaseActuator { get; set; } = null!;

        /// <summary>
        ///     Родительская модель исполнительного механизма
        /// </summary>
        public int BaseActuatorId { get; set; }

        public override ProjectVersionedEntityBase? TryGetParentProjectVersionedEntity() => BaseActuator;
        public override bool HasParentProjectVersionedEntity() => true;
        public override Type GetParentProjectVersionedEntity_PropertyType() => typeof(BaseActuator);
        public override int GetParentProjectVersionedEntity_Id() => BaseActuatorId;
    }    

    /// <summary>
    ///     Свойство Объект мониторинга
    /// </summary>
    [Resource]    
    public class SafetyControllerParam : VersionedParamBase
    {
        /// <summary>
        ///     Родительский Объект мониторинга
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(SafetyControllerId))]
        public SafetyController SafetyController { get; set; } = null!;

        /// <summary>
        ///     Родительский Объект мониторинга
        /// </summary>
        public int SafetyControllerId { get; set; }

        public override ProjectVersionedEntityBase? TryGetParentProjectVersionedEntity() => SafetyController;
        public override bool HasParentProjectVersionedEntity() => true;
        public override Type GetParentProjectVersionedEntity_PropertyType() => typeof(SafetyController);
        public override int GetParentProjectVersionedEntity_Id() => SafetyControllerId;
    }

    /// <summary>
    ///     Свойство Легенды
    /// </summary>
    [Resource]
    public class LegendParam : VersionedParamBase
    {
        /// <summary>
        ///     Родительский Объект мониторинга
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(LegendId))]
        public Legend Legend { get; set; } = null!;

        /// <summary>
        ///     Родительский Объект мониторинга
        /// </summary>
        public int LegendId { get; set; }

        public override ProjectVersionedEntityBase? TryGetParentProjectVersionedEntity() => Legend;
        public override bool HasParentProjectVersionedEntity() => true;
        public override Type GetParentProjectVersionedEntity_PropertyType() => typeof(Legend);
        public override int GetParentProjectVersionedEntity_Id() => LegendId;
    }
}


///// <summary>
/////     Ссылка на описание свойства, если есть
///// </summary>
//[HasOne]
//[ForeignKey(nameof(ParamName))]
//public ParamDesc? ParamDesc { get; set; }