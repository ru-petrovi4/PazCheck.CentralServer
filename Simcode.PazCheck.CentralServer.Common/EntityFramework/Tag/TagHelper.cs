using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public static class TagHelper
    {
        #region public functions

        public static string GetFullConditionString(string tagName, string elementName, string? mathOperator, string? value)
        {
            return tagName + @"." + GetTagConditionString(elementName, mathOperator, value);
        }

        public static string GetTagConditionString(string elementName, string? mathOperator, string? value)
        {
            if (String.IsNullOrEmpty(elementName))
                throw new InvalidOperationException();

            if (!String.IsNullOrEmpty(value))
            {                
                if (String.IsNullOrEmpty(mathOperator))
                    throw new InvalidOperationException();
                return elementName + mathOperator + value;
            }
            else
            {                
                return elementName;
            }
        }

        #endregion
    }
}
