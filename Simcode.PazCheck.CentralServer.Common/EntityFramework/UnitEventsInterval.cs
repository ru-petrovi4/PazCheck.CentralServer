using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    /// <summary>
    ///     �������� ������� ��������������� ���������
    /// </summary>
    [Resource]
    public class UnitEventsInterval : Identifiable<int>
    {
        /// <summary>
        ///     ����� �������� ���������
        /// </summary>
        [Attr]
        public DateTime LoadTimeUtc { get; set; }

        /// <summary>
        ///     �������� ��������� (��� �������� ���� ���� ��������)
        /// </summary>
        [Attr]
        public string Source { get; set; } = @"";

        /// <summary>
        ///     ����������� � ���������
        /// </summary>
        [Attr]
        public string Comment { get; set; } = @"";

        /// <summary>
        ///     ��������� ����� ���������
        /// </summary>
        [Attr]
        public DateTime BeginTimeUtc { get; set; }

        /// <summary>
        ///     ������� ����� ���������
        /// </summary>
        [Attr]
        public DateTime EndTimeUtc { get; set; }        

        /// <summary>
        ///     ������� ���������
        /// </summary>
        [HasMany]
        public List<UnitEvent> UnitEvents { get; set; } = new();

        /// <summary>
        ///     ������������ ���������
        /// </summary>
        [HasOne]
        public Unit Unit { get; set; } = null!;
    }
}
