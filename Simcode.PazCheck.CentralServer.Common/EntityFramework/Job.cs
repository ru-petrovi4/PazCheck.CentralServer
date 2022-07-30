using System;
using System.Threading.Tasks;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class Job : Identifiable<string>
    {
        /// <summary>
        ///     ��������� ������
        /// </summary>
        [Attr]
        public string JobTitle { get; set; } = @"";

        /// <summary>
        ///     ������� ���������� ������ 0 - 100
        /// </summary>
        [Attr]
        public double ProgressPercent { get; set; }

        /// <summary>
        ///     ����� � ������� ���������� ��� ��������� �� ������
        /// </summary>
        [Attr]
        public string ProgressLabel { get; set; } = @"";

        /// <summary>
        ///     ������� � ������� ���������� ��� ������ �� ������
        /// </summary>
        [Attr]
        public string ProgressDetail { get; set; } = @"";

        /// <summary>
        ///     OK = 0, Cancelled = 1, Error >= 2.
        ///     For details, see Grpc.Core.StatusCode        
        /// </summary>        
        [Attr]
        public uint StatusCode { get; set; }

        /// <summary>
        ///     ����� ��������� ������, ������� ��� � �������.
        /// </summary>
        [Attr]
        public DateTime? FinishedTimeUtc { get; set; }
    }
}
