using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Ssz.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Сообщение пользователю о выполнении операции
    /// </summary>
    [Resource]
    public class UserEvent : Identifiable<int>
    {
        /// <summary>
        ///     Время сообщения
        /// </summary>
        [Attr]
        public DateTime EventTimeUtc { get; set; }

        /// <summary>
        ///     Ссылка на задачу, если есть
        /// </summary>
        [Attr]
        [JsonIgnore]
        public string JobId { get; set; } = @"";

        /// <summary>
        ///     Пользователь, кому предназначено сообщение
        /// </summary>
        /// <remarks>
        ///     Если поле пустое, то сообщение предназначено всем.
        /// </remarks>
        [Attr]
        public string User { get; set; } = @"";

        /// <summary>
        ///     Тип сообщения
        /// </summary>
        /// <remarks>
        ///     2 - Information; 3 - Warning; 4 - Error.
        /// </remarks>
        [Attr]
        public int LogLevel { get; set; }

        /// <summary>
        ///     Текст сообщения
        /// </summary>
        [Attr]
        public string Message { get; set; } = @"";

        /// <summary>
        ///     Поля сообщения
        /// </summary>   
        /// <remarks>
        ///     Url encoded name-values collection.
        /// </remarks>
        [Attr]
        public string Details { get; set; } = @"";
    }
}
