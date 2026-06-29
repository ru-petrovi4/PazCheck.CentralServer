using Microsoft.Extensions.ObjectPool;
using Simcode.PazCheck.CentralServer.Common.Properties;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Simcode.PazCheck.CentralServer.Common.Serialization
{
    [TypeConverter(typeof(Param_TypeConverter))]
    [DefaultProperty(nameof(Name))]
    public class Param
    {
        /// <summary>
        ///     ParamName
        /// </summary>
        public string Name
        {
            get => _name;
            set => _name = value.Trim();
        }

        /// <summary>
        ///     Value
        /// </summary>        
        public string Value { get; set; } = @"";

        [JsonIgnore]
        public EntityFramework.ParamDesc? ParamDesc { get; set; }

        #region private fields

        private string _name = @"";

        #endregion
    }

    public class Param_TypeConverter : TypeConverter
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
                return new Param { Value = valueString };

            return base.ConvertFrom(context, culture, value);
        }

        #endregion
    }

    public class ParamPooledPolicy : IPooledObjectPolicy<Param>
    {
        public Param Create() => new Param();

        public bool Return(Param obj)
        {            
            return true; // вернуть объект в пул
        }
    }
}

