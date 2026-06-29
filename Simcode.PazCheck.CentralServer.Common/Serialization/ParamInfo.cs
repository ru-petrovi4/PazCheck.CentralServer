using JsonApiDotNetCore.Resources.Annotations;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using System;
using System.Collections.Generic;

namespace Simcode.PazCheck.CentralServer.Common.Serialization
{
    public class ParamInfo
    {
        /// <summary>
        ///     Имя свойства
        /// </summary>        
        public string Name
        {
            get => _name;
            set => _name = value.Trim();
        }

        /// <summary>
        ///     Значение свойства по умолчанию   
        /// </summary>        
        public string DefaultValue { get; set; } = @"";

        /// <summary>
        ///     Поля с мета-данными свойства.        
        /// </summary>
        /// <remarks>
        ///     (Url encoded name-values collection)        
        /// </remarks>
        public string MetadataFields { get; set; } = @"";

        #region private fields

        private string _name = @"";

        #endregion
    }
}
