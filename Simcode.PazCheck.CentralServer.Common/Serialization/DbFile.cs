using System;

namespace Simcode.PazCheck.CentralServer.Common.Serialization
{
    public class DbFile
    {
        public string OriginalFileName { get; set; } = @"";

        public int FileBytesCount { get; set; }

        public string FileBytesHash_Base64 { get; set; } = @"";

        public string FileBytes_Base64 { get; set; } = @"";
    }

    public class DbFileReference
    {
        public string Name { get; set; } = @"";

        // <summary>
        ///     Принадлежность к папке (папкам), если есть      
        /// </summary>
        /// <remarks>
        ///     Пусто, если в корневой папке.
        ///     Разделитель всегда '/'. Нет '/' в начале и нет в конце.
        /// </remarks>
        public string? Path { get; set; }

        public string? Tags { get; set; }

        public DateTime LastWriteTimeUtc { get; set; }

        public int BytesCount { get; set; }

        public string? FileBytesHash_Base64 { get; set; }
    }
}
