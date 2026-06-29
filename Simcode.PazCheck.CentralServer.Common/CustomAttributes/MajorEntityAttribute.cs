using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common
{
    /// <summary>
    ///     Атрибут что бы помечать ключевые сущности информационной модели. 
    ///     (Используется при генерации документации)
    /// </summary>
    public class MajorEntityAttribute : Attribute
    {
        #region construction and destruction
        
        public MajorEntityAttribute()
        {            
        }        

        #endregion        
    }
}
