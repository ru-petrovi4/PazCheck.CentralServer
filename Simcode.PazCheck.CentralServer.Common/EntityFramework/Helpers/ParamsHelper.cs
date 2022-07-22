using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public static class ParamsHelper
    {
        #region public functions

        public static TParam GetOrCreateParam<TParam>(List<TParam> paramsList, string paramName)
            where TParam : Param, new()
        {
            var result = paramsList.FirstOrDefault(p => p.ParamName == paramName);
            if (result is null)
            {
                result = new TParam() { ParamName = paramName };
                paramsList.Add(result);
            }
            return result;
        }

        #endregion
    }
}
