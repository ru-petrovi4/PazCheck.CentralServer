using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public interface ILastChangeEntity
    {
        string _LastChangeUser { get; set; }

        /// <summary>
        ///     Updates only if entity or child entity was added or marked for deletion
        /// </summary>
        DateTime _LastChangeTimeUtc { get; set; }

        public bool _IsDeleted { get; set; }

        /// <summary>
        ///     Parent for _LastChangeTimeUtc updating.
        /// </summary>
        /// <returns></returns>
        ILastChangeEntity? GetParentForLastChange();
    }

    public static class LastChangeEntityExtensions
    {
        #region public functions

        /// <summary>
        ///     Updates parent _LastChangeUser and _LastChangeTimeUtc recursively.
        ///     Does not change _LastChangeUser and _LastChangeTimeUtc for current entity.
        /// </summary>
        /// <param name="lastChangeEntity"></param>
        public static void UpdateParentLastChange(this ILastChangeEntity lastChangeEntity)
        {
            ILastChangeEntity? parent = lastChangeEntity;
            while ((parent = parent.GetParentForLastChange()) != null)
            {
                parent._LastChangeUser = lastChangeEntity._LastChangeUser;
                parent._LastChangeTimeUtc = lastChangeEntity._LastChangeTimeUtc;
            }
        }

        #endregion
    }
}
