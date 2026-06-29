using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Разрешение роли
    /// </summary>
    [Resource]
    public class RolePermission : Identifiable<int>
    {
        /// <summary>
        ///     Ссылка на бизнес-функцию
        /// </summary>
        [HasOne]
        public RoleBusinessFunction RoleBusinessFunction { get; set; } = null!;

        /// <summary>
        ///     Зарезервировано
        /// </summary>
        [Attr]
        public string Scope { get; set; } = @"";

        /// <summary>
        ///     Разрешено ли
        /// </summary>
        [Attr]
        public bool IsAllowed { get; set; }

        /// <summary>
        ///     Сыылка на роль пользователей
        /// </summary>
        [HasOne]
        public Role Role { get; set; } = null!;
    }
}