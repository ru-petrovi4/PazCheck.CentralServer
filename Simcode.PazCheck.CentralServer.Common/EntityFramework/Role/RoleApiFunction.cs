using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     API-функция в системе ПАЗ-Чек
    /// </summary>
    [Resource]
    [Index(nameof(Identifier), IsUnique = true)]
    public class RoleApiFunction : Identifiable<int>
    {
        /// <summary>
        ///     Идентификатор функции 
        /// </summary>
        [Attr]
        public string Identifier { get; set; } = @"";

        /// <summary>
        ///     Описание функции
        /// </summary>
        [Attr]
        public string Desc { get; set; } = @"";

        /// <summary>
        ///     Модификатор доступа     
        /// </summary>
        /// <remarks>
        ///     Если поле пустое, то доступ определяется по роли пользователя.
        ///     Если значение 'Public', то доступ есть всегда, даже незалогиненным пользователям.
        ///     Если значение 'AllRoles', то доступ есть всем ролям, единственное требование, пользователь должен быть залогинен.
        ///     Если модификатор не пустой, то не нужно отображать в веб-интерфейсе, в разделе настройки ролей.
        /// </remarks>
        [Attr]
        public string Modifier { get; set; } = @"";

        /// <summary>
        ///     Ссылки на бизнес-функции
        /// </summary>
        [HasMany]
        public List<RoleBusinessFunction> RoleBusinessFunctions { get; set; } = new();        
    }
}
