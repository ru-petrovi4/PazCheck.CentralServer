using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common;

//public class ActuatorCommandInfo
//{
//    public DateTime CommandTimeUtc { get; set; }

//    public string TagName { get; set; } = null!;

//    public string Command_SymbolToDisplay_Desc { get; set; } = null!;

//    public string ValueAfterExecution { get; set; } = null!;

//    /// <summary>
//    ///     <see cref="PazCheckConstants.CommandSource_Logic"/>or<see cref="PazCheckConstants.CommandSource_Journal"/>
//    /// </summary>
//    public string CommandSource { get; set; } = null!;

//    /// <summary>
//    ///     <see cref="TriggeredType"/>
//    /// </summary>
//    public int CommandExecutionType { get; set; }

//    /// <summary>
//    ///     Invariant culture
//    /// </summary>
//    public string CommandExecutionDuration { get; set; } = null!;

//    /// <summary>
//    ///     Invariant culture
//    /// </summary>
//    public string CommandExecutionMaxDuration { get; set; } = null!;

//    public List<string> CeMatrixIdentifiers { get; set; } = null!;
//}

//public class ActuatorCommandInfo_EqualityComparer : IEqualityComparer<ActuatorCommandInfo>
//{
//    public static ActuatorCommandInfo_EqualityComparer Instance { get; } = new();

//    public bool Equals(ActuatorCommandInfo? x, ActuatorCommandInfo? y)
//    {
//        if (ReferenceEquals(x, y))
//            return true;
//        if (x is null || y is null)
//            return false;
//        return x.CommandTimeUtc == y.CommandTimeUtc &&
//            x.TagName == y.TagName && 
//            x.Command_SymbolToDisplay_Desc == y.Command_SymbolToDisplay_Desc && 
//            x.CommandSource == y.CommandSource;
//    }

//    public int GetHashCode([DisallowNull] ActuatorCommandInfo obj)
//    {
//        return 0;
//    }
//}
