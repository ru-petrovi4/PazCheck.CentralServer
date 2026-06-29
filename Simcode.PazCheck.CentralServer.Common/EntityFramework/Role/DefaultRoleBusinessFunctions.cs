using Simcode.PazCheck.CentralServer.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public static class DefaultRoleBusinessFunctions
    {
        public const string Public_RoleApiFunction_Modifier = @"Public";

        public const string AllRoles_RoleApiFunction_Modifier = @"AllRoles";

        /// <summary>
        ///     Has access to all read        
        /// </summary>
        [DefaultRoleBusinessFunction_Roles(
            PazCheckDbHelper.PazCheckAdmins + "," + PazCheckDbHelper.PazCheckEngineers + "," + PazCheckDbHelper.PazCheckSupervisors + "," + PazCheckDbHelper.PazCheckObservers
            )]
        public const string View = @"Просмотр, формирование отчетности";

        /// <summary>
        ///     Has long-running access to widgets
        /// </summary>
        [DefaultRoleBusinessFunction_Roles(
            PazCheckDbHelper.WidgetsLongRunningViewers
            )]
        public const string WidgetsLongRunningView = @"Долговременный просмотр приборных панелей";

        /// <summary>
        ///     Has access to all write        
        /// </summary>
        [DefaultRoleBusinessFunction_Roles(
            PazCheckDbHelper.PazCheckAdmins
            )]
        public const string Administrating = @"Администрирование";

        [DefaultRoleBusinessFunction_Roles(
            PazCheckDbHelper.PazCheckAdmins + "," + PazCheckDbHelper.PazCheckEngineers
            )]
        public const string Projects_Edit = @"Изменение проектов справочника элементов ПАЗ";

        [DefaultRoleBusinessFunction_Roles(
            PazCheckDbHelper.PazCheckAdmins + "," + PazCheckDbHelper.PazCheckSupervisors
            )]
        public const string Projects_Supervise = @"Авторизация изменений в проектах справочника элементов ПАЗ";

        [DefaultRoleBusinessFunction_Roles(
            PazCheckDbHelper.PazCheckAdmins + "," + PazCheckDbHelper.PazCheckEngineers
            )]
        public const string Diagnost_EditUnitEvents = @"Загрузка логов технологшических событий";        

        [DefaultRoleBusinessFunction_Roles(
            PazCheckDbHelper.PazCheckAdmins + "," + PazCheckDbHelper.PazCheckEngineers
            )]
        public const string Diagnost_EditCalculationResults = @"Редактирование реестра результатов анализа остановов";

        [DefaultRoleBusinessFunction_Roles(
            PazCheckDbHelper.PazCheckAdmins + "," + PazCheckDbHelper.PazCheckEngineers
            )]
        public const string Diagnost_Calculate = @"Анализ прошедших остановов";           

        [DefaultRoleBusinessFunction_Roles(
            PazCheckDbHelper.PazCheckAdmins + "," + PazCheckDbHelper.PazCheckEngineers
            )]
        public const string Monitoring_Edit = @"Модуль Мониторинг: добавлние/редактирование данных об объектах, комментирование, загрузка сопутствующих документов";

        [DefaultRoleBusinessFunction_Roles(
            PazCheckDbHelper.PazCheckIsAdmins
            )]
        public const string InformationSecurityEvents_View = @"Просмотр логов событий ИБ";

        [DefaultRoleBusinessFunction_Roles(
            PazCheckDbHelper.PazCheckAdmins + "," + PazCheckDbHelper.PazCheckIsAdmins
            )]
        public const string Roles_View = @"Просмотр настроек ролей пользователей";

        [DefaultRoleBusinessFunction_Roles(
            PazCheckDbHelper.PazCheckAdmins
            )]
        public const string Roles_Edit = @"Настройка ролей пользователей";
    }
}