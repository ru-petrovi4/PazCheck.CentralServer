using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.Serialization;

public abstract class JournalParamValue
{        
    public DateTime TimestampUtc { get; set; }        
}

public class FloatJournalParamValue : JournalParamValue
{        
    public float Value { get; set; }
}

public class FloatJournalParamValue_EqualityComparer : IEqualityComparer<FloatJournalParamValue>
{
    public static readonly FloatJournalParamValue_EqualityComparer Instance = new();

    public bool Equals(FloatJournalParamValue? leftObj, FloatJournalParamValue? rightObj)
    {
        return
            leftObj?.TimestampUtc == rightObj?.TimestampUtc &&
            leftObj?.Value == rightObj?.Value;
    }

    public int GetHashCode(FloatJournalParamValue obj)
    {
        return 0;
    }
}

//public class Int32JournalParamValue : JournalParamValue
//{
//    public int Value { get; set; }
//}

//public class StringJournalParamValue : JournalParamValue
//{
//    public string Value { get; set; } = @"";
//}