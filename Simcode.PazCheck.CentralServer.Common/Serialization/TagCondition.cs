using JsonApiDotNetCore.Resources.Annotations;
using Ssz.Utils;
using System;
using System.ComponentModel;
using System.Globalization;

namespace Simcode.PazCheck.CentralServer.Common.Serialization
{
    /// <summary>
    ///     ConditionCategory, AeCondition, DaCondition - Unique for the Tag    
    /// </summary>
    [TypeConverter(typeof(TagConditionTypeConverter))]
    public class TagCondition
    {
        /// <summary>
        ///     Категория события
        /// </summary>
        /// <remarks>
        ///     Категория события: команда на закрытие, команда на открытие, факт открытия, факт закрытия.
        ///     Возможно, с '!' в начале.
        ///     <see cref="CentralServer.Common.PazCheckConstants"/>
        /// </remarks>        
        public string ConditionCategory
        {
            get
            {
                return _conditionCategory;
            }
            set
            {
                _conditionCategory = value.Trim();
            }
        }

        /// <summary>
        ///     Строка состояния для поиска в журнале событий
        /// </summary>
        /// <remarks>
        ///     Tag.ConditionType|TripValue|Desc.        
        /// </remarks>
        public string AeCondition
        {
            get
            {
                return _aeCondition;
            }
            set
            {
                _aeCondition = value.Trim();
            }
        }

        /// <summary>
        ///     Строка состояния для получения из БДРВ
        /// </summary>
        /// <remarks>
        ///     Tag.Property=Value.        
        /// </remarks>
        public string DaCondition
        {
            get
            {
                return _daCondition;
            }
            set
            {
                _daCondition = value.Trim();
            }
        }

        /// <summary>
        ///     Может ли быть причиной в матрицах ПСС        
        /// </summary>
        public bool CanBeCause { get; set; } = true;

        /// <summary>
        ///     Может ли быть следствием в матрицах ПСС        
        /// </summary>
        public bool CanBeEffect { get; set; } = true;

        /// <summary>
        ///     Краткое имя состояния для отображения пользователю в матрицах ПСС
        /// </summary>
        /// <remarks>
        ///     null, если не определено. Пустая строка, если нужно отображать как пустой символ.
        /// </remarks>
        public string? SymbolToDisplay
        {
            get
            {
                return _symbolToDisplay;
            }
            set
            {
                _symbolToDisplay = value?.Trim();
            }
        }

        public override string ToString()
        {
            string s = CsvHelper.FormatForCsv(@":", [
                StringHelper.GetNullForEmptyString(ConditionCategory),
                StringHelper.GetNullForEmptyString(AeCondition),
                StringHelper.GetNullForEmptyString(DaCondition),
                new Any(CanBeCause).ValueAsString(false), 
                new Any(CanBeEffect).ValueAsString(false)]);
            if (SymbolToDisplay is not null)
                s = "(" + SymbolToDisplay + ")" + s;
            return s;
        }

        #region private fields

        private string _conditionCategory = @"";

        /// <summary>
        ///     Строка состояния для поиска в журнале событий
        /// </summary>
        /// <remarks>
        ///     Tag.ConditionType|TripValue|Desc.        
        /// </remarks>
        private string _aeCondition = @"";

        /// <summary>
        ///     Строка состояния для получения из БДРВ
        /// </summary>
        /// <remarks>
        ///     Tag.Property=Value.        
        /// </remarks>
        private string _daCondition = @"";

        /// <summary>
        ///     Краткое имя состояния для отображения пользователю в матрицах ПСС
        /// </summary>
        /// <remarks>
        ///     null, если не определено. Пустая строка, если нужно отображать как пустой символ.
        /// </remarks>
        private string? _symbolToDisplay;

        #endregion
    }

    public class TagConditionTypeConverter : TypeConverter
    {
        #region public functions

        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            var valueString = value as string;

            if (valueString is not null)
            {
                string? symbolToDisplay = null;
                if (valueString.StartsWith("("))
                {
                    int i = valueString.IndexOf(')');
                    if (i > 0)
                    {
                        symbolToDisplay = valueString.Substring(1, i - 1);
                        valueString = valueString.Substring(i + 1);
                    }
                }

                var parts = Ssz.Utils.CsvHelper.ParseCsvLine(":", valueString);
                if (parts.Length != 1 && parts.Length != 2 && parts.Length != 5)
                    return null;

                TagCondition tagCondition = new() { SymbolToDisplay = symbolToDisplay };
                
                if (parts.Length == 1)
                {
                    var aeCondition = parts[0] ?? @"";
                    if (aeCondition.StartsWith("!"))
                    {
                        tagCondition.ConditionCategory = @"!";
                        tagCondition.AeCondition = aeCondition.Substring(1);
                        tagCondition.DaCondition = @"";
                        tagCondition.CanBeCause = true;
                        tagCondition.CanBeEffect = true;
                    }
                    else
                    {
                        tagCondition.ConditionCategory = @"";
                        tagCondition.AeCondition = aeCondition;
                        tagCondition.DaCondition = @"";
                        tagCondition.CanBeCause = true;
                        tagCondition.CanBeEffect = true;
                    }                    
                }
                else if (parts.Length == 2)
                {
                    tagCondition.ConditionCategory = parts[0] ?? @"";
                    tagCondition.AeCondition = parts[1] ?? @"";
                    tagCondition.DaCondition = @"";
                    tagCondition.CanBeCause = true;
                    tagCondition.CanBeEffect = true;
                }
                else
                {
                    tagCondition.ConditionCategory = parts[0] ?? @"";
                    tagCondition.AeCondition = parts[1] ?? @"";
                    tagCondition.DaCondition = parts[2] ?? @"";
                    tagCondition.CanBeCause = new Any(parts[3]).ValueAsBoolean(false);
                    tagCondition.CanBeEffect = new Any(parts[4]).ValueAsBoolean(false);
                }

                return tagCondition;
            }

            return base.ConvertFrom(context, culture, value);
        }

        #endregion
    }
}
