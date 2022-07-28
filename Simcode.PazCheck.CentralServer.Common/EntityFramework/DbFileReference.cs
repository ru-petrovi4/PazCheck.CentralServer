using JsonApiDotNetCore.Resources.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{     
    public abstract class DbFileReference : VersionEntityBase
    {
        /// <summary>
        ///     Имя файла
        /// </summary>
        [Attr]
        public string FileName { get; set; } = @"";

        /// <summary>
        ///     Ссылка на данные файла 
        /// </summary>
        [HasOne]
        public DbFile DbFile { get; set; } = null!;
        
        public int DbFileId { get; set; }
    }

    /// <summary>
    ///     Ссылка на приложенный файл матрицы
    /// </summary>
    public class CeMatrixDbFileReference : DbFileReference
    {
        /// <summary>
        ///     Родительская матрица
        /// </summary>
        [HasOne]
        public CeMatrix CeMatrix { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => CeMatrix;
    }

    /// <summary>
    ///     Ссылка на приложенный файл модели механизма
    /// </summary>
    public class BaseActuatorDbFileReference : DbFileReference
    {
        /// <summary>
        ///     Родительская модель механизма
        /// </summary>
        [HasOne]
        public BaseActuator BaseActuator { get; set; } = null!;

        public override ILastChangeEntity? GetParentForLastChange() => BaseActuator;
    }
}
