using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.Serialization
{
    public class UnitEvent
    {
        /// <summary>
        ///     Время события
        /// </summary>        
        public DateTime EventTimeUtc { get; set; }

        /// <summary>
        ///     Имя тэга
        /// </summary>        
        public string TagName { get; set; } = @"";

        /// <summary>
        ///     Важные поля события для отображения пользователю
        /// </summary>         
        public string ConditionString { get; set; } = @"";

        /// <summary>
        ///     Состояне стало активным, неактивным или не изменилось.
        /// </summary>        
        public bool? ConditionIsActive { get; set; }

        /// <summary>
        ///     Приоритет события
        /// </summary>
        /// <remarks>
        ///     0 - Journal (J); 1 - Low (L); 2 - High (H); 3 - Urgent (U).
        /// </remarks>        
        public int Priority { get; set; }

        /// <summary>
        ///     Описание события для пользователя
        /// </summary>        
        public string Message { get; set; } = @"";

        /// <summary>
        ///     Исходное событие со всеми исходными полями
        /// </summary>
        public List<Param>? Params { get; set; }

        /// <summary>
        ///      Obsolete. For compatibility only.
        /// </summary>
        public string OriginalEventDictionary { get; set; } = @"";
    }
}
