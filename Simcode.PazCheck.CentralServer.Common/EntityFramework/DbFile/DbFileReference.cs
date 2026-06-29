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
    ///     Ссылка на файл
    /// </summary>
    /// <remarks>
    ///     Если FileBytesHash_Base64 null, то данная сущность это папка.
    /// </remarks>
    public abstract class DbFileReference : Identifiable<int>
    {
        /// <summary>
        ///     Пользователь, создавший ссылку на файл
        /// </summary>
        [Attr]
        public string _CreateUser { get; set; } = @"";

        /// <summary>
        ///     Время создания ссылки на файл
        ///     Заполняется автоматически
        /// </summary>
        [Attr]
        public DateTime _CreateTimeUtc { get; set; } = DateTime.UtcNow;

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
        ///     Метки файла или папки
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
    ///     Ссылка на приложенный файл версии проекта
    /// </summary>
    [Resource]
    public class ProjectVersionDbFileReference : DbFileReference
    {
        /// <summary>
        ///     Родительская версия проекта
        /// </summary>
        [HasOne]
        public ProjectVersion ProjectVersion { get; set; } = null!;
    }    

    /// <summary>
    ///     Ссылка на приложенный файл шаблона объекта модуля 'Мониторинг'
    /// </summary>
    [Resource]
    public class BasePcObjectDbFileReference : DbFileReference
    {
        /// <summary>
        ///     Родительскай шаблон объекта модуля 'Мониторинг'
        /// </summary>
        [HasOne]
        public BasePcObject BasePcObject { get; set; } = null!;
    }

    /// <summary>
    ///     Ссылка на приложенный файл объекта модуля 'Мониторинг'
    /// </summary>
    [Resource]
    public class PcObjectDbFileReference : DbFileReference
    {
        /// <summary>
        ///     Родительский объект модуля 'Мониторинг'
        /// </summary>
        [HasOne]
        public PcObject PcObject { get; set; } = null!;
    }

    /// <summary>
    ///     Ссылка на приложенный файл события объекта модуля 'Мониторинг'
    /// </summary>
    [Resource]
    public class PcObjectEventDbFileReference : DbFileReference
    {
        /// <summary>
        ///     Родительское событие объекта модуля 'Мониторинг'
        /// </summary>
        [HasOne]
        public PcObjectEvent PcObjectEvent { get; set; } = null!;
    }
}
