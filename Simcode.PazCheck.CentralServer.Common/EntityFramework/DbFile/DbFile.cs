using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Файл
    /// </summary>
    /// <remarks>
    ///     Содержимое файла в сущности DbFileContent
    /// </remarks>
    [Resource]
    [Index(nameof(FileBytesHash_Base64), IsUnique = true)]
    public class DbFile : Identifiable<int>
    {
        /// <summary>
        ///     Имя первоначального файла (с расширением)
        /// </summary>
        [Attr]
        public string OriginalFileName { get; set; } = @"";

        /// <summary>
        ///     Время удаления файла
        /// </summary>
        [Attr]
        public DateTime? _DeleteTimeUtc { get; set; }

        /// <summary>
        ///     Размер файла
        /// </summary>
        [Attr]
        public int FileBytesCount { get; set; }

        /// <summary>
        ///     SHA256 хэш содержимого файла в формате Base64       
        /// </summary>
        [Attr]
        public string FileBytesHash_Base64 { get; set; } = @"";        

        /// <summary>
        ///     Ссылка на содержимое файла 
        /// </summary>
        [HasOne]
        [ForeignKey(nameof(DbFileContentId))]
        public DbFileContent? DbFileContent { get; set; }

        /// <summary>
        ///     Ссылка на содержимое файла 
        /// </summary>
        public int? DbFileContentId { get; set; }
    }
}
