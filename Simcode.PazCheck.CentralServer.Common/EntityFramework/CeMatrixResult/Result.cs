using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     ��������� �������
    /// </summary>
    [Resource]
    public class Result : Identifiable<int>
    {
        /// <summary>
        ///     ����� ��������� ���������� �������
        /// </summary>
        [Attr]
        public DateTime AlalyzeTimeUtc { get; set; }

        /// <summary>
        ///     �������� ���������� (������������ ��� �������������� ������)
        /// </summary>
        [Attr]
        public string Source { get; set; } = @"";

        /// <summary>
        ///     ����������� � ����������
        /// </summary>
        [Attr]
        public string Comment { get; set; } = @"";

        /// <summary>
        ///     ������ ������������������� ���������� ���������
        /// </summary>
        [Attr]
        public DateTime BeginTimeUtc { get; set; }

        /// <summary>
        ///     ����� ������������������� ���������� ���������
        /// </summary>
        [Attr]
        public DateTime EndTimeUtc { get; set; }

        [HasMany]
        public List<CeMatrixResult> CeMatrixResults { get; set; } = new();

        [HasMany]
        public List<DbFile> DbFiles { get; set; } = new();

        /// <summary>
        ///     ������������ ���������
        /// </summary>
        [HasOne]
        public Unit Unit { get; set; } = null!;

        [HasMany]
        public List<ResultEvent> ResultEvents { get; set; } = new();
    }
}
