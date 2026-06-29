using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common
{
    public class DefaultEntity_RoleBusinessFunctionsAttribute : Attribute
    {
        #region construction and destruction

        /// <summary>
        ///     RoleBusinessFunctions names for read, create, update, delete, view in UI
        /// </summary>
        /// <param name="read"></param>
        /// <param name="create"></param>
        /// <param name="update"></param>
        /// <param name="delete"></param>
        /// <param name="view"></param>
        public DefaultEntity_RoleBusinessFunctionsAttribute(string[] read, string[] create, string[] update, string[] delete, string[] view)
        {
            Read = String.Join(@",", read.Concat(new string[] { @"SuperUser" }).ToArray());
            Create = String.Join(@",", create.Concat(new string[] { @"SuperUser" }).ToArray());
            Update = String.Join(@",", update.Concat(new string[] { @"SuperUser" }).ToArray());
            Delete = String.Join(@",", delete.Concat(new string[] { @"SuperUser" }).ToArray());
            View = String.Join(@",", view.Concat(new string[] { @"SuperUser" }).ToArray());
        }

        #endregion

        #region public functions        

        /// <summary>
        ///     Comma-separated list of RoleBusinessFunctions
        /// </summary>
        public string Read { get; set; }

        /// <summary>
        ///     Comma-separated list of RoleBusinessFunctions
        /// </summary>
        public string Create { get; set; }

        /// <summary>
        ///     Comma-separated list of RoleBusinessFunctions
        /// </summary>
        public string Update { get; set; }

        /// <summary>
        ///     Comma-separated list of RoleBusinessFunctions
        /// </summary>
        public string Delete { get; set; }

        /// <summary>
        ///     Comma-separated list of RoleBusinessFunctions
        /// </summary>
        public string View { get; set; }

        #endregion
    }
}
