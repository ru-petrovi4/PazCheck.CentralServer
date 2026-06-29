using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Зашифрованная именованная строка, например пароль
    /// </summary>
    [Resource]    
    public class CryptoEntity : Identifiable<int>
    {
        /// <summary>
        ///     Идентификатор строки
        /// </summary>        
        [Attr]
        public string Identifier { get; set; } = @"";

        [Attr]
        public string IdentifierLower { get; set; } = @"";

        /// <summary>
        ///     Зашифрованное значение
        /// </summary>        
        public byte[] ValueEncrypted { get; set; } = null!;

        /// <summary>
        ///     Хэш значения
        /// </summary>
        [Attr]
        public string ValueHash_Base64 { get; set; } = @"";

        /// <summary>
        ///     Комментарий
        /// </summary>
        [Attr]
        public string Comment { get; set; } = @"";

        public override string ToString()
        {
            return Identifier + ", " + Comment;
        }
    }
}
