using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    //public static class JsonFieldsHelper
    //{
    //    #region public functions

    //    public static string SetDictionary(Dictionary<string, string>? value)
    //    {
    //        if (value is null || value.Count == 0)
    //        {                
    //            return @"";
    //        }
    //        try
    //        {
    //            return JsonSerializer.Serialize(value); // new JsonSerializerOptions { }
    //        }
    //        catch
    //        {
    //            return @"";
    //        }
    //    }

    //    public static Dictionary<string, string> GetDictionary(string? value)
    //    {
    //        if (String.IsNullOrEmpty(value))
    //        {
    //            return new();
    //        }
    //        try
    //        {
    //            return JsonSerializer.Deserialize<Dictionary<string, string>>(value)!;
    //        }
    //        catch
    //        {
    //            return new();
    //        }
    //    }

    //    #endregion
    //}
}
