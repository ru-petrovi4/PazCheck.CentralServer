using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public interface ICreateDeleteInfoEntity
    {
        int Id { get; set; }

        /// <summary>
        ///     Время создания сущности
        /// </summary>
        /// <remarks>
        ///     Заполняется автоматически при сохранении в базу.
        /// </remarks>        
        DateTime _CreateTimeUtc { get; set; }

        /// <summary>
        ///     Пользователь, создавший сущность
        /// </summary>        
        /// <remarks>
        ///     Заполняется автоматически при сохранении в базу, если есть HttpContext или задан dbContext.User.
        /// </remarks> 
        string _CreateUser { get; set; }

        /// <summary>
        ///     Время последнего изменения
        /// </summary>
        /// <remarks>
        ///     Заполняется автоматически при сохранении в базу.
        /// </remarks>        
        DateTime _LastChangeTimeUtc { get; set; }

        /// <summary>
        ///     Последний пользователь, изменивший сущность
        /// </summary>
        /// <remarks>
        ///     Заполняется автоматически при сохранении в базу, если есть HttpContext или задан dbContext.User.
        /// </remarks>         
        string _LastChangeUser { get; set; }        

        /// <summary>
        ///     Удалена ли сущность
        /// </summary>        
        bool _IsDeleted { get; set; }
    }
}