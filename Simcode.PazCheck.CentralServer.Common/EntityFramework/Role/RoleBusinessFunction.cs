using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Бизнес-функция в системе ПАЗ-Чек
    /// </summary>
    [Resource]
    [Index(nameof(Identifier), IsUnique = true)]
    public class RoleBusinessFunction : Identifiable<int>
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
        ///     Ссылки на API-функции
        /// </summary>
        [HasMany]
        public List<RoleApiFunction> RoleApiFunctions { get; set; } = new();

        public override string ToString()
        {
            return Identifier + ", " + Desc;
        }
    }
}
