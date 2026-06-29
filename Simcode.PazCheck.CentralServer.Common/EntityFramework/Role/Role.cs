using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Роль пользователей
    /// </summary>
    [Resource]
    [Index(nameof(Identifier), IsUnique = true)]
    public class Role : Identifiable<int>
    {
        /// <summary>
        ///     Идентификатор роли
        /// </summary>
        /// <remarks>
        ///     Совпададаент с именем группы в AD.
        /// </remarks>
        [Attr]
        public string Identifier { get; set; } = @"";        

        /// <summary>
        ///     Описание роли
        /// </summary>
        [Attr]
        public string Desc { get; set; } = @"";

        /// <summary>
        ///     
        /// </summary>
        [HasMany]
        public List<RolePermission> RolePermissions { get; set; } = new();

        public override string ToString()
        {
            return Identifier + ", " + Desc;
        }
    }
}
