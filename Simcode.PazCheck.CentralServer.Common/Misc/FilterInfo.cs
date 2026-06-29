using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common
{
    public class FilterInfo
    {
        /// <summary>
        ///     Отображается в выпадающем списке "Поиск по".
        /// </summary>
        public string[]? SearchBy_List { get; set; }

        /// <summary>
        ///     Отображается в выпадающем списке "Поиск по".
        /// </summary>
        public string[]? SearchBy_DescList { get; set; }

        /// <summary>
        ///     Набор возможных дополнительных критериев, отображаются в голубой области.
        ///     Что бы добавить критерий, нужно нажать кнопку плюс.
        /// </summary>
        public CriterionInfo[]? CriterionInfosList { get; set; }

        /// <summary>
        ///     Стандартный набор предустановленных фильтров
        /// </summary>
        public Filter[]? StandardFiltersList { get; set; }

        /// <summary>
        ///     Имена фильтров из стандартного набора
        /// </summary>
        public string[]? StandardFilterNamesList { get; set; }
    }

    public class CriterionInfo
    {
        public bool IsMultiCriterion { get; set; } = true;

        public string CriterionName { get; set; } = null!;

        public string CriterionDesc { get; set; } = null!;

        public bool IsRequiredParamName { get; set; }

        public string[]? PriorityParamNamesList { get; set; }

        public string[]? OperatorsList { get; set; }

        public string[]? OperatorDescsList { get; set; }

        public string[]? OptionsList { get; set; }

        public string[]? OptionDescsList { get; set; }

        public bool IsMultiValue { get; set; }

        public string? ValueType { get; set; }

        public string[]? ValuesList { get; set; }

        public string[]? ValueDescsList { get; set; }
    }
}
