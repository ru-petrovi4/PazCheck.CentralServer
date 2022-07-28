using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     ������� � ���������� ������� ������� ���
    /// </summary>
    [Resource]
    public class CauseResult : Identifiable<int>
    {
        /// <summary>
        ///     ���������� ����� ������ �������
        /// </summary>
        [Attr]
        public int Num { get; set; } = 0;

        /// <summary>
        ///     ������� � ���������� ����������� (�� ��� ������������)
        /// </summary>
        [Attr]
        public bool IsDebug { get; set; }

        [HasMany]
        public List<SubCauseResult> SubCauseResults { get; set; } = new();

        /// <summary>
        ///     ����� ������������ �������
        /// </summary>
        [Attr]
        public DateTime? TriggeredTimeUtc { get; set; }

        /// <summary>
        ///     ������������ ��������� ������� ������� ���
        /// </summary>
        [HasOne]
        public CeMatrixResult CeMatrixResult { get; set; } = null!;        
    }
}
