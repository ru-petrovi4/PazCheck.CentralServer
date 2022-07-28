#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    [Resource]
    public class Unit : Identifiable<string>
    {
        /// <summary>
        ///     Название установки
        ///     <para>Текстовое поле RW: Название установки</para>
        /// </summary>
        [Attr]
        public string Title { get; set; } = @"";

        /// <summary>
        ///     Описание установки
        ///     <para>Текстовое поле RW: Описание установки</para>
        /// </summary>
        [Attr]
        public string Desc { get; set; } = @"";

        [HasMany]
        [InverseProperty(nameof(Project.Unit))] // Because ActiveProject property exists.
        public List<Project> Projects { get; set; } = new();

        /// <summary>
        ///     Активный проект
        ///     <para>Текстовое поле R: Активный проект</para>
        /// </summary>
        [HasOne]
        public Project? ActiveProject { get; set; } = null!;

        [HasMany]
        public List<UnitEventsInterval> UnitEventsIntervals { get; set; } = new();

        [HasMany]
        public List<Result> Results { get; set; } = new();

        [HasMany]
        public List<Section> Sections { get; set; } = new();
    }
}
