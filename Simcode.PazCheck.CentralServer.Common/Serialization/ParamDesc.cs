using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.Serialization
{
    public class ParamDesc
    {
        /// <summary>
        ///     Имя свойства
        /// </summary>        
        public string Name
        {
            get => _name;
            set => _name = value.Trim();
        }

        /// <summary>
        ///     Описание свойства
        /// </summary>        
        public string Desc { get; set; } = @"";

        /// <summary>
        ///     Развернутое описание свойства
        /// </summary>        
        public string Details { get; set; } = @"";

        /// <summary>
        ///     Приоритет свойств
        /// </summary>
        /// <remarks>
        ///     Чем больше приоритет, тем выше свойство в списке свойств.
        /// </remarks>        
        public int Priority { get; set; }

        /// <summary>
        ///     Тип данных значения
        /// </summary>
        /// <remarks>
        ///     Возможные значения 'Enum','Double','Int32','UInt32','TimeSpan','DateTime'.
        /// </remarks>
        public string DataType { get; set; } = @"";

        /// <summary>
        ///     Поля с мета-данными параметра
        /// </summary>
        /// <remarks>
        ///     (Url encoded name-values collection).
        /// </remarks>        
        public string MetadataFields { get; set; } = @"";

        #region private fields

        private string _name = @"";

        #endregion
    }
}
