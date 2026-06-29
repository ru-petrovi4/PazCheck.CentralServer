using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public interface IStringIdentifierEntity
    {
        /// <summary>
        ///     Идентификатор, уникальный внутри версии проекта или родительского объекта
        /// </summary>
        string Identifier { get; set; }
    }
}
