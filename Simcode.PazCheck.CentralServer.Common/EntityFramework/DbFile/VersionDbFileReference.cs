using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Версионируемая ссылка на файл
    /// </summary>
    /// <remarks>
    ///     Если FileBytesHash_Base64 null, то данная сущность это папка.
    /// </remarks>
    public abstract class VersionDbFileReference : VersionedEntityBase
    {
        /// <summary>
        ///     Имя файла (с расширением) или имя папки        
        /// </summary>
        [Attr]
        public string Name { get; set; } = @"";

        /// <summary>
        ///     Принадлежность к папке (папкам), если есть      
        /// </summary>
        /// <remarks>
        ///     Пусто, если в корневой папке.
        ///     Разделитель всегда '/'. Нет '/' в начале и нет в конце.
        /// </remarks>
        [Attr]
        public string Path { get; set; } = @"";

        /// <summary>
        ///     Метки файла или пвпки
        /// </summary>
        /// <remarks>
        ///     CSV format.
        /// </remarks>
        [Attr]
        public string Tags { get; set; } = @"";

        /// <summary>
        ///     Время последнего изменения файла или папки       
        /// </summary>
        [Attr]
        public DateTime LastWriteTimeUtc { get; set; }

        /// <summary>
        ///     Размер файла или папки        
        /// </summary>
        /// <remarks>
        ///     Дублирует для оптимизации DbFile.FileBytesCount.
        /// </remarks>
        [Attr]
        public int BytesCount { get; set; }

        /// <summary>
        ///     SHA256 хэш содержимого файла в формате Base64     
        /// </summary> 
        /// <remarks>
        ///     Null для папки.
        ///     Дублирует для оптимизации DbFile.FileBytesHash_Base64.
        /// </remarks>      
        [Attr]
        public string? FileBytesHash_Base64 { get; set; }

        /// <summary>
        ///     Ссылка на файл
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(DbFileId))]
        public DbFile? DbFile { get; set; } = null!;

        /// <summary>
        ///     Ссылка на файл
        /// </summary>
        public int? DbFileId { get; set; }
    }    

    /// <summary>
    ///     Ссылка на приложенный файл матрицы ПСС
    /// </summary>
    [Resource]
    public class CeMatrixDbFileReference : VersionDbFileReference
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
    ///     Ссылка на приложенный файл тэга
    /// </summary>
    [Resource]
    public class TagDbFileReference : VersionDbFileReference
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
    ///     Ссылка на приложенный файл модели исполнительного механизма
    /// </summary>
    [Resource]
    public class BaseActuatorDbFileReference : VersionDbFileReference
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
    ///     Ссылка на приложенный файл Объект мониторинга
    /// </summary>
    [Resource]
    public class SafetyControllerDbFileReference : VersionDbFileReference
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
    ///     Ссылка на приложенный файлы легенды
    /// </summary>
    [Resource]
    public class LegendDbFileReference : VersionDbFileReference
    {
        /// <summary>
        ///     Родительская легенда
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
