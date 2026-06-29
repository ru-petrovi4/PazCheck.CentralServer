using System;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     Задача
    /// </summary>
    /// <remarks>
    ///     Задача, которая выполняется системой ПАЗ-Чек.
    /// </remarks>
    [Resource]
    public class Job : Identifiable<string>
    {
        /// <summary>
        ///     Заголовок задачи
        /// </summary>
        [Attr]
        public string JobTitle { get; set; } = @"";

        /// <summary>
        ///     Пользователь, запустивший задачу
        /// </summary>
        [Attr]
        public string User { get; set; } = @"";

        /// <summary>
        ///     Процент выполнения задачи
        /// </summary>
        /// <remarks>
        ///     0 - 100.
        /// </remarks>
        [Attr]
        public uint ProgressPercent { get; set; }

        /// <summary>
        ///     Лейбл о статусе выполнения или сообщение об ошибке
        /// </summary>
        [Attr]
        public string ProgressLabel { get; set; } = @"";

        /// <summary>
        ///     Детали о статусе исполнение или детали об ошибке
        /// </summary>
        [Attr]
        public string ProgressDetail { get; set; } = @"";

        /// <summary>        
        ///     Статус выполнения задачи
        /// </summary>  
        /// <remarks>     
        ///     Cancelled - BadRequestCancelledByClient = 0x802C0000
        ///     <see cref="Ssz.Utils.StatusCodes"/>
        /// </remarks>
        [Attr]
        public uint JobStatusCode { get; set; }

        /// <summary>
        ///     Время начала задачи
        /// </summary>
        [Attr]
        public DateTime BeginTimeUtc { get; set; }

        /// <summary>
        ///     Время окончания задачи
        /// </summary>
        [Attr]
        public DateTime? EndTimeUtc { get; set; }
    }
}
