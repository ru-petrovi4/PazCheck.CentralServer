#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Simcode.PazCheck.CentralServer.Common.Serialization
{    
    public class Unit
    {
        public string Identifier
        {
            get => _identifier;
            set => _identifier = value.Trim();
        }

        public string Title { get; set; } = @"";

        public string Desc { get; set; } = @"";

        #region private fields

        private string _identifier = @"";

        #endregion
    }
}
