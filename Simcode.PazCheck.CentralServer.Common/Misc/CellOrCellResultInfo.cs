using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common
{
    /// <summary>
    ///     Результат возращается - либо без ошибки и стоит флаг MergeWithUpperCell = true
    ///     Либо MergeWithUpperCell = false и код ошибки. И то и другое не бывает.
    /// </summary>
    public class CellOrCellResultInfo
    {
        /// <summary>
        ///     Индекс строки ячейки (0-based)
        /// </summary>
        public int RowIndex { get; set; }

        /// <summary>
        ///     Индекс столбца ячейки (0-based)
        /// </summary>
        public int ColumnIndex { get; set; }

        /// <summary>
        ///     Вертикальное объединение. Сколько строк участвуют в объединении.
        /// </summary>
        public int RowSpan { get; set; } = 1;

        /// <summary>
        ///     Горизонтальное объединение. Сколько столбцов участвуют в объединении.
        /// </summary>
        public int ColumnSpan { get; set; } = 1;

        /// <summary>
        ///     Объединять ли с вышестоящей ячейкой
        /// </summary>
        public bool MergeWithUpperCell { get; set; } = false;

        /// <summary>
        ///     Объединять ли с ячейкой слева
        /// </summary>
        public bool MergeWithLeftCell { get; set; } = false;

        /// <summary>
        ///     Показывать ли ячейку как ошибочную.
        ///     See consts in <see cref="CellInfoStatusCodes" />
        /// </summary>
        public uint StatusCode { get; set; } = CellInfoStatusCodes.OK;

        /// <summary>
        ///     Описание ошибки
        /// </summary>
        public string Comment { get; set; } = @"";

        /// <summary>
        ///    EntityFramework.Cell.Id or EntityFramework.CellResult.Id
        /// </summary>
        public int CellIdOrCellResultId { get; set; }
    }

    public static class CellInfoStatusCodes
    {
        public const uint OK = 0;

        public const uint Warning = 1;

        public const uint Error = 2;
    }
}


///// <summary>
/////     Значение, которое отображать в ячейке. Может содержать тэг br
///// </summary>
//public string ValueToShow { get; set; } = @"";