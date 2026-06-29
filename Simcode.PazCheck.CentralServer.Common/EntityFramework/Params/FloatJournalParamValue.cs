using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Ssz.Utils;
using Simcode.PazCheck.CentralServer.Common.Helpers;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    [PrimaryKey(nameof(JournalParamValuesCollectionId), nameof(TimestampUtc))]
    [Index(nameof(JournalParamValuesCollectionId), nameof(TimestampUtc), IsDescending = [false, true])]
    public class FloatJournalParamValue : Identifiable<int>
    {
        /// <summary>
        ///    Storage size optimization.
        /// </summary>
        [NotMapped]
        public override int Id { get; set; }

        /// <summary>
        ///     Время значения
        ///     The number of milliseconds that have elapsed since 1970-01-01T00:00:00.000Z.
        /// </summary>  
        [Attr]
        public long TimestampUtc { get; set; }

        [Attr]
        public float Value { get; set; }

        /// <summary>
        ///     Ссылка на JournalParamValuesCollection
        /// </summary>     
        [HasOne]
        [ForeignKey(nameof(JournalParamValuesCollectionId))]
        public JournalParamValuesCollection JournalParamValuesCollection { get; set; } = null!;

        public int JournalParamValuesCollectionId { get; set; }        

        /// <summary>
        ///     Temp object
        /// </summary>
        [NotMapped]
        public object? Obj;
    } 
}
