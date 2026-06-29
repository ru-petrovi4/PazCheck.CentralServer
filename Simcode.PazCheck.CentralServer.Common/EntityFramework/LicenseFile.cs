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
    ///     Файл лицензии
    /// </summary>
    [Resource]
    public class LicenseFile : Identifiable<int>
    {
        /// <summary>
        ///     Пользователь, загрузивший файл лицензии
        /// </summary>
        [Attr]
        public string _CreateUser { get; set; } = @"";

        /// <summary>
        ///     Время загрузки файла лицензий
        /// </summary>
        /// <remarks>
        ///     Заполняется автоматичски.
        /// </remarks>
        [Attr]
        public DateTime _CreateTimeUtc { get; set; } = DateTime.UtcNow;

        /// <summary> 
        ///     Описание, комментарии файла лицензии      
        /// </summary>
        [Attr]
        public string Comments { get; set; } = @"";

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

        public override string ToString()
        {
            return Comments;
        }
    }
}
