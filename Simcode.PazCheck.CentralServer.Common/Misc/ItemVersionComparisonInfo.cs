using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common
{
    public class ItemVersionComparisonInfo
    {
        /// <summary>
        ///     The entity is being tracked by the context and exists in the database. It has
        ///     been marked for deletion from the database.
        /// </summary>
        public const string ChangeType_Deleted = @"Deleted";

        /// <summary>
        ///    The entity is being tracked by the context and exists in the database. Some or
        ///    all of its property values have been modified.
        /// </summary>
        public const string ChangeType_Modified = @"Modified";

        /// <summary>
        ///     The entity is being tracked by the context but does not yet exist in the database.
        /// </summary>
        public const string ChangeType_Added = @"Added";

        public string ObjectType { get; set; } = @"";

        public int? OldObjectId { get; set; }

        public int? NewObjectId { get; set; }

        public string ChangeType { get; set; } = @"";

        /// <summary>
        ///     Родительский объект, если есть.       
        /// </summary>
        public int? ParentNewObjectId { get; set; }

        /// <summary>
        ///     Имя объекта
        /// </summary>
        public string ObjectName { get; set; } = @"";

        /// <summary>
        ///     Описание (если не помещается, можно покзывать как всплывающую подсказку)
        /// </summary>
        public string ObjectDesc { get; set; } = @"";

        public string OldValue { get; set; } = @"";

        public string NewValue { get; set; } = @"";

        /// <summary>
        ///     Свойство Source объекта. Источник (Локальная/АСУБ)
        /// </summary>
        public string ChangeSource { get; set; } = @"";

        public DateTime ChangeTimeUtc { get; set; }          

        public string ChangedBy_User { get; set; } = @"";

        public string Change_Comment { get; set; } = @"";

        public DateTime? ApproveTimeUtc { get; set; }

        public string ApprovedBy_User { get; set; } = @"";
    }
}
