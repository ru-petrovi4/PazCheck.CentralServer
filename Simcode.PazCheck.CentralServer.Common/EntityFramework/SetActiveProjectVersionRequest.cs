using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class SetActiveProjectVersionRequest : Identifiable<int>
    {
        /// <summary>
        ///     Версия проекта, котроая запрошена, что бы стать активной
        /// </summary>
        [HasOne]
        public ProjectVersion RequestedActiveProjectVersion { get; set; } = null!;

        /// <summary>
        ///     Создавший запрос пользователь.
        /// </summary>
        [Attr]
        public string RequestUser { get; set; } = @"";

        /// <summary>
        ///     Пользовательское сообщение запроса
        /// </summary>
        [Attr]
        public string RequestMessage { get; set; } = @"";

        /// <summary>
        ///     Подтвердивший запрос пользователь.
        /// </summary>
        [Attr]
        public string ReplyUser { get; set; } = @"";

        /// <summary>
        ///     Пользовательское сообщение от подтвердившего запрос
        /// </summary>
        [Attr]
        public string ReplyMessage { get; set; } = @"";

        /// <summary>
        ///     Статус ответа на запрос
        ///     0 - unknown
        ///     1 - approved
        ///    -1 - rejected
        /// </summary>
        [Attr]
        public int Status { get; set; }
    }
}
