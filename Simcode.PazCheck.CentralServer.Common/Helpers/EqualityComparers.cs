using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.Helpers
{
    public class StringIdentifierEqualityComparer<TStringIdentifierEntity> : IEqualityComparer<TStringIdentifierEntity>
        where TStringIdentifierEntity : IStringIdentifierEntity
    {
        public static readonly StringIdentifierEqualityComparer<TStringIdentifierEntity> Instance = new();

        public bool Equals(TStringIdentifierEntity? leftObj, TStringIdentifierEntity? rightObj)
        {
            return String.Equals(leftObj?.Identifier, rightObj?.Identifier, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(TStringIdentifierEntity obj)
        {
            return 0;
        }
    }

    public class RowOrColumnEqualityComparer : IEqualityComparer<RowOrColumnBase>
    {
        public static readonly RowOrColumnEqualityComparer Instance = new();

        public bool Equals(RowOrColumnBase? leftObj, RowOrColumnBase? rightObj)
        {
            return leftObj?.Header == rightObj?.Header &&
                leftObj?.TagCondition_SymbolToDisplay == rightObj?.TagCondition_SymbolToDisplay;
        }

        public int GetHashCode(RowOrColumnBase obj)
        {
            return 0;
        }
    }

    public class RowOrColumnValueEqualityComparer : IEqualityComparer<RowOrColumnBase>
    {
        public static readonly RowOrColumnValueEqualityComparer Instance = new();

        public bool Equals(RowOrColumnBase? leftObj, RowOrColumnBase? rightObj)
        {
            return leftObj?.Order == rightObj?.Order;
        }

        public int GetHashCode(RowOrColumnBase obj)
        {
            return 0;
        }
    }

    public class CellEqualityComparer : IEqualityComparer<Cell>
    {
        public static readonly CellEqualityComparer Instance = new();

        public bool Equals(Cell? leftObj, Cell? rightObj)
        {
            return leftObj!.Row?.Header == rightObj!.Row?.Header &&
                leftObj!.Row?.TagCondition_SymbolToDisplay == rightObj!.Row?.TagCondition_SymbolToDisplay &&
                leftObj!.Column?.Header == rightObj!.Column?.Header &&
                leftObj!.Column?.TagCondition_SymbolToDisplay == rightObj!.Column?.TagCondition_SymbolToDisplay;
        }

        public int GetHashCode(Cell obj)
        {
            return 0;
        }
    }

    public class CellValueEqualityComparer : IEqualityComparer<Cell>
    {
        public static readonly CellValueEqualityComparer Instance = new();

        public bool Equals(Cell? leftObj, Cell? rightObj)
        {
            return leftObj?.Value == rightObj?.Value;
        }

        public int GetHashCode(Cell obj)
        {
            return 0;
        }
    }

    public class ParamNameEqualityComparer<TParam> : IEqualityComparer<TParam>
        where TParam : VersionedParamBase
    {
        public static readonly ParamNameEqualityComparer<TParam> Instance = new();

        public bool Equals(TParam? leftObj, TParam? rightObj)
        {
            return String.Equals(leftObj?.ParamName, rightObj?.ParamName, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(TParam obj)
        {
            return 0;
        }
    }

    public class ParamValueEqualityComparer<TParam> : IEqualityComparer<TParam>
        where TParam : VersionedParamBase
    {
        public static readonly ParamValueEqualityComparer<TParam> Instance = new();

        public bool Equals(TParam? leftObj, TParam? rightObj)
        {
            return leftObj?.Value == rightObj?.Value;
        }

        public int GetHashCode(TParam obj)
        {
            return 0;
        }
    }

    public class TagConditionEqualityComparer : IEqualityComparer<TagCondition>
    {
        public static readonly TagConditionEqualityComparer Instance = new();

        public bool Equals(TagCondition? leftObj, TagCondition? rightObj)
        {
            return leftObj?.ConditionCategory == rightObj?.ConditionCategory &&
                leftObj?.AeCondition == rightObj?.AeCondition &&
                leftObj?.DaCondition == rightObj?.DaCondition &&
                leftObj?.SymbolToDisplay == rightObj?.SymbolToDisplay;
        }

        public int GetHashCode(TagCondition obj)
        {
            return 0;
        }
    }

    public class TagConditionValueEqualityComparer : IEqualityComparer<TagCondition>
    {
        public static readonly TagConditionValueEqualityComparer Instance = new();

        public bool Equals(TagCondition? leftObj, TagCondition? rightObj)
        {
            return leftObj?.CanBeCause == rightObj?.CanBeCause &&
                leftObj?.CanBeEffect == rightObj?.CanBeEffect;
        }

        public int GetHashCode(TagCondition obj)
        {
            return 0;
        }
    }

    public class DbFileReferenceFilePathEqualityComparer<TDbFileReference> : IEqualityComparer<TDbFileReference>
        where TDbFileReference : VersionDbFileReference
    {
        public static readonly DbFileReferenceFilePathEqualityComparer<TDbFileReference> Instance = new();

        public bool Equals(TDbFileReference? leftObj, TDbFileReference? rightObj)
        {
            return leftObj?.Path + "/" + leftObj?.Name == rightObj?.Path + "/" + rightObj?.Name;
        }

        public int GetHashCode(TDbFileReference obj)
        {
            return 0;
        }
    }

    public class DbFileReferenceFileContentEqualityComparer<TDbFileReference> : IEqualityComparer<TDbFileReference>
        where TDbFileReference : VersionDbFileReference
    {
        public static readonly DbFileReferenceFileContentEqualityComparer<TDbFileReference> Instance = new();

        public bool Equals(TDbFileReference? leftObj, TDbFileReference? rightObj)
        {
            return leftObj?.FileBytesHash_Base64 == rightObj?.FileBytesHash_Base64 && leftObj?.Tags == rightObj?.Tags;
        }

        public int GetHashCode(TDbFileReference obj)
        {
            return 0;
        }
    }
}
