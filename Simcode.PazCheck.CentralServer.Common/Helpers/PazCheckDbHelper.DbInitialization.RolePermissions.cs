using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Simcode.PazCheck.CentralServer.Common;
using Simcode.PazCheck.CentralServer.Common.EntityFramework;
using Simcode.PazCheck.CentralServer.Common.Serialization;
using Ssz.Utils;
using Ssz.Utils.Addons;

namespace Simcode.PazCheck.CentralServer.Common.Helpers
{
    public static partial class PazCheckDbHelper
    {
        #region private functions

        /// <summary>
        ///     Saves changes.
        /// </summary>
        /// <param name="dbContext"></param>
        private static void InitializeDb_RolePermissions(PazCheckDbContext dbContext)
        {
            #region GeneratedCode            

            var roleApiFunction_Entity_Unit_Read = new RoleApiFunction
            {
                Identifier = "Entity.Unit.Read",
                Desc = "Unit Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Unit_Read);

            var roleApiFunction_Entity_Unit_Create = new RoleApiFunction
            {
                Identifier = "Entity.Unit.Create",
                Desc = "Unit Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Unit_Create);

            var roleApiFunction_Entity_Unit_Update = new RoleApiFunction
            {
                Identifier = "Entity.Unit.Update",
                Desc = "Unit Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Unit_Update);

            var roleApiFunction_Entity_Unit_Delete = new RoleApiFunction
            {
                Identifier = "Entity.Unit.Delete",
                Desc = "Unit Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Unit_Delete);

            var roleApiFunction_Entity_Unit_View = new RoleApiFunction
            {
                Identifier = "Entity.Unit.View",
                Desc = "Unit Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Unit_View);
            

            var roleApiFunction_Entity_Project_Read = new RoleApiFunction
            {
                Identifier = "Entity.Project.Read",
                Desc = "Project Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Project_Read);

            var roleApiFunction_Entity_Project_Create = new RoleApiFunction
            {
                Identifier = "Entity.Project.Create",
                Desc = "Project Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Project_Create);

            var roleApiFunction_Entity_Project_Update = new RoleApiFunction
            {
                Identifier = "Entity.Project.Update",
                Desc = "Project Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Project_Update);

            var roleApiFunction_Entity_Project_Delete = new RoleApiFunction
            {
                Identifier = "Entity.Project.Delete",
                Desc = "Project Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Project_Delete);

            var roleApiFunction_Entity_Project_View = new RoleApiFunction
            {
                Identifier = "Entity.Project.View",
                Desc = "Project Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Project_View);
            

            var roleApiFunction_Entity_ProjectVersion_Read = new RoleApiFunction
            {
                Identifier = "Entity.ProjectVersion.Read",
                Desc = "ProjectVersion Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersion_Read);

            var roleApiFunction_Entity_ProjectVersion_Create = new RoleApiFunction
            {
                Identifier = "Entity.ProjectVersion.Create",
                Desc = "ProjectVersion Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersion_Create);

            var roleApiFunction_Entity_ProjectVersion_Update = new RoleApiFunction
            {
                Identifier = "Entity.ProjectVersion.Update",
                Desc = "ProjectVersion Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersion_Update);

            var roleApiFunction_Entity_ProjectVersion_Delete = new RoleApiFunction
            {
                Identifier = "Entity.ProjectVersion.Delete",
                Desc = "ProjectVersion Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersion_Delete);

            var roleApiFunction_Entity_ProjectVersion_View = new RoleApiFunction
            {
                Identifier = "Entity.ProjectVersion.View",
                Desc = "ProjectVersion Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersion_View);
            

            var roleApiFunction_Entity_ProjectVersionType_Read = new RoleApiFunction
            {
                Identifier = "Entity.ProjectVersionType.Read",
                Desc = "ProjectVersionType Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionType_Read);

            var roleApiFunction_Entity_ProjectVersionType_Create = new RoleApiFunction
            {
                Identifier = "Entity.ProjectVersionType.Create",
                Desc = "ProjectVersionType Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionType_Create);

            var roleApiFunction_Entity_ProjectVersionType_Update = new RoleApiFunction
            {
                Identifier = "Entity.ProjectVersionType.Update",
                Desc = "ProjectVersionType Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionType_Update);

            var roleApiFunction_Entity_ProjectVersionType_Delete = new RoleApiFunction
            {
                Identifier = "Entity.ProjectVersionType.Delete",
                Desc = "ProjectVersionType Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionType_Delete);

            var roleApiFunction_Entity_ProjectVersionType_View = new RoleApiFunction
            {
                Identifier = "Entity.ProjectVersionType.View",
                Desc = "ProjectVersionType Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionType_View);
            

            var roleApiFunction_Entity_ProjectVersionDbFileReference_Read = new RoleApiFunction
            {
                Identifier = "Entity.ProjectVersionDbFileReference.Read",
                Desc = "ProjectVersionDbFileReference Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionDbFileReference_Read);

            var roleApiFunction_Entity_ProjectVersionDbFileReference_Create = new RoleApiFunction
            {
                Identifier = "Entity.ProjectVersionDbFileReference.Create",
                Desc = "ProjectVersionDbFileReference Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionDbFileReference_Create);

            var roleApiFunction_Entity_ProjectVersionDbFileReference_Update = new RoleApiFunction
            {
                Identifier = "Entity.ProjectVersionDbFileReference.Update",
                Desc = "ProjectVersionDbFileReference Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionDbFileReference_Update);

            var roleApiFunction_Entity_ProjectVersionDbFileReference_Delete = new RoleApiFunction
            {
                Identifier = "Entity.ProjectVersionDbFileReference.Delete",
                Desc = "ProjectVersionDbFileReference Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionDbFileReference_Delete);

            var roleApiFunction_Entity_ProjectVersionDbFileReference_View = new RoleApiFunction
            {
                Identifier = "Entity.ProjectVersionDbFileReference.View",
                Desc = "ProjectVersionDbFileReference Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionDbFileReference_View);
            

            var roleApiFunction_Entity_CeMatrix_Read = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrix.Read",
                Desc = "CeMatrix Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrix_Read);

            var roleApiFunction_Entity_CeMatrix_Create = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrix.Create",
                Desc = "CeMatrix Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrix_Create);

            var roleApiFunction_Entity_CeMatrix_Update = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrix.Update",
                Desc = "CeMatrix Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrix_Update);

            var roleApiFunction_Entity_CeMatrix_Delete = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrix.Delete",
                Desc = "CeMatrix Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrix_Delete);

            var roleApiFunction_Entity_CeMatrix_View = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrix.View",
                Desc = "CeMatrix Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrix_View);
            

            var roleApiFunction_Entity_CeMatrixParam_Read = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrixParam.Read",
                Desc = "CeMatrixParam Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixParam_Read);

            var roleApiFunction_Entity_CeMatrixParam_Create = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrixParam.Create",
                Desc = "CeMatrixParam Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixParam_Create);

            var roleApiFunction_Entity_CeMatrixParam_Update = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrixParam.Update",
                Desc = "CeMatrixParam Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixParam_Update);

            var roleApiFunction_Entity_CeMatrixParam_Delete = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrixParam.Delete",
                Desc = "CeMatrixParam Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixParam_Delete);

            var roleApiFunction_Entity_CeMatrixParam_View = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrixParam.View",
                Desc = "CeMatrixParam Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixParam_View);
            

            var roleApiFunction_Entity_CeMatrixDbFileReference_Read = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrixDbFileReference.Read",
                Desc = "CeMatrixDbFileReference Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixDbFileReference_Read);

            var roleApiFunction_Entity_CeMatrixDbFileReference_Create = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrixDbFileReference.Create",
                Desc = "CeMatrixDbFileReference Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixDbFileReference_Create);

            var roleApiFunction_Entity_CeMatrixDbFileReference_Update = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrixDbFileReference.Update",
                Desc = "CeMatrixDbFileReference Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixDbFileReference_Update);

            var roleApiFunction_Entity_CeMatrixDbFileReference_Delete = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrixDbFileReference.Delete",
                Desc = "CeMatrixDbFileReference Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixDbFileReference_Delete);

            var roleApiFunction_Entity_CeMatrixDbFileReference_View = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrixDbFileReference.View",
                Desc = "CeMatrixDbFileReference Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixDbFileReference_View);
            

            var roleApiFunction_Entity_Row_Read = new RoleApiFunction
            {
                Identifier = "Entity.Row.Read",
                Desc = "Row Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Row_Read);

            var roleApiFunction_Entity_Row_Create = new RoleApiFunction
            {
                Identifier = "Entity.Row.Create",
                Desc = "Row Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Row_Create);

            var roleApiFunction_Entity_Row_Update = new RoleApiFunction
            {
                Identifier = "Entity.Row.Update",
                Desc = "Row Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Row_Update);

            var roleApiFunction_Entity_Row_Delete = new RoleApiFunction
            {
                Identifier = "Entity.Row.Delete",
                Desc = "Row Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Row_Delete);

            var roleApiFunction_Entity_Row_View = new RoleApiFunction
            {
                Identifier = "Entity.Row.View",
                Desc = "Row Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Row_View);
            

            var roleApiFunction_Entity_Column_Read = new RoleApiFunction
            {
                Identifier = "Entity.Column.Read",
                Desc = "Column Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Column_Read);

            var roleApiFunction_Entity_Column_Create = new RoleApiFunction
            {
                Identifier = "Entity.Column.Create",
                Desc = "Column Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Column_Create);

            var roleApiFunction_Entity_Column_Update = new RoleApiFunction
            {
                Identifier = "Entity.Column.Update",
                Desc = "Column Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Column_Update);

            var roleApiFunction_Entity_Column_Delete = new RoleApiFunction
            {
                Identifier = "Entity.Column.Delete",
                Desc = "Column Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Column_Delete);

            var roleApiFunction_Entity_Column_View = new RoleApiFunction
            {
                Identifier = "Entity.Column.View",
                Desc = "Column Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Column_View);
            

            var roleApiFunction_Entity_Cell_Read = new RoleApiFunction
            {
                Identifier = "Entity.Cell.Read",
                Desc = "Cell Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Cell_Read);

            var roleApiFunction_Entity_Cell_Create = new RoleApiFunction
            {
                Identifier = "Entity.Cell.Create",
                Desc = "Cell Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Cell_Create);

            var roleApiFunction_Entity_Cell_Update = new RoleApiFunction
            {
                Identifier = "Entity.Cell.Update",
                Desc = "Cell Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Cell_Update);

            var roleApiFunction_Entity_Cell_Delete = new RoleApiFunction
            {
                Identifier = "Entity.Cell.Delete",
                Desc = "Cell Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Cell_Delete);

            var roleApiFunction_Entity_Cell_View = new RoleApiFunction
            {
                Identifier = "Entity.Cell.View",
                Desc = "Cell Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Cell_View);
            

            var roleApiFunction_Entity_CeMatrixComment_Read = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrixComment.Read",
                Desc = "CeMatrixComment Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixComment_Read);

            var roleApiFunction_Entity_CeMatrixComment_Create = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrixComment.Create",
                Desc = "CeMatrixComment Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixComment_Create);

            var roleApiFunction_Entity_CeMatrixComment_Update = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrixComment.Update",
                Desc = "CeMatrixComment Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixComment_Update);

            var roleApiFunction_Entity_CeMatrixComment_Delete = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrixComment.Delete",
                Desc = "CeMatrixComment Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixComment_Delete);

            var roleApiFunction_Entity_CeMatrixComment_View = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrixComment.View",
                Desc = "CeMatrixComment Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixComment_View);
            

            var roleApiFunction_Entity_Tag_Read = new RoleApiFunction
            {
                Identifier = "Entity.Tag.Read",
                Desc = "Tag Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Tag_Read);

            var roleApiFunction_Entity_Tag_Create = new RoleApiFunction
            {
                Identifier = "Entity.Tag.Create",
                Desc = "Tag Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Tag_Create);

            var roleApiFunction_Entity_Tag_Update = new RoleApiFunction
            {
                Identifier = "Entity.Tag.Update",
                Desc = "Tag Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Tag_Update);

            var roleApiFunction_Entity_Tag_Delete = new RoleApiFunction
            {
                Identifier = "Entity.Tag.Delete",
                Desc = "Tag Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Tag_Delete);

            var roleApiFunction_Entity_Tag_View = new RoleApiFunction
            {
                Identifier = "Entity.Tag.View",
                Desc = "Tag Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Tag_View);
            

            var roleApiFunction_Entity_TagConditionInfo_Read = new RoleApiFunction
            {
                Identifier = "Entity.TagConditionInfo.Read",
                Desc = "TagConditionInfo Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_TagConditionInfo_Read);

            var roleApiFunction_Entity_TagConditionInfo_Create = new RoleApiFunction
            {
                Identifier = "Entity.TagConditionInfo.Create",
                Desc = "TagConditionInfo Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_TagConditionInfo_Create);

            var roleApiFunction_Entity_TagConditionInfo_Update = new RoleApiFunction
            {
                Identifier = "Entity.TagConditionInfo.Update",
                Desc = "TagConditionInfo Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_TagConditionInfo_Update);

            var roleApiFunction_Entity_TagConditionInfo_Delete = new RoleApiFunction
            {
                Identifier = "Entity.TagConditionInfo.Delete",
                Desc = "TagConditionInfo Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_TagConditionInfo_Delete);

            var roleApiFunction_Entity_TagConditionInfo_View = new RoleApiFunction
            {
                Identifier = "Entity.TagConditionInfo.View",
                Desc = "TagConditionInfo Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_TagConditionInfo_View);
            

            var roleApiFunction_Entity_TagParam_Read = new RoleApiFunction
            {
                Identifier = "Entity.TagParam.Read",
                Desc = "TagParam Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_TagParam_Read);

            var roleApiFunction_Entity_TagParam_Create = new RoleApiFunction
            {
                Identifier = "Entity.TagParam.Create",
                Desc = "TagParam Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_TagParam_Create);

            var roleApiFunction_Entity_TagParam_Update = new RoleApiFunction
            {
                Identifier = "Entity.TagParam.Update",
                Desc = "TagParam Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_TagParam_Update);

            var roleApiFunction_Entity_TagParam_Delete = new RoleApiFunction
            {
                Identifier = "Entity.TagParam.Delete",
                Desc = "TagParam Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_TagParam_Delete);

            var roleApiFunction_Entity_TagParam_View = new RoleApiFunction
            {
                Identifier = "Entity.TagParam.View",
                Desc = "TagParam Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_TagParam_View);
            

            var roleApiFunction_Entity_TagCondition_Read = new RoleApiFunction
            {
                Identifier = "Entity.TagCondition.Read",
                Desc = "TagCondition Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_TagCondition_Read);

            var roleApiFunction_Entity_TagCondition_Create = new RoleApiFunction
            {
                Identifier = "Entity.TagCondition.Create",
                Desc = "TagCondition Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_TagCondition_Create);

            var roleApiFunction_Entity_TagCondition_Update = new RoleApiFunction
            {
                Identifier = "Entity.TagCondition.Update",
                Desc = "TagCondition Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_TagCondition_Update);

            var roleApiFunction_Entity_TagCondition_Delete = new RoleApiFunction
            {
                Identifier = "Entity.TagCondition.Delete",
                Desc = "TagCondition Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_TagCondition_Delete);

            var roleApiFunction_Entity_TagCondition_View = new RoleApiFunction
            {
                Identifier = "Entity.TagCondition.View",
                Desc = "TagCondition Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_TagCondition_View);
            

            var roleApiFunction_Entity_TagDbFileReference_Read = new RoleApiFunction
            {
                Identifier = "Entity.TagDbFileReference.Read",
                Desc = "TagDbFileReference Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_TagDbFileReference_Read);

            var roleApiFunction_Entity_TagDbFileReference_Create = new RoleApiFunction
            {
                Identifier = "Entity.TagDbFileReference.Create",
                Desc = "TagDbFileReference Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_TagDbFileReference_Create);

            var roleApiFunction_Entity_TagDbFileReference_Update = new RoleApiFunction
            {
                Identifier = "Entity.TagDbFileReference.Update",
                Desc = "TagDbFileReference Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_TagDbFileReference_Update);

            var roleApiFunction_Entity_TagDbFileReference_Delete = new RoleApiFunction
            {
                Identifier = "Entity.TagDbFileReference.Delete",
                Desc = "TagDbFileReference Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_TagDbFileReference_Delete);

            var roleApiFunction_Entity_TagDbFileReference_View = new RoleApiFunction
            {
                Identifier = "Entity.TagDbFileReference.View",
                Desc = "TagDbFileReference Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_TagDbFileReference_View);
            

            var roleApiFunction_Entity_BaseActuator_Read = new RoleApiFunction
            {
                Identifier = "Entity.BaseActuator.Read",
                Desc = "BaseActuator Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuator_Read);

            var roleApiFunction_Entity_BaseActuator_Create = new RoleApiFunction
            {
                Identifier = "Entity.BaseActuator.Create",
                Desc = "BaseActuator Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuator_Create);

            var roleApiFunction_Entity_BaseActuator_Update = new RoleApiFunction
            {
                Identifier = "Entity.BaseActuator.Update",
                Desc = "BaseActuator Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuator_Update);

            var roleApiFunction_Entity_BaseActuator_Delete = new RoleApiFunction
            {
                Identifier = "Entity.BaseActuator.Delete",
                Desc = "BaseActuator Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuator_Delete);

            var roleApiFunction_Entity_BaseActuator_View = new RoleApiFunction
            {
                Identifier = "Entity.BaseActuator.View",
                Desc = "BaseActuator Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuator_View);
            

            var roleApiFunction_Entity_BaseActuatorParam_Read = new RoleApiFunction
            {
                Identifier = "Entity.BaseActuatorParam.Read",
                Desc = "BaseActuatorParam Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorParam_Read);

            var roleApiFunction_Entity_BaseActuatorParam_Create = new RoleApiFunction
            {
                Identifier = "Entity.BaseActuatorParam.Create",
                Desc = "BaseActuatorParam Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorParam_Create);

            var roleApiFunction_Entity_BaseActuatorParam_Update = new RoleApiFunction
            {
                Identifier = "Entity.BaseActuatorParam.Update",
                Desc = "BaseActuatorParam Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorParam_Update);

            var roleApiFunction_Entity_BaseActuatorParam_Delete = new RoleApiFunction
            {
                Identifier = "Entity.BaseActuatorParam.Delete",
                Desc = "BaseActuatorParam Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorParam_Delete);

            var roleApiFunction_Entity_BaseActuatorParam_View = new RoleApiFunction
            {
                Identifier = "Entity.BaseActuatorParam.View",
                Desc = "BaseActuatorParam Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorParam_View);
            

            var roleApiFunction_Entity_BaseActuatorDbFileReference_Read = new RoleApiFunction
            {
                Identifier = "Entity.BaseActuatorDbFileReference.Read",
                Desc = "BaseActuatorDbFileReference Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorDbFileReference_Read);

            var roleApiFunction_Entity_BaseActuatorDbFileReference_Create = new RoleApiFunction
            {
                Identifier = "Entity.BaseActuatorDbFileReference.Create",
                Desc = "BaseActuatorDbFileReference Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorDbFileReference_Create);

            var roleApiFunction_Entity_BaseActuatorDbFileReference_Update = new RoleApiFunction
            {
                Identifier = "Entity.BaseActuatorDbFileReference.Update",
                Desc = "BaseActuatorDbFileReference Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorDbFileReference_Update);

            var roleApiFunction_Entity_BaseActuatorDbFileReference_Delete = new RoleApiFunction
            {
                Identifier = "Entity.BaseActuatorDbFileReference.Delete",
                Desc = "BaseActuatorDbFileReference Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorDbFileReference_Delete);

            var roleApiFunction_Entity_BaseActuatorDbFileReference_View = new RoleApiFunction
            {
                Identifier = "Entity.BaseActuatorDbFileReference.View",
                Desc = "BaseActuatorDbFileReference Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorDbFileReference_View);
            

            var roleApiFunction_Entity_SafetyController_Read = new RoleApiFunction
            {
                Identifier = "Entity.SafetyController.Read",
                Desc = "SafetyController Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyController_Read);

            var roleApiFunction_Entity_SafetyController_Create = new RoleApiFunction
            {
                Identifier = "Entity.SafetyController.Create",
                Desc = "SafetyController Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyController_Create);

            var roleApiFunction_Entity_SafetyController_Update = new RoleApiFunction
            {
                Identifier = "Entity.SafetyController.Update",
                Desc = "SafetyController Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyController_Update);

            var roleApiFunction_Entity_SafetyController_Delete = new RoleApiFunction
            {
                Identifier = "Entity.SafetyController.Delete",
                Desc = "SafetyController Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyController_Delete);

            var roleApiFunction_Entity_SafetyController_View = new RoleApiFunction
            {
                Identifier = "Entity.SafetyController.View",
                Desc = "SafetyController Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyController_View);
            

            var roleApiFunction_Entity_SafetyControllerParam_Read = new RoleApiFunction
            {
                Identifier = "Entity.SafetyControllerParam.Read",
                Desc = "SafetyControllerParam Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerParam_Read);

            var roleApiFunction_Entity_SafetyControllerParam_Create = new RoleApiFunction
            {
                Identifier = "Entity.SafetyControllerParam.Create",
                Desc = "SafetyControllerParam Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerParam_Create);

            var roleApiFunction_Entity_SafetyControllerParam_Update = new RoleApiFunction
            {
                Identifier = "Entity.SafetyControllerParam.Update",
                Desc = "SafetyControllerParam Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerParam_Update);

            var roleApiFunction_Entity_SafetyControllerParam_Delete = new RoleApiFunction
            {
                Identifier = "Entity.SafetyControllerParam.Delete",
                Desc = "SafetyControllerParam Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerParam_Delete);

            var roleApiFunction_Entity_SafetyControllerParam_View = new RoleApiFunction
            {
                Identifier = "Entity.SafetyControllerParam.View",
                Desc = "SafetyControllerParam Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerParam_View);
            

            var roleApiFunction_Entity_SafetyControllerDbFileReference_Read = new RoleApiFunction
            {
                Identifier = "Entity.SafetyControllerDbFileReference.Read",
                Desc = "SafetyControllerDbFileReference Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerDbFileReference_Read);

            var roleApiFunction_Entity_SafetyControllerDbFileReference_Create = new RoleApiFunction
            {
                Identifier = "Entity.SafetyControllerDbFileReference.Create",
                Desc = "SafetyControllerDbFileReference Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerDbFileReference_Create);

            var roleApiFunction_Entity_SafetyControllerDbFileReference_Update = new RoleApiFunction
            {
                Identifier = "Entity.SafetyControllerDbFileReference.Update",
                Desc = "SafetyControllerDbFileReference Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerDbFileReference_Update);

            var roleApiFunction_Entity_SafetyControllerDbFileReference_Delete = new RoleApiFunction
            {
                Identifier = "Entity.SafetyControllerDbFileReference.Delete",
                Desc = "SafetyControllerDbFileReference Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerDbFileReference_Delete);

            var roleApiFunction_Entity_SafetyControllerDbFileReference_View = new RoleApiFunction
            {
                Identifier = "Entity.SafetyControllerDbFileReference.View",
                Desc = "SafetyControllerDbFileReference Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerDbFileReference_View);
            

            var roleApiFunction_Entity_Legend_Read = new RoleApiFunction
            {
                Identifier = "Entity.Legend.Read",
                Desc = "Legend Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Legend_Read);

            var roleApiFunction_Entity_Legend_Create = new RoleApiFunction
            {
                Identifier = "Entity.Legend.Create",
                Desc = "Legend Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Legend_Create);

            var roleApiFunction_Entity_Legend_Update = new RoleApiFunction
            {
                Identifier = "Entity.Legend.Update",
                Desc = "Legend Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Legend_Update);

            var roleApiFunction_Entity_Legend_Delete = new RoleApiFunction
            {
                Identifier = "Entity.Legend.Delete",
                Desc = "Legend Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Legend_Delete);

            var roleApiFunction_Entity_Legend_View = new RoleApiFunction
            {
                Identifier = "Entity.Legend.View",
                Desc = "Legend Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Legend_View);
            

            var roleApiFunction_Entity_LegendDbFileReference_Read = new RoleApiFunction
            {
                Identifier = "Entity.LegendDbFileReference.Read",
                Desc = "LegendDbFileReference Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_LegendDbFileReference_Read);

            var roleApiFunction_Entity_LegendDbFileReference_Create = new RoleApiFunction
            {
                Identifier = "Entity.LegendDbFileReference.Create",
                Desc = "LegendDbFileReference Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_LegendDbFileReference_Create);

            var roleApiFunction_Entity_LegendDbFileReference_Update = new RoleApiFunction
            {
                Identifier = "Entity.LegendDbFileReference.Update",
                Desc = "LegendDbFileReference Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_LegendDbFileReference_Update);

            var roleApiFunction_Entity_LegendDbFileReference_Delete = new RoleApiFunction
            {
                Identifier = "Entity.LegendDbFileReference.Delete",
                Desc = "LegendDbFileReference Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_LegendDbFileReference_Delete);

            var roleApiFunction_Entity_LegendDbFileReference_View = new RoleApiFunction
            {
                Identifier = "Entity.LegendDbFileReference.View",
                Desc = "LegendDbFileReference Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_LegendDbFileReference_View);
            

            var roleApiFunction_Entity_LegendParam_Read = new RoleApiFunction
            {
                Identifier = "Entity.LegendParam.Read",
                Desc = "LegendParam Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_LegendParam_Read);

            var roleApiFunction_Entity_LegendParam_Create = new RoleApiFunction
            {
                Identifier = "Entity.LegendParam.Create",
                Desc = "LegendParam Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_LegendParam_Create);

            var roleApiFunction_Entity_LegendParam_Update = new RoleApiFunction
            {
                Identifier = "Entity.LegendParam.Update",
                Desc = "LegendParam Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_LegendParam_Update);

            var roleApiFunction_Entity_LegendParam_Delete = new RoleApiFunction
            {
                Identifier = "Entity.LegendParam.Delete",
                Desc = "LegendParam Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_LegendParam_Delete);

            var roleApiFunction_Entity_LegendParam_View = new RoleApiFunction
            {
                Identifier = "Entity.LegendParam.View",
                Desc = "LegendParam Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_LegendParam_View);
            

            var roleApiFunction_Entity_UnitEventsInterval_Read = new RoleApiFunction
            {
                Identifier = "Entity.UnitEventsInterval.Read",
                Desc = "UnitEventsInterval Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEventsInterval_Read);

            var roleApiFunction_Entity_UnitEventsInterval_Create = new RoleApiFunction
            {
                Identifier = "Entity.UnitEventsInterval.Create",
                Desc = "UnitEventsInterval Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEventsInterval_Create);

            var roleApiFunction_Entity_UnitEventsInterval_Update = new RoleApiFunction
            {
                Identifier = "Entity.UnitEventsInterval.Update",
                Desc = "UnitEventsInterval Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEventsInterval_Update);

            var roleApiFunction_Entity_UnitEventsInterval_Delete = new RoleApiFunction
            {
                Identifier = "Entity.UnitEventsInterval.Delete",
                Desc = "UnitEventsInterval Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEventsInterval_Delete);

            var roleApiFunction_Entity_UnitEventsInterval_View = new RoleApiFunction
            {
                Identifier = "Entity.UnitEventsInterval.View",
                Desc = "UnitEventsInterval Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEventsInterval_View);
            

            var roleApiFunction_Entity_UnitEvent_Read = new RoleApiFunction
            {
                Identifier = "Entity.UnitEvent.Read",
                Desc = "UnitEvent Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEvent_Read);

            var roleApiFunction_Entity_UnitEvent_Create = new RoleApiFunction
            {
                Identifier = "Entity.UnitEvent.Create",
                Desc = "UnitEvent Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEvent_Create);

            var roleApiFunction_Entity_UnitEvent_Update = new RoleApiFunction
            {
                Identifier = "Entity.UnitEvent.Update",
                Desc = "UnitEvent Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEvent_Update);

            var roleApiFunction_Entity_UnitEvent_Delete = new RoleApiFunction
            {
                Identifier = "Entity.UnitEvent.Delete",
                Desc = "UnitEvent Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEvent_Delete);

            var roleApiFunction_Entity_UnitEvent_View = new RoleApiFunction
            {
                Identifier = "Entity.UnitEvent.View",
                Desc = "UnitEvent Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEvent_View);
            

            var roleApiFunction_Entity_BasePcObject_Read = new RoleApiFunction
            {
                Identifier = "Entity.BasePcObject.Read",
                Desc = "BasePcObject Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObject_Read);

            var roleApiFunction_Entity_BasePcObject_Create = new RoleApiFunction
            {
                Identifier = "Entity.BasePcObject.Create",
                Desc = "BasePcObject Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObject_Create);

            var roleApiFunction_Entity_BasePcObject_Update = new RoleApiFunction
            {
                Identifier = "Entity.BasePcObject.Update",
                Desc = "BasePcObject Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObject_Update);

            var roleApiFunction_Entity_BasePcObject_Delete = new RoleApiFunction
            {
                Identifier = "Entity.BasePcObject.Delete",
                Desc = "BasePcObject Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObject_Delete);

            var roleApiFunction_Entity_BasePcObject_View = new RoleApiFunction
            {
                Identifier = "Entity.BasePcObject.View",
                Desc = "BasePcObject Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObject_View);
            

            var roleApiFunction_Entity_PcObjectEventType_Read = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectEventType.Read",
                Desc = "PcObjectEventType Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventType_Read);

            var roleApiFunction_Entity_PcObjectEventType_Create = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectEventType.Create",
                Desc = "PcObjectEventType Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventType_Create);

            var roleApiFunction_Entity_PcObjectEventType_Update = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectEventType.Update",
                Desc = "PcObjectEventType Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventType_Update);

            var roleApiFunction_Entity_PcObjectEventType_Delete = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectEventType.Delete",
                Desc = "PcObjectEventType Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventType_Delete);

            var roleApiFunction_Entity_PcObjectEventType_View = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectEventType.View",
                Desc = "PcObjectEventType Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventType_View);
            

            var roleApiFunction_Entity_BasePcObjectDbFileReference_Read = new RoleApiFunction
            {
                Identifier = "Entity.BasePcObjectDbFileReference.Read",
                Desc = "BasePcObjectDbFileReference Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectDbFileReference_Read);

            var roleApiFunction_Entity_BasePcObjectDbFileReference_Create = new RoleApiFunction
            {
                Identifier = "Entity.BasePcObjectDbFileReference.Create",
                Desc = "BasePcObjectDbFileReference Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectDbFileReference_Create);

            var roleApiFunction_Entity_BasePcObjectDbFileReference_Update = new RoleApiFunction
            {
                Identifier = "Entity.BasePcObjectDbFileReference.Update",
                Desc = "BasePcObjectDbFileReference Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectDbFileReference_Update);

            var roleApiFunction_Entity_BasePcObjectDbFileReference_Delete = new RoleApiFunction
            {
                Identifier = "Entity.BasePcObjectDbFileReference.Delete",
                Desc = "BasePcObjectDbFileReference Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectDbFileReference_Delete);

            var roleApiFunction_Entity_BasePcObjectDbFileReference_View = new RoleApiFunction
            {
                Identifier = "Entity.BasePcObjectDbFileReference.View",
                Desc = "BasePcObjectDbFileReference Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectDbFileReference_View);
            

            var roleApiFunction_Entity_PcObject_Read = new RoleApiFunction
            {
                Identifier = "Entity.PcObject.Read",
                Desc = "PcObject Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObject_Read);

            var roleApiFunction_Entity_PcObject_Create = new RoleApiFunction
            {
                Identifier = "Entity.PcObject.Create",
                Desc = "PcObject Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObject_Create);

            var roleApiFunction_Entity_PcObject_Update = new RoleApiFunction
            {
                Identifier = "Entity.PcObject.Update",
                Desc = "PcObject Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObject_Update);

            var roleApiFunction_Entity_PcObject_Delete = new RoleApiFunction
            {
                Identifier = "Entity.PcObject.Delete",
                Desc = "PcObject Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObject_Delete);

            var roleApiFunction_Entity_PcObject_View = new RoleApiFunction
            {
                Identifier = "Entity.PcObject.View",
                Desc = "PcObject Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObject_View);
            

            var roleApiFunction_Entity_PcObjectDbFileReference_Read = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectDbFileReference.Read",
                Desc = "PcObjectDbFileReference Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectDbFileReference_Read);

            var roleApiFunction_Entity_PcObjectDbFileReference_Create = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectDbFileReference.Create",
                Desc = "PcObjectDbFileReference Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectDbFileReference_Create);

            var roleApiFunction_Entity_PcObjectDbFileReference_Update = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectDbFileReference.Update",
                Desc = "PcObjectDbFileReference Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectDbFileReference_Update);

            var roleApiFunction_Entity_PcObjectDbFileReference_Delete = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectDbFileReference.Delete",
                Desc = "PcObjectDbFileReference Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectDbFileReference_Delete);

            var roleApiFunction_Entity_PcObjectDbFileReference_View = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectDbFileReference.View",
                Desc = "PcObjectDbFileReference Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectDbFileReference_View);
            

            var roleApiFunction_Entity_PcObjectEvent_Read = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectEvent.Read",
                Desc = "PcObjectEvent Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEvent_Read);

            var roleApiFunction_Entity_PcObjectEvent_Create = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectEvent.Create",
                Desc = "PcObjectEvent Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEvent_Create);

            var roleApiFunction_Entity_PcObjectEvent_Update = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectEvent.Update",
                Desc = "PcObjectEvent Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEvent_Update);

            var roleApiFunction_Entity_PcObjectEvent_Delete = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectEvent.Delete",
                Desc = "PcObjectEvent Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEvent_Delete);

            var roleApiFunction_Entity_PcObjectEvent_View = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectEvent.View",
                Desc = "PcObjectEvent Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEvent_View);
            

            var roleApiFunction_Entity_PcObjectEventDbFileReference_Read = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectEventDbFileReference.Read",
                Desc = "PcObjectEventDbFileReference Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventDbFileReference_Read);

            var roleApiFunction_Entity_PcObjectEventDbFileReference_Create = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectEventDbFileReference.Create",
                Desc = "PcObjectEventDbFileReference Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventDbFileReference_Create);

            var roleApiFunction_Entity_PcObjectEventDbFileReference_Update = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectEventDbFileReference.Update",
                Desc = "PcObjectEventDbFileReference Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventDbFileReference_Update);

            var roleApiFunction_Entity_PcObjectEventDbFileReference_Delete = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectEventDbFileReference.Delete",
                Desc = "PcObjectEventDbFileReference Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventDbFileReference_Delete);

            var roleApiFunction_Entity_PcObjectEventDbFileReference_View = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectEventDbFileReference.View",
                Desc = "PcObjectEventDbFileReference Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventDbFileReference_View);
            

            var roleApiFunction_Entity_DbFile_Read = new RoleApiFunction
            {
                Identifier = "Entity.DbFile.Read",
                Desc = "DbFile Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_DbFile_Read);

            var roleApiFunction_Entity_DbFile_Create = new RoleApiFunction
            {
                Identifier = "Entity.DbFile.Create",
                Desc = "DbFile Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_DbFile_Create);

            var roleApiFunction_Entity_DbFile_Update = new RoleApiFunction
            {
                Identifier = "Entity.DbFile.Update",
                Desc = "DbFile Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_DbFile_Update);

            var roleApiFunction_Entity_DbFile_Delete = new RoleApiFunction
            {
                Identifier = "Entity.DbFile.Delete",
                Desc = "DbFile Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_DbFile_Delete);

            var roleApiFunction_Entity_DbFile_View = new RoleApiFunction
            {
                Identifier = "Entity.DbFile.View",
                Desc = "DbFile Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_DbFile_View);
            

            var roleApiFunction_Entity_DbFileContent_Read = new RoleApiFunction
            {
                Identifier = "Entity.DbFileContent.Read",
                Desc = "DbFileContent Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_DbFileContent_Read);

            var roleApiFunction_Entity_DbFileContent_Create = new RoleApiFunction
            {
                Identifier = "Entity.DbFileContent.Create",
                Desc = "DbFileContent Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_DbFileContent_Create);

            var roleApiFunction_Entity_DbFileContent_Update = new RoleApiFunction
            {
                Identifier = "Entity.DbFileContent.Update",
                Desc = "DbFileContent Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_DbFileContent_Update);

            var roleApiFunction_Entity_DbFileContent_Delete = new RoleApiFunction
            {
                Identifier = "Entity.DbFileContent.Delete",
                Desc = "DbFileContent Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_DbFileContent_Delete);

            var roleApiFunction_Entity_DbFileContent_View = new RoleApiFunction
            {
                Identifier = "Entity.DbFileContent.View",
                Desc = "DbFileContent Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_DbFileContent_View);
            

            var roleApiFunction_Entity_ParamInfo_Read = new RoleApiFunction
            {
                Identifier = "Entity.ParamInfo.Read",
                Desc = "ParamInfo Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ParamInfo_Read);

            var roleApiFunction_Entity_ParamInfo_Create = new RoleApiFunction
            {
                Identifier = "Entity.ParamInfo.Create",
                Desc = "ParamInfo Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ParamInfo_Create);

            var roleApiFunction_Entity_ParamInfo_Update = new RoleApiFunction
            {
                Identifier = "Entity.ParamInfo.Update",
                Desc = "ParamInfo Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ParamInfo_Update);

            var roleApiFunction_Entity_ParamInfo_Delete = new RoleApiFunction
            {
                Identifier = "Entity.ParamInfo.Delete",
                Desc = "ParamInfo Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ParamInfo_Delete);

            var roleApiFunction_Entity_ParamInfo_View = new RoleApiFunction
            {
                Identifier = "Entity.ParamInfo.View",
                Desc = "ParamInfo Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ParamInfo_View);
            

            var roleApiFunction_Entity_ParamDesc_Read = new RoleApiFunction
            {
                Identifier = "Entity.ParamDesc.Read",
                Desc = "ParamDesc Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ParamDesc_Read);

            var roleApiFunction_Entity_ParamDesc_Create = new RoleApiFunction
            {
                Identifier = "Entity.ParamDesc.Create",
                Desc = "ParamDesc Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ParamDesc_Create);

            var roleApiFunction_Entity_ParamDesc_Update = new RoleApiFunction
            {
                Identifier = "Entity.ParamDesc.Update",
                Desc = "ParamDesc Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ParamDesc_Update);

            var roleApiFunction_Entity_ParamDesc_Delete = new RoleApiFunction
            {
                Identifier = "Entity.ParamDesc.Delete",
                Desc = "ParamDesc Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ParamDesc_Delete);

            var roleApiFunction_Entity_ParamDesc_View = new RoleApiFunction
            {
                Identifier = "Entity.ParamDesc.View",
                Desc = "ParamDesc Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ParamDesc_View);
            

            var roleApiFunction_Entity_Result_Read = new RoleApiFunction
            {
                Identifier = "Entity.Result.Read",
                Desc = "Result Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Result_Read);

            var roleApiFunction_Entity_Result_Create = new RoleApiFunction
            {
                Identifier = "Entity.Result.Create",
                Desc = "Result Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Result_Create);

            var roleApiFunction_Entity_Result_Update = new RoleApiFunction
            {
                Identifier = "Entity.Result.Update",
                Desc = "Result Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Result_Update);

            var roleApiFunction_Entity_Result_Delete = new RoleApiFunction
            {
                Identifier = "Entity.Result.Delete",
                Desc = "Result Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Result_Delete);

            var roleApiFunction_Entity_Result_View = new RoleApiFunction
            {
                Identifier = "Entity.Result.View",
                Desc = "Result Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Result_View);
            

            var roleApiFunction_Entity_ResultEvent_Read = new RoleApiFunction
            {
                Identifier = "Entity.ResultEvent.Read",
                Desc = "ResultEvent Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ResultEvent_Read);

            var roleApiFunction_Entity_ResultEvent_Create = new RoleApiFunction
            {
                Identifier = "Entity.ResultEvent.Create",
                Desc = "ResultEvent Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ResultEvent_Create);

            var roleApiFunction_Entity_ResultEvent_Update = new RoleApiFunction
            {
                Identifier = "Entity.ResultEvent.Update",
                Desc = "ResultEvent Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ResultEvent_Update);

            var roleApiFunction_Entity_ResultEvent_Delete = new RoleApiFunction
            {
                Identifier = "Entity.ResultEvent.Delete",
                Desc = "ResultEvent Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ResultEvent_Delete);

            var roleApiFunction_Entity_ResultEvent_View = new RoleApiFunction
            {
                Identifier = "Entity.ResultEvent.View",
                Desc = "ResultEvent Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ResultEvent_View);
            

            var roleApiFunction_Entity_CeMatrixResult_Read = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrixResult.Read",
                Desc = "CeMatrixResult Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixResult_Read);

            var roleApiFunction_Entity_CeMatrixResult_Create = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrixResult.Create",
                Desc = "CeMatrixResult Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixResult_Create);

            var roleApiFunction_Entity_CeMatrixResult_Update = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrixResult.Update",
                Desc = "CeMatrixResult Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixResult_Update);

            var roleApiFunction_Entity_CeMatrixResult_Delete = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrixResult.Delete",
                Desc = "CeMatrixResult Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixResult_Delete);

            var roleApiFunction_Entity_CeMatrixResult_View = new RoleApiFunction
            {
                Identifier = "Entity.CeMatrixResult.View",
                Desc = "CeMatrixResult Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixResult_View);
            

            var roleApiFunction_Entity_RowResult_Read = new RoleApiFunction
            {
                Identifier = "Entity.RowResult.Read",
                Desc = "RowResult Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RowResult_Read);

            var roleApiFunction_Entity_RowResult_Create = new RoleApiFunction
            {
                Identifier = "Entity.RowResult.Create",
                Desc = "RowResult Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RowResult_Create);

            var roleApiFunction_Entity_RowResult_Update = new RoleApiFunction
            {
                Identifier = "Entity.RowResult.Update",
                Desc = "RowResult Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RowResult_Update);

            var roleApiFunction_Entity_RowResult_Delete = new RoleApiFunction
            {
                Identifier = "Entity.RowResult.Delete",
                Desc = "RowResult Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RowResult_Delete);

            var roleApiFunction_Entity_RowResult_View = new RoleApiFunction
            {
                Identifier = "Entity.RowResult.View",
                Desc = "RowResult Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RowResult_View);
            

            var roleApiFunction_Entity_ColumnResult_Read = new RoleApiFunction
            {
                Identifier = "Entity.ColumnResult.Read",
                Desc = "ColumnResult Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ColumnResult_Read);

            var roleApiFunction_Entity_ColumnResult_Create = new RoleApiFunction
            {
                Identifier = "Entity.ColumnResult.Create",
                Desc = "ColumnResult Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ColumnResult_Create);

            var roleApiFunction_Entity_ColumnResult_Update = new RoleApiFunction
            {
                Identifier = "Entity.ColumnResult.Update",
                Desc = "ColumnResult Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ColumnResult_Update);

            var roleApiFunction_Entity_ColumnResult_Delete = new RoleApiFunction
            {
                Identifier = "Entity.ColumnResult.Delete",
                Desc = "ColumnResult Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ColumnResult_Delete);

            var roleApiFunction_Entity_ColumnResult_View = new RoleApiFunction
            {
                Identifier = "Entity.ColumnResult.View",
                Desc = "ColumnResult Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_ColumnResult_View);
            

            var roleApiFunction_Entity_CellResult_Read = new RoleApiFunction
            {
                Identifier = "Entity.CellResult.Read",
                Desc = "CellResult Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CellResult_Read);

            var roleApiFunction_Entity_CellResult_Create = new RoleApiFunction
            {
                Identifier = "Entity.CellResult.Create",
                Desc = "CellResult Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CellResult_Create);

            var roleApiFunction_Entity_CellResult_Update = new RoleApiFunction
            {
                Identifier = "Entity.CellResult.Update",
                Desc = "CellResult Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CellResult_Update);

            var roleApiFunction_Entity_CellResult_Delete = new RoleApiFunction
            {
                Identifier = "Entity.CellResult.Delete",
                Desc = "CellResult Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CellResult_Delete);

            var roleApiFunction_Entity_CellResult_View = new RoleApiFunction
            {
                Identifier = "Entity.CellResult.View",
                Desc = "CellResult Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CellResult_View);
            

            var roleApiFunction_Entity_Job_Read = new RoleApiFunction
            {
                Identifier = "Entity.Job.Read",
                Desc = "Job Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Job_Read);

            var roleApiFunction_Entity_Job_Create = new RoleApiFunction
            {
                Identifier = "Entity.Job.Create",
                Desc = "Job Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Job_Create);

            var roleApiFunction_Entity_Job_Update = new RoleApiFunction
            {
                Identifier = "Entity.Job.Update",
                Desc = "Job Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Job_Update);

            var roleApiFunction_Entity_Job_Delete = new RoleApiFunction
            {
                Identifier = "Entity.Job.Delete",
                Desc = "Job Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Job_Delete);

            var roleApiFunction_Entity_Job_View = new RoleApiFunction
            {
                Identifier = "Entity.Job.View",
                Desc = "Job Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Job_View);
            

            var roleApiFunction_Entity_AddonStatus_Read = new RoleApiFunction
            {
                Identifier = "Entity.AddonStatus.Read",
                Desc = "AddonStatus Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_AddonStatus_Read);

            var roleApiFunction_Entity_AddonStatus_Create = new RoleApiFunction
            {
                Identifier = "Entity.AddonStatus.Create",
                Desc = "AddonStatus Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_AddonStatus_Create);

            var roleApiFunction_Entity_AddonStatus_Update = new RoleApiFunction
            {
                Identifier = "Entity.AddonStatus.Update",
                Desc = "AddonStatus Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_AddonStatus_Update);

            var roleApiFunction_Entity_AddonStatus_Delete = new RoleApiFunction
            {
                Identifier = "Entity.AddonStatus.Delete",
                Desc = "AddonStatus Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_AddonStatus_Delete);

            var roleApiFunction_Entity_AddonStatus_View = new RoleApiFunction
            {
                Identifier = "Entity.AddonStatus.View",
                Desc = "AddonStatus Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_AddonStatus_View);
            

            var roleApiFunction_Entity_UserEvent_Read = new RoleApiFunction
            {
                Identifier = "Entity.UserEvent.Read",
                Desc = "UserEvent Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_UserEvent_Read);

            var roleApiFunction_Entity_UserEvent_Create = new RoleApiFunction
            {
                Identifier = "Entity.UserEvent.Create",
                Desc = "UserEvent Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_UserEvent_Create);

            var roleApiFunction_Entity_UserEvent_Update = new RoleApiFunction
            {
                Identifier = "Entity.UserEvent.Update",
                Desc = "UserEvent Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_UserEvent_Update);

            var roleApiFunction_Entity_UserEvent_Delete = new RoleApiFunction
            {
                Identifier = "Entity.UserEvent.Delete",
                Desc = "UserEvent Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_UserEvent_Delete);

            var roleApiFunction_Entity_UserEvent_View = new RoleApiFunction
            {
                Identifier = "Entity.UserEvent.View",
                Desc = "UserEvent Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_UserEvent_View);
            

            var roleApiFunction_Entity_InformationMessage_Read = new RoleApiFunction
            {
                Identifier = "Entity.InformationMessage.Read",
                Desc = "InformationMessage Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_InformationMessage_Read);

            var roleApiFunction_Entity_InformationMessage_Create = new RoleApiFunction
            {
                Identifier = "Entity.InformationMessage.Create",
                Desc = "InformationMessage Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_InformationMessage_Create);

            var roleApiFunction_Entity_InformationMessage_Update = new RoleApiFunction
            {
                Identifier = "Entity.InformationMessage.Update",
                Desc = "InformationMessage Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_InformationMessage_Update);

            var roleApiFunction_Entity_InformationMessage_Delete = new RoleApiFunction
            {
                Identifier = "Entity.InformationMessage.Delete",
                Desc = "InformationMessage Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_InformationMessage_Delete);

            var roleApiFunction_Entity_InformationMessage_View = new RoleApiFunction
            {
                Identifier = "Entity.InformationMessage.View",
                Desc = "InformationMessage Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_InformationMessage_View);
            

            var roleApiFunction_Entity_RequestMessage_Read = new RoleApiFunction
            {
                Identifier = "Entity.RequestMessage.Read",
                Desc = "RequestMessage Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RequestMessage_Read);

            var roleApiFunction_Entity_RequestMessage_Create = new RoleApiFunction
            {
                Identifier = "Entity.RequestMessage.Create",
                Desc = "RequestMessage Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RequestMessage_Create);

            var roleApiFunction_Entity_RequestMessage_Update = new RoleApiFunction
            {
                Identifier = "Entity.RequestMessage.Update",
                Desc = "RequestMessage Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RequestMessage_Update);

            var roleApiFunction_Entity_RequestMessage_Delete = new RoleApiFunction
            {
                Identifier = "Entity.RequestMessage.Delete",
                Desc = "RequestMessage Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RequestMessage_Delete);

            var roleApiFunction_Entity_RequestMessage_View = new RoleApiFunction
            {
                Identifier = "Entity.RequestMessage.View",
                Desc = "RequestMessage Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RequestMessage_View);
            

            var roleApiFunction_Entity_BasePcObjectJournalParam_Read = new RoleApiFunction
            {
                Identifier = "Entity.BasePcObjectJournalParam.Read",
                Desc = "BasePcObjectJournalParam Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectJournalParam_Read);

            var roleApiFunction_Entity_BasePcObjectJournalParam_Create = new RoleApiFunction
            {
                Identifier = "Entity.BasePcObjectJournalParam.Create",
                Desc = "BasePcObjectJournalParam Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectJournalParam_Create);

            var roleApiFunction_Entity_BasePcObjectJournalParam_Update = new RoleApiFunction
            {
                Identifier = "Entity.BasePcObjectJournalParam.Update",
                Desc = "BasePcObjectJournalParam Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectJournalParam_Update);

            var roleApiFunction_Entity_BasePcObjectJournalParam_Delete = new RoleApiFunction
            {
                Identifier = "Entity.BasePcObjectJournalParam.Delete",
                Desc = "BasePcObjectJournalParam Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectJournalParam_Delete);

            var roleApiFunction_Entity_BasePcObjectJournalParam_View = new RoleApiFunction
            {
                Identifier = "Entity.BasePcObjectJournalParam.View",
                Desc = "BasePcObjectJournalParam Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectJournalParam_View);
            

            var roleApiFunction_Entity_PcObjectJournalParam_Read = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectJournalParam.Read",
                Desc = "PcObjectJournalParam Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectJournalParam_Read);

            var roleApiFunction_Entity_PcObjectJournalParam_Create = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectJournalParam.Create",
                Desc = "PcObjectJournalParam Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectJournalParam_Create);

            var roleApiFunction_Entity_PcObjectJournalParam_Update = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectJournalParam.Update",
                Desc = "PcObjectJournalParam Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectJournalParam_Update);

            var roleApiFunction_Entity_PcObjectJournalParam_Delete = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectJournalParam.Delete",
                Desc = "PcObjectJournalParam Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectJournalParam_Delete);

            var roleApiFunction_Entity_PcObjectJournalParam_View = new RoleApiFunction
            {
                Identifier = "Entity.PcObjectJournalParam.View",
                Desc = "PcObjectJournalParam Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectJournalParam_View);
            

            var roleApiFunction_Entity_JournalParamValuesCollection_Read = new RoleApiFunction
            {
                Identifier = "Entity.JournalParamValuesCollection.Read",
                Desc = "JournalParamValuesCollection Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_JournalParamValuesCollection_Read);

            var roleApiFunction_Entity_JournalParamValuesCollection_Create = new RoleApiFunction
            {
                Identifier = "Entity.JournalParamValuesCollection.Create",
                Desc = "JournalParamValuesCollection Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_JournalParamValuesCollection_Create);

            var roleApiFunction_Entity_JournalParamValuesCollection_Update = new RoleApiFunction
            {
                Identifier = "Entity.JournalParamValuesCollection.Update",
                Desc = "JournalParamValuesCollection Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_JournalParamValuesCollection_Update);

            var roleApiFunction_Entity_JournalParamValuesCollection_Delete = new RoleApiFunction
            {
                Identifier = "Entity.JournalParamValuesCollection.Delete",
                Desc = "JournalParamValuesCollection Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_JournalParamValuesCollection_Delete);

            var roleApiFunction_Entity_JournalParamValuesCollection_View = new RoleApiFunction
            {
                Identifier = "Entity.JournalParamValuesCollection.View",
                Desc = "JournalParamValuesCollection Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_JournalParamValuesCollection_View);
            

            var roleApiFunction_Entity_FloatJournalParamValue_Read = new RoleApiFunction
            {
                Identifier = "Entity.FloatJournalParamValue.Read",
                Desc = "FloatJournalParamValue Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_FloatJournalParamValue_Read);

            var roleApiFunction_Entity_FloatJournalParamValue_Create = new RoleApiFunction
            {
                Identifier = "Entity.FloatJournalParamValue.Create",
                Desc = "FloatJournalParamValue Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_FloatJournalParamValue_Create);

            var roleApiFunction_Entity_FloatJournalParamValue_Update = new RoleApiFunction
            {
                Identifier = "Entity.FloatJournalParamValue.Update",
                Desc = "FloatJournalParamValue Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_FloatJournalParamValue_Update);

            var roleApiFunction_Entity_FloatJournalParamValue_Delete = new RoleApiFunction
            {
                Identifier = "Entity.FloatJournalParamValue.Delete",
                Desc = "FloatJournalParamValue Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_FloatJournalParamValue_Delete);

            var roleApiFunction_Entity_FloatJournalParamValue_View = new RoleApiFunction
            {
                Identifier = "Entity.FloatJournalParamValue.View",
                Desc = "FloatJournalParamValue Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_FloatJournalParamValue_View);
            

            var roleApiFunction_Entity_InformationSecurityEvent_Read = new RoleApiFunction
            {
                Identifier = "Entity.InformationSecurityEvent.Read",
                Desc = "InformationSecurityEvent Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_InformationSecurityEvent_Read);

            var roleApiFunction_Entity_InformationSecurityEvent_Create = new RoleApiFunction
            {
                Identifier = "Entity.InformationSecurityEvent.Create",
                Desc = "InformationSecurityEvent Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_InformationSecurityEvent_Create);

            var roleApiFunction_Entity_InformationSecurityEvent_Update = new RoleApiFunction
            {
                Identifier = "Entity.InformationSecurityEvent.Update",
                Desc = "InformationSecurityEvent Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_InformationSecurityEvent_Update);

            var roleApiFunction_Entity_InformationSecurityEvent_Delete = new RoleApiFunction
            {
                Identifier = "Entity.InformationSecurityEvent.Delete",
                Desc = "InformationSecurityEvent Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_InformationSecurityEvent_Delete);

            var roleApiFunction_Entity_InformationSecurityEvent_View = new RoleApiFunction
            {
                Identifier = "Entity.InformationSecurityEvent.View",
                Desc = "InformationSecurityEvent Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_InformationSecurityEvent_View);
            

            var roleApiFunction_Entity_AllRolesAccessInformationSecurityEvent_Read = new RoleApiFunction
            {
                Identifier = "Entity.AllRolesAccessInformationSecurityEvent.Read",
                Desc = "AllRolesAccessInformationSecurityEvent Чтение",
                Modifier = "AllRoles"
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_AllRolesAccessInformationSecurityEvent_Read);

            var roleApiFunction_Entity_AllRolesAccessInformationSecurityEvent_Create = new RoleApiFunction
            {
                Identifier = "Entity.AllRolesAccessInformationSecurityEvent.Create",
                Desc = "AllRolesAccessInformationSecurityEvent Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_AllRolesAccessInformationSecurityEvent_Create);

            var roleApiFunction_Entity_AllRolesAccessInformationSecurityEvent_Update = new RoleApiFunction
            {
                Identifier = "Entity.AllRolesAccessInformationSecurityEvent.Update",
                Desc = "AllRolesAccessInformationSecurityEvent Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_AllRolesAccessInformationSecurityEvent_Update);

            var roleApiFunction_Entity_AllRolesAccessInformationSecurityEvent_Delete = new RoleApiFunction
            {
                Identifier = "Entity.AllRolesAccessInformationSecurityEvent.Delete",
                Desc = "AllRolesAccessInformationSecurityEvent Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_AllRolesAccessInformationSecurityEvent_Delete);

            var roleApiFunction_Entity_AllRolesAccessInformationSecurityEvent_View = new RoleApiFunction
            {
                Identifier = "Entity.AllRolesAccessInformationSecurityEvent.View",
                Desc = "AllRolesAccessInformationSecurityEvent Просмотр в UI",
                Modifier = "AllRoles"                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_AllRolesAccessInformationSecurityEvent_View);
            

            var roleApiFunction_Entity_Role_Read = new RoleApiFunction
            {
                Identifier = "Entity.Role.Read",
                Desc = "Role Чтение",
                Modifier = "Public"
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Role_Read);

            var roleApiFunction_Entity_Role_Create = new RoleApiFunction
            {
                Identifier = "Entity.Role.Create",
                Desc = "Role Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Role_Create);

            var roleApiFunction_Entity_Role_Update = new RoleApiFunction
            {
                Identifier = "Entity.Role.Update",
                Desc = "Role Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Role_Update);

            var roleApiFunction_Entity_Role_Delete = new RoleApiFunction
            {
                Identifier = "Entity.Role.Delete",
                Desc = "Role Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Role_Delete);

            var roleApiFunction_Entity_Role_View = new RoleApiFunction
            {
                Identifier = "Entity.Role.View",
                Desc = "Role Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_Role_View);
            

            var roleApiFunction_Entity_RolePermission_Read = new RoleApiFunction
            {
                Identifier = "Entity.RolePermission.Read",
                Desc = "RolePermission Чтение",
                Modifier = "AllRoles"
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RolePermission_Read);

            var roleApiFunction_Entity_RolePermission_Create = new RoleApiFunction
            {
                Identifier = "Entity.RolePermission.Create",
                Desc = "RolePermission Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RolePermission_Create);

            var roleApiFunction_Entity_RolePermission_Update = new RoleApiFunction
            {
                Identifier = "Entity.RolePermission.Update",
                Desc = "RolePermission Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RolePermission_Update);

            var roleApiFunction_Entity_RolePermission_Delete = new RoleApiFunction
            {
                Identifier = "Entity.RolePermission.Delete",
                Desc = "RolePermission Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RolePermission_Delete);

            var roleApiFunction_Entity_RolePermission_View = new RoleApiFunction
            {
                Identifier = "Entity.RolePermission.View",
                Desc = "RolePermission Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RolePermission_View);
            

            var roleApiFunction_Entity_RoleBusinessFunction_Read = new RoleApiFunction
            {
                Identifier = "Entity.RoleBusinessFunction.Read",
                Desc = "RoleBusinessFunction Чтение",
                Modifier = "AllRoles"
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RoleBusinessFunction_Read);

            var roleApiFunction_Entity_RoleBusinessFunction_Create = new RoleApiFunction
            {
                Identifier = "Entity.RoleBusinessFunction.Create",
                Desc = "RoleBusinessFunction Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RoleBusinessFunction_Create);

            var roleApiFunction_Entity_RoleBusinessFunction_Update = new RoleApiFunction
            {
                Identifier = "Entity.RoleBusinessFunction.Update",
                Desc = "RoleBusinessFunction Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RoleBusinessFunction_Update);

            var roleApiFunction_Entity_RoleBusinessFunction_Delete = new RoleApiFunction
            {
                Identifier = "Entity.RoleBusinessFunction.Delete",
                Desc = "RoleBusinessFunction Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RoleBusinessFunction_Delete);

            var roleApiFunction_Entity_RoleBusinessFunction_View = new RoleApiFunction
            {
                Identifier = "Entity.RoleBusinessFunction.View",
                Desc = "RoleBusinessFunction Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RoleBusinessFunction_View);
            

            var roleApiFunction_Entity_RoleApiFunction_Read = new RoleApiFunction
            {
                Identifier = "Entity.RoleApiFunction.Read",
                Desc = "RoleApiFunction Чтение",
                Modifier = "AllRoles"
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RoleApiFunction_Read);

            var roleApiFunction_Entity_RoleApiFunction_Create = new RoleApiFunction
            {
                Identifier = "Entity.RoleApiFunction.Create",
                Desc = "RoleApiFunction Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RoleApiFunction_Create);

            var roleApiFunction_Entity_RoleApiFunction_Update = new RoleApiFunction
            {
                Identifier = "Entity.RoleApiFunction.Update",
                Desc = "RoleApiFunction Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RoleApiFunction_Update);

            var roleApiFunction_Entity_RoleApiFunction_Delete = new RoleApiFunction
            {
                Identifier = "Entity.RoleApiFunction.Delete",
                Desc = "RoleApiFunction Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RoleApiFunction_Delete);

            var roleApiFunction_Entity_RoleApiFunction_View = new RoleApiFunction
            {
                Identifier = "Entity.RoleApiFunction.View",
                Desc = "RoleApiFunction Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_RoleApiFunction_View);
            

            var roleApiFunction_Entity_LicenseFile_Read = new RoleApiFunction
            {
                Identifier = "Entity.LicenseFile.Read",
                Desc = "LicenseFile Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_LicenseFile_Read);

            var roleApiFunction_Entity_LicenseFile_Create = new RoleApiFunction
            {
                Identifier = "Entity.LicenseFile.Create",
                Desc = "LicenseFile Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_LicenseFile_Create);

            var roleApiFunction_Entity_LicenseFile_Update = new RoleApiFunction
            {
                Identifier = "Entity.LicenseFile.Update",
                Desc = "LicenseFile Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_LicenseFile_Update);

            var roleApiFunction_Entity_LicenseFile_Delete = new RoleApiFunction
            {
                Identifier = "Entity.LicenseFile.Delete",
                Desc = "LicenseFile Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_LicenseFile_Delete);

            var roleApiFunction_Entity_LicenseFile_View = new RoleApiFunction
            {
                Identifier = "Entity.LicenseFile.View",
                Desc = "LicenseFile Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_LicenseFile_View);
            

            var roleApiFunction_Entity_CryptoEntity_Read = new RoleApiFunction
            {
                Identifier = "Entity.CryptoEntity.Read",
                Desc = "CryptoEntity Чтение",
                Modifier = ""
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CryptoEntity_Read);

            var roleApiFunction_Entity_CryptoEntity_Create = new RoleApiFunction
            {
                Identifier = "Entity.CryptoEntity.Create",
                Desc = "CryptoEntity Создание",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CryptoEntity_Create);

            var roleApiFunction_Entity_CryptoEntity_Update = new RoleApiFunction
            {
                Identifier = "Entity.CryptoEntity.Update",
                Desc = "CryptoEntity Редактирование",
                Modifier = ""              
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CryptoEntity_Update);

            var roleApiFunction_Entity_CryptoEntity_Delete = new RoleApiFunction
            {
                Identifier = "Entity.CryptoEntity.Delete",
                Desc = "CryptoEntity Удаление",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CryptoEntity_Delete);

            var roleApiFunction_Entity_CryptoEntity_View = new RoleApiFunction
            {
                Identifier = "Entity.CryptoEntity.View",
                Desc = "CryptoEntity Просмотр в UI",
                Modifier = ""                
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Entity_CryptoEntity_View);
            

            var roleApiFunction_Method_GetAddonTestInfo = new RoleApiFunction
            {
                Identifier = "Method.GetAddonTestInfo",
                Desc = "GetAddonTestInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetAddonTestInfo);

            var roleApiFunction_Method_AddonTest = new RoleApiFunction
            {
                Identifier = "Method.AddonTest",
                Desc = "AddonTest Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_AddonTest);

            var roleApiFunction_Method_ReadConfiguration = new RoleApiFunction
            {
                Identifier = "Method.ReadConfiguration",
                Desc = "ReadConfiguration Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_ReadConfiguration);

            var roleApiFunction_Method_WriteConfiguration = new RoleApiFunction
            {
                Identifier = "Method.WriteConfiguration",
                Desc = "WriteConfiguration Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_WriteConfiguration);

            var roleApiFunction_Method_AddCryptoEntity = new RoleApiFunction
            {
                Identifier = "Method.AddCryptoEntity",
                Desc = "AddCryptoEntity Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_AddCryptoEntity);

            var roleApiFunction_Method_GetFilterInfo = new RoleApiFunction
            {
                Identifier = "Method.GetFilterInfo",
                Desc = "GetFilterInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetFilterInfo);

            var roleApiFunction_Method_Filter = new RoleApiFunction
            {
                Identifier = "Method.Filter",
                Desc = "Filter Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_Filter);

            var roleApiFunction_Method_GetChildrenForSafetyIndex = new RoleApiFunction
            {
                Identifier = "Method.GetChildrenForSafetyIndex",
                Desc = "GetChildrenForSafetyIndex Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetChildrenForSafetyIndex);

            var roleApiFunction_Method_DeleteEntities = new RoleApiFunction
            {
                Identifier = "Method.DeleteEntities",
                Desc = "DeleteEntities Вызов",
                Modifier = "AllRoles"         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_DeleteEntities);

            var roleApiFunction_Method_AddResultToPcObject = new RoleApiFunction
            {
                Identifier = "Method.AddResultToPcObject",
                Desc = "AddResultToPcObject Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_AddResultToPcObject);

            var roleApiFunction_Method_ExportFiles = new RoleApiFunction
            {
                Identifier = "Method.ExportFiles",
                Desc = "ExportFiles Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_ExportFiles);

            var roleApiFunction_Method_ExportFile = new RoleApiFunction
            {
                Identifier = "Method.ExportFile",
                Desc = "ExportFile Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_ExportFile);

            var roleApiFunction_Method_GetExportProjectFilesInfo = new RoleApiFunction
            {
                Identifier = "Method.GetExportProjectFilesInfo",
                Desc = "GetExportProjectFilesInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetExportProjectFilesInfo);

            var roleApiFunction_Method_GetExportCeMatrixFilesInfo = new RoleApiFunction
            {
                Identifier = "Method.GetExportCeMatrixFilesInfo",
                Desc = "GetExportCeMatrixFilesInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetExportCeMatrixFilesInfo);

            var roleApiFunction_Method_GetExportTagFilesInfo = new RoleApiFunction
            {
                Identifier = "Method.GetExportTagFilesInfo",
                Desc = "GetExportTagFilesInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetExportTagFilesInfo);

            var roleApiFunction_Method_GetExportBaseActuatorFilesInfo = new RoleApiFunction
            {
                Identifier = "Method.GetExportBaseActuatorFilesInfo",
                Desc = "GetExportBaseActuatorFilesInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetExportBaseActuatorFilesInfo);

            var roleApiFunction_Method_GetExportSafetyControllerFilesInfo = new RoleApiFunction
            {
                Identifier = "Method.GetExportSafetyControllerFilesInfo",
                Desc = "GetExportSafetyControllerFilesInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetExportSafetyControllerFilesInfo);

            var roleApiFunction_Method_GetExportLegendFilesInfo = new RoleApiFunction
            {
                Identifier = "Method.GetExportLegendFilesInfo",
                Desc = "GetExportLegendFilesInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetExportLegendFilesInfo);

            var roleApiFunction_Method_ExportProjectFile = new RoleApiFunction
            {
                Identifier = "Method.ExportProjectFile",
                Desc = "ExportProjectFile Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_ExportProjectFile);

            var roleApiFunction_Method_GetExportEntitiesFilesInfo = new RoleApiFunction
            {
                Identifier = "Method.GetExportEntitiesFilesInfo",
                Desc = "GetExportEntitiesFilesInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetExportEntitiesFilesInfo);

            var roleApiFunction_Method_ExportEntitiesFile = new RoleApiFunction
            {
                Identifier = "Method.ExportEntitiesFile",
                Desc = "ExportEntitiesFile Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_ExportEntitiesFile);

            var roleApiFunction_Method_GetExportResultFilesInfo = new RoleApiFunction
            {
                Identifier = "Method.GetExportResultFilesInfo",
                Desc = "GetExportResultFilesInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetExportResultFilesInfo);

            var roleApiFunction_Method_ExportResultToFile = new RoleApiFunction
            {
                Identifier = "Method.ExportResultToFile",
                Desc = "ExportResultToFile Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_ExportResultToFile);

            var roleApiFunction_Method_GetExportCeMatrixResultFilesInfo = new RoleApiFunction
            {
                Identifier = "Method.GetExportCeMatrixResultFilesInfo",
                Desc = "GetExportCeMatrixResultFilesInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetExportCeMatrixResultFilesInfo);

            var roleApiFunction_Method_ExportCeMatrixResultsToFile = new RoleApiFunction
            {
                Identifier = "Method.ExportCeMatrixResultsToFile",
                Desc = "ExportCeMatrixResultsToFile Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_ExportCeMatrixResultsToFile);

            var roleApiFunction_Method_GetExportVersionComparisonFilesInfo = new RoleApiFunction
            {
                Identifier = "Method.GetExportVersionComparisonFilesInfo",
                Desc = "GetExportVersionComparisonFilesInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetExportVersionComparisonFilesInfo);

            var roleApiFunction_Method_ExportVersionComparisonToFile = new RoleApiFunction
            {
                Identifier = "Method.ExportVersionComparisonToFile",
                Desc = "ExportVersionComparisonToFile Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_ExportVersionComparisonToFile);

            var roleApiFunction_Method_GetExportEvents_FilesInfo = new RoleApiFunction
            {
                Identifier = "Method.GetExportEvents_FilesInfo",
                Desc = "GetExportEvents_FilesInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetExportEvents_FilesInfo);

            var roleApiFunction_Method_ExportEventsToFile = new RoleApiFunction
            {
                Identifier = "Method.ExportEventsToFile",
                Desc = "ExportEventsToFile Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_ExportEventsToFile);

            var roleApiFunction_Method_GetExportMonitoringWidgetsReport_FilesInfo = new RoleApiFunction
            {
                Identifier = "Method.GetExportMonitoringWidgetsReport_FilesInfo",
                Desc = "GetExportMonitoringWidgetsReport_FilesInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetExportMonitoringWidgetsReport_FilesInfo);

            var roleApiFunction_Method_ExportMonitoringWidgetsReportToFile = new RoleApiFunction
            {
                Identifier = "Method.ExportMonitoringWidgetsReportToFile",
                Desc = "ExportMonitoringWidgetsReportToFile Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_ExportMonitoringWidgetsReportToFile);

            var roleApiFunction_Method_GetExportMonitoringEventsReport_FilesInfo = new RoleApiFunction
            {
                Identifier = "Method.GetExportMonitoringEventsReport_FilesInfo",
                Desc = "GetExportMonitoringEventsReport_FilesInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetExportMonitoringEventsReport_FilesInfo);

            var roleApiFunction_Method_ExportMonitoringEventsReportToFile = new RoleApiFunction
            {
                Identifier = "Method.ExportMonitoringEventsReportToFile",
                Desc = "ExportMonitoringEventsReportToFile Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_ExportMonitoringEventsReportToFile);

            var roleApiFunction_Method_DatasourceHealth = new RoleApiFunction
            {
                Identifier = "Method.DatasourceHealth",
                Desc = "DatasourceHealth Вызов",
                Modifier = "Public"         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_DatasourceHealth);

            var roleApiFunction_Method_ListMetrics = new RoleApiFunction
            {
                Identifier = "Method.ListMetrics",
                Desc = "ListMetrics Вызов",
                Modifier = "Public"         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_ListMetrics);

            var roleApiFunction_Method_Query = new RoleApiFunction
            {
                Identifier = "Method.Query",
                Desc = "Query Вызов",
                Modifier = "Public"         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_Query);

            var roleApiFunction_Method_Annotations = new RoleApiFunction
            {
                Identifier = "Method.Annotations",
                Desc = "Annotations Вызов",
                Modifier = "Public"         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_Annotations);

            var roleApiFunction_Method_TagKeys = new RoleApiFunction
            {
                Identifier = "Method.TagKeys",
                Desc = "TagKeys Вызов",
                Modifier = "Public"         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_TagKeys);

            var roleApiFunction_Method_TagValues = new RoleApiFunction
            {
                Identifier = "Method.TagValues",
                Desc = "TagValues Вызов",
                Modifier = "Public"         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_TagValues);

            var roleApiFunction_Method_GetImportProjectFilesInfo = new RoleApiFunction
            {
                Identifier = "Method.GetImportProjectFilesInfo",
                Desc = "GetImportProjectFilesInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetImportProjectFilesInfo);

            var roleApiFunction_Method_ImportProjectFiles = new RoleApiFunction
            {
                Identifier = "Method.ImportProjectFiles",
                Desc = "ImportProjectFiles Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_ImportProjectFiles);

            var roleApiFunction_Method_GetImportEntitiesFilesInfo = new RoleApiFunction
            {
                Identifier = "Method.GetImportEntitiesFilesInfo",
                Desc = "GetImportEntitiesFilesInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetImportEntitiesFilesInfo);

            var roleApiFunction_Method_ImportEntitiesFiles = new RoleApiFunction
            {
                Identifier = "Method.ImportEntitiesFiles",
                Desc = "ImportEntitiesFiles Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_ImportEntitiesFiles);

            var roleApiFunction_Method_GetImportCeMatrixFilesInfo = new RoleApiFunction
            {
                Identifier = "Method.GetImportCeMatrixFilesInfo",
                Desc = "GetImportCeMatrixFilesInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetImportCeMatrixFilesInfo);

            var roleApiFunction_Method_ImportCeMatrixFiles = new RoleApiFunction
            {
                Identifier = "Method.ImportCeMatrixFiles",
                Desc = "ImportCeMatrixFiles Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_ImportCeMatrixFiles);

            var roleApiFunction_Method_GetImportTagFilesInfo = new RoleApiFunction
            {
                Identifier = "Method.GetImportTagFilesInfo",
                Desc = "GetImportTagFilesInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetImportTagFilesInfo);

            var roleApiFunction_Method_ImportTagFiles = new RoleApiFunction
            {
                Identifier = "Method.ImportTagFiles",
                Desc = "ImportTagFiles Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_ImportTagFiles);

            var roleApiFunction_Method_GetImportBaseActuatorFilesInfo = new RoleApiFunction
            {
                Identifier = "Method.GetImportBaseActuatorFilesInfo",
                Desc = "GetImportBaseActuatorFilesInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetImportBaseActuatorFilesInfo);

            var roleApiFunction_Method_ImportBaseActuatorFiles = new RoleApiFunction
            {
                Identifier = "Method.ImportBaseActuatorFiles",
                Desc = "ImportBaseActuatorFiles Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_ImportBaseActuatorFiles);

            var roleApiFunction_Method_GetImportSafetyControllerFilesInfo = new RoleApiFunction
            {
                Identifier = "Method.GetImportSafetyControllerFilesInfo",
                Desc = "GetImportSafetyControllerFilesInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetImportSafetyControllerFilesInfo);

            var roleApiFunction_Method_ImportSafetyControllerFiles = new RoleApiFunction
            {
                Identifier = "Method.ImportSafetyControllerFiles",
                Desc = "ImportSafetyControllerFiles Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_ImportSafetyControllerFiles);

            var roleApiFunction_Method_GetImportEventsJournalFilesInfo = new RoleApiFunction
            {
                Identifier = "Method.GetImportEventsJournalFilesInfo",
                Desc = "GetImportEventsJournalFilesInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetImportEventsJournalFilesInfo);

            var roleApiFunction_Method_ImportEventsJournalFiles = new RoleApiFunction
            {
                Identifier = "Method.ImportEventsJournalFiles",
                Desc = "ImportEventsJournalFiles Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_ImportEventsJournalFiles);

            var roleApiFunction_Method_GetImportLicenseFilesInfo = new RoleApiFunction
            {
                Identifier = "Method.GetImportLicenseFilesInfo",
                Desc = "GetImportLicenseFilesInfo Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetImportLicenseFilesInfo);

            var roleApiFunction_Method_ImportLicenseFiles = new RoleApiFunction
            {
                Identifier = "Method.ImportLicenseFiles",
                Desc = "ImportLicenseFiles Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_ImportLicenseFiles);

            var roleApiFunction_Method_GetLicenseFileInfos = new RoleApiFunction
            {
                Identifier = "Method.GetLicenseFileInfos",
                Desc = "GetLicenseFileInfos Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetLicenseFileInfos);

            var roleApiFunction_Method_GetRunningJobsList = new RoleApiFunction
            {
                Identifier = "Method.GetRunningJobsList",
                Desc = "GetRunningJobsList Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetRunningJobsList);

            var roleApiFunction_Method_GetJobProgress = new RoleApiFunction
            {
                Identifier = "Method.GetJobProgress",
                Desc = "GetJobProgress Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetJobProgress);

            var roleApiFunction_Method_CancelJob = new RoleApiFunction
            {
                Identifier = "Method.CancelJob",
                Desc = "CancelJob Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_CancelJob);

            var roleApiFunction_Method_ContinueJob = new RoleApiFunction
            {
                Identifier = "Method.ContinueJob",
                Desc = "ContinueJob Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_ContinueJob);

            var roleApiFunction_Method_GetCellInfos = new RoleApiFunction
            {
                Identifier = "Method.GetCellInfos",
                Desc = "GetCellInfos Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetCellInfos);

            var roleApiFunction_Method_GetResultCellInfos = new RoleApiFunction
            {
                Identifier = "Method.GetResultCellInfos",
                Desc = "GetResultCellInfos Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetResultCellInfos);

            var roleApiFunction_Method_InitializeProject = new RoleApiFunction
            {
                Identifier = "Method.InitializeProject",
                Desc = "InitializeProject Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_InitializeProject);

            var roleApiFunction_Method_GetCurrentUser = new RoleApiFunction
            {
                Identifier = "Method.GetCurrentUser",
                Desc = "GetCurrentUser Вызов",
                Modifier = "AllRoles"         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetCurrentUser);

            var roleApiFunction_Method_SetUserParamValue = new RoleApiFunction
            {
                Identifier = "Method.SetUserParamValue",
                Desc = "SetUserParamValue Вызов",
                Modifier = "AllRoles"         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_SetUserParamValue);

            var roleApiFunction_Method_GetUserParamValue = new RoleApiFunction
            {
                Identifier = "Method.GetUserParamValue",
                Desc = "GetUserParamValue Вызов",
                Modifier = "AllRoles"         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetUserParamValue);

            var roleApiFunction_Method_LogOutCurrentUser = new RoleApiFunction
            {
                Identifier = "Method.LogOutCurrentUser",
                Desc = "LogOutCurrentUser Вызов",
                Modifier = "Public"         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_LogOutCurrentUser);

            var roleApiFunction_Method_CalculateResults = new RoleApiFunction
            {
                Identifier = "Method.CalculateResults",
                Desc = "CalculateResults Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_CalculateResults);

            var roleApiFunction_Method_HasUnversionedChanges = new RoleApiFunction
            {
                Identifier = "Method.HasUnversionedChanges",
                Desc = "HasUnversionedChanges Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_HasUnversionedChanges);

            var roleApiFunction_Method_SaveUnversionedChanges = new RoleApiFunction
            {
                Identifier = "Method.SaveUnversionedChanges",
                Desc = "SaveUnversionedChanges Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_SaveUnversionedChanges);

            var roleApiFunction_Method_ClearUnversionedChanges = new RoleApiFunction
            {
                Identifier = "Method.ClearUnversionedChanges",
                Desc = "ClearUnversionedChanges Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_ClearUnversionedChanges);

            var roleApiFunction_Method_SetActiveProjectVersion = new RoleApiFunction
            {
                Identifier = "Method.SetActiveProjectVersion",
                Desc = "SetActiveProjectVersion Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_SetActiveProjectVersion);

            var roleApiFunction_Method_SetCurrentProjectVersion = new RoleApiFunction
            {
                Identifier = "Method.SetCurrentProjectVersion",
                Desc = "SetCurrentProjectVersion Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_SetCurrentProjectVersion);

            var roleApiFunction_Method_CompareVersions = new RoleApiFunction
            {
                Identifier = "Method.CompareVersions",
                Desc = "CompareVersions Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_CompareVersions);

            var roleApiFunction_Method_CompareVersions_CeMatrix = new RoleApiFunction
            {
                Identifier = "Method.CompareVersions_CeMatrix",
                Desc = "CompareVersions_CeMatrix Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_CompareVersions_CeMatrix);

            var roleApiFunction_Method_CompareVersions_Tag = new RoleApiFunction
            {
                Identifier = "Method.CompareVersions_Tag",
                Desc = "CompareVersions_Tag Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_CompareVersions_Tag);

            var roleApiFunction_Method_CompareVersions_BaseActuator = new RoleApiFunction
            {
                Identifier = "Method.CompareVersions_BaseActuator",
                Desc = "CompareVersions_BaseActuator Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_CompareVersions_BaseActuator);

            var roleApiFunction_Method_CompareVersions_SafetyController = new RoleApiFunction
            {
                Identifier = "Method.CompareVersions_SafetyController",
                Desc = "CompareVersions_SafetyController Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_CompareVersions_SafetyController);

            var roleApiFunction_Method_GetVersions_CeMatrix = new RoleApiFunction
            {
                Identifier = "Method.GetVersions_CeMatrix",
                Desc = "GetVersions_CeMatrix Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetVersions_CeMatrix);

            var roleApiFunction_Method_GetVersions_Tag = new RoleApiFunction
            {
                Identifier = "Method.GetVersions_Tag",
                Desc = "GetVersions_Tag Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetVersions_Tag);

            var roleApiFunction_Method_GetVersions_BaseActuator = new RoleApiFunction
            {
                Identifier = "Method.GetVersions_BaseActuator",
                Desc = "GetVersions_BaseActuator Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetVersions_BaseActuator);

            var roleApiFunction_Method_GetVersions_SafetyController = new RoleApiFunction
            {
                Identifier = "Method.GetVersions_SafetyController",
                Desc = "GetVersions_SafetyController Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetVersions_SafetyController);

            var roleApiFunction_Method_GetVersions_Legend = new RoleApiFunction
            {
                Identifier = "Method.GetVersions_Legend",
                Desc = "GetVersions_Legend Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_GetVersions_Legend);

            var roleApiFunction_Method_WidgetsViewer = new RoleApiFunction
            {
                Identifier = "Method.WidgetsViewer",
                Desc = "WidgetsViewer Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_WidgetsViewer);

            var roleApiFunction_Method_WidgetsLongRunningViewer = new RoleApiFunction
            {
                Identifier = "Method.WidgetsLongRunningViewer",
                Desc = "WidgetsLongRunningViewer Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_WidgetsLongRunningViewer);

            var roleApiFunction_Method_WidgetsEditor = new RoleApiFunction
            {
                Identifier = "Method.WidgetsEditor",
                Desc = "WidgetsEditor Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_WidgetsEditor);

            var roleApiFunction_Method_WidgetsAdmin = new RoleApiFunction
            {
                Identifier = "Method.WidgetsAdmin",
                Desc = "WidgetsAdmin Вызов",
                Modifier = ""         
            };
            dbContext.RoleApiFunctions.Add(roleApiFunction_Method_WidgetsAdmin);

            var roleBusinessFunction_View = new RoleBusinessFunction
            {
                Identifier = "View",
                Desc = "Просмотр, формирование отчетности"                
            };
            dbContext.RoleBusinessFunctions.Add(roleBusinessFunction_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_Unit_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_Unit_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_Project_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_Project_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersion_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersion_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionType_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionType_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionDbFileReference_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionDbFileReference_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrix_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrix_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixParam_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixParam_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixDbFileReference_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixDbFileReference_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_Row_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_Row_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_Column_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_Column_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_Cell_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_Cell_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixComment_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixComment_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_Tag_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_Tag_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_TagConditionInfo_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_TagConditionInfo_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_TagParam_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_TagParam_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_TagCondition_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_TagCondition_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_TagDbFileReference_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_TagDbFileReference_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuator_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuator_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorParam_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorParam_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorDbFileReference_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorDbFileReference_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyController_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyController_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerParam_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerParam_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerDbFileReference_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerDbFileReference_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_Legend_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_Legend_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_LegendDbFileReference_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_LegendDbFileReference_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_LegendParam_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_LegendParam_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEventsInterval_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEventsInterval_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEvent_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEvent_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObject_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObject_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventType_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventType_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectDbFileReference_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectDbFileReference_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_PcObject_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_PcObject_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectDbFileReference_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectDbFileReference_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEvent_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEvent_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventDbFileReference_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventDbFileReference_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_DbFile_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_DbFile_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_DbFileContent_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_DbFileContent_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_ParamInfo_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_ParamInfo_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_ParamDesc_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_ParamDesc_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_Result_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_Result_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_ResultEvent_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_ResultEvent_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixResult_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixResult_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_RowResult_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_RowResult_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_ColumnResult_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_ColumnResult_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_CellResult_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_CellResult_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_Job_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_Job_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_AddonStatus_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_UserEvent_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_UserEvent_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_InformationMessage_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_InformationMessage_Create);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_InformationMessage_Update);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_InformationMessage_Delete);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_InformationMessage_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_RequestMessage_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_RequestMessage_Create);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_RequestMessage_Update);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_RequestMessage_Delete);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_RequestMessage_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectJournalParam_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectJournalParam_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectJournalParam_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectJournalParam_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_JournalParamValuesCollection_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_JournalParamValuesCollection_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_FloatJournalParamValue_Read);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Entity_FloatJournalParamValue_View);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetFilterInfo);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_Filter);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetChildrenForSafetyIndex);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_ExportFiles);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_ExportFile);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetExportProjectFilesInfo);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetExportCeMatrixFilesInfo);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetExportTagFilesInfo);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetExportBaseActuatorFilesInfo);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetExportSafetyControllerFilesInfo);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetExportLegendFilesInfo);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_ExportProjectFile);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetExportEntitiesFilesInfo);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_ExportEntitiesFile);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetExportResultFilesInfo);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_ExportResultToFile);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetExportCeMatrixResultFilesInfo);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_ExportCeMatrixResultsToFile);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetExportVersionComparisonFilesInfo);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_ExportVersionComparisonToFile);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetExportEvents_FilesInfo);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_ExportEventsToFile);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetExportMonitoringWidgetsReport_FilesInfo);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_ExportMonitoringWidgetsReportToFile);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetExportMonitoringEventsReport_FilesInfo);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_ExportMonitoringEventsReportToFile);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetRunningJobsList);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetJobProgress);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetCellInfos);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetResultCellInfos);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_HasUnversionedChanges);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_CompareVersions);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_CompareVersions_CeMatrix);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_CompareVersions_Tag);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_CompareVersions_BaseActuator);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_CompareVersions_SafetyController);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetVersions_CeMatrix);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetVersions_Tag);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetVersions_BaseActuator);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetVersions_SafetyController);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_GetVersions_Legend);
                        
            roleBusinessFunction_View.RoleApiFunctions.Add(roleApiFunction_Method_WidgetsViewer);

            var roleBusinessFunction_WidgetsLongRunningView = new RoleBusinessFunction
            {
                Identifier = "WidgetsLongRunningView",
                Desc = "Долговременный просмотр приборных панелей"                
            };
            dbContext.RoleBusinessFunctions.Add(roleBusinessFunction_WidgetsLongRunningView);
                        
            roleBusinessFunction_WidgetsLongRunningView.RoleApiFunctions.Add(roleApiFunction_Entity_Unit_Read);
                        
            roleBusinessFunction_WidgetsLongRunningView.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObject_Read);
                        
            roleBusinessFunction_WidgetsLongRunningView.RoleApiFunctions.Add(roleApiFunction_Method_WidgetsViewer);
                        
            roleBusinessFunction_WidgetsLongRunningView.RoleApiFunctions.Add(roleApiFunction_Method_WidgetsLongRunningViewer);

            var roleBusinessFunction_Administrating = new RoleBusinessFunction
            {
                Identifier = "Administrating",
                Desc = "Администрирование"                
            };
            dbContext.RoleBusinessFunctions.Add(roleBusinessFunction_Administrating);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Unit_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Unit_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Unit_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Project_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Project_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Project_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersion_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionType_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionType_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionType_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionDbFileReference_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionDbFileReference_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionDbFileReference_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrix_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrix_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrix_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixParam_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixParam_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixParam_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixDbFileReference_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixDbFileReference_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixDbFileReference_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Row_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Row_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Row_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Column_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Column_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Column_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Cell_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Cell_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Cell_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixComment_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixComment_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixComment_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Tag_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Tag_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Tag_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_TagConditionInfo_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_TagConditionInfo_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_TagConditionInfo_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_TagParam_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_TagParam_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_TagParam_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_TagCondition_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_TagCondition_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_TagCondition_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_TagDbFileReference_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_TagDbFileReference_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_TagDbFileReference_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuator_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuator_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuator_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorParam_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorParam_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorParam_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorDbFileReference_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorDbFileReference_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorDbFileReference_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyController_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyController_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyController_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerParam_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerParam_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerParam_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerDbFileReference_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerDbFileReference_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerDbFileReference_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Legend_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Legend_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Legend_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_LegendDbFileReference_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_LegendDbFileReference_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_LegendDbFileReference_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_LegendParam_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_LegendParam_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_LegendParam_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEventsInterval_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEventsInterval_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEventsInterval_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEvent_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEvent_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEvent_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObject_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObject_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObject_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventType_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventType_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventType_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectDbFileReference_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectDbFileReference_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectDbFileReference_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_PcObject_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_PcObject_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_PcObject_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectDbFileReference_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectDbFileReference_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectDbFileReference_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEvent_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEvent_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEvent_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventDbFileReference_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventDbFileReference_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventDbFileReference_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_DbFile_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_DbFile_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_DbFile_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_DbFileContent_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_DbFileContent_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_DbFileContent_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_ParamInfo_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_ParamInfo_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_ParamInfo_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_ParamDesc_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_ParamDesc_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_ParamDesc_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Result_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Result_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Result_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_ResultEvent_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_ResultEvent_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_ResultEvent_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixResult_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixResult_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixResult_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_RowResult_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_RowResult_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_RowResult_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_ColumnResult_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_ColumnResult_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_ColumnResult_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_CellResult_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_CellResult_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_CellResult_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Job_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Job_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_Job_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_AddonStatus_View);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_UserEvent_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_UserEvent_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_UserEvent_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectJournalParam_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectJournalParam_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectJournalParam_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectJournalParam_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectJournalParam_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectJournalParam_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_JournalParamValuesCollection_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_JournalParamValuesCollection_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_JournalParamValuesCollection_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_FloatJournalParamValue_Create);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_FloatJournalParamValue_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_FloatJournalParamValue_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_LicenseFile_Read);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_LicenseFile_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_LicenseFile_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_LicenseFile_View);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_CryptoEntity_Read);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_CryptoEntity_Update);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_CryptoEntity_Delete);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Entity_CryptoEntity_View);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_GetAddonTestInfo);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_AddonTest);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_ReadConfiguration);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_WriteConfiguration);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_AddCryptoEntity);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_GetImportProjectFilesInfo);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_ImportProjectFiles);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_GetImportEntitiesFilesInfo);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_ImportEntitiesFiles);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_GetImportCeMatrixFilesInfo);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_ImportCeMatrixFiles);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_GetImportTagFilesInfo);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_ImportTagFiles);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_GetImportBaseActuatorFilesInfo);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_ImportBaseActuatorFiles);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_GetImportSafetyControllerFilesInfo);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_ImportSafetyControllerFiles);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_GetImportEventsJournalFilesInfo);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_ImportEventsJournalFiles);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_GetImportLicenseFilesInfo);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_ImportLicenseFiles);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_GetLicenseFileInfos);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_CancelJob);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_ContinueJob);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_InitializeProject);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_SaveUnversionedChanges);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_ClearUnversionedChanges);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_SetActiveProjectVersion);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_SetCurrentProjectVersion);
                        
            roleBusinessFunction_Administrating.RoleApiFunctions.Add(roleApiFunction_Method_WidgetsAdmin);

            var roleBusinessFunction_Projects_Edit = new RoleBusinessFunction
            {
                Identifier = "Projects_Edit",
                Desc = "Изменение проектов справочника элементов ПАЗ"                
            };
            dbContext.RoleBusinessFunctions.Add(roleBusinessFunction_Projects_Edit);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_Project_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_Project_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersion_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionType_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionType_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionType_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionDbFileReference_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionDbFileReference_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionDbFileReference_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrix_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrix_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrix_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixParam_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixParam_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixParam_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixDbFileReference_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixDbFileReference_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixDbFileReference_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_Row_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_Row_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_Row_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_Column_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_Column_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_Column_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_Cell_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_Cell_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_Cell_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixComment_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixComment_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixComment_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_Tag_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_Tag_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_Tag_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_TagConditionInfo_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_TagConditionInfo_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_TagConditionInfo_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_TagParam_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_TagParam_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_TagParam_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_TagCondition_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_TagCondition_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_TagCondition_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_TagDbFileReference_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_TagDbFileReference_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_TagDbFileReference_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuator_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuator_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuator_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorParam_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorParam_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorParam_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorDbFileReference_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorDbFileReference_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorDbFileReference_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyController_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyController_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyController_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerParam_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerParam_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerParam_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerDbFileReference_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerDbFileReference_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerDbFileReference_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_Legend_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_Legend_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_Legend_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_LegendDbFileReference_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_LegendDbFileReference_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_LegendDbFileReference_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_LegendParam_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_LegendParam_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_LegendParam_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_DbFile_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_DbFile_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_DbFile_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_DbFileContent_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_DbFileContent_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_DbFileContent_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_ParamInfo_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_ParamInfo_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_ParamInfo_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_ParamDesc_Create);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_ParamDesc_Update);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_ParamDesc_Delete);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Method_GetImportProjectFilesInfo);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Method_ImportProjectFiles);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Method_GetImportCeMatrixFilesInfo);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Method_ImportCeMatrixFiles);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Method_GetImportTagFilesInfo);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Method_ImportTagFiles);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Method_GetImportBaseActuatorFilesInfo);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Method_ImportBaseActuatorFiles);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Method_GetImportSafetyControllerFilesInfo);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Method_ImportSafetyControllerFiles);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Method_CancelJob);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Method_ContinueJob);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Method_InitializeProject);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Method_SaveUnversionedChanges);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Method_ClearUnversionedChanges);
                        
            roleBusinessFunction_Projects_Edit.RoleApiFunctions.Add(roleApiFunction_Method_SetCurrentProjectVersion);

            var roleBusinessFunction_Projects_Supervise = new RoleBusinessFunction
            {
                Identifier = "Projects_Supervise",
                Desc = "Авторизация изменений в проектах справочника элементов ПАЗ"                
            };
            dbContext.RoleBusinessFunctions.Add(roleBusinessFunction_Projects_Supervise);
                        
            roleBusinessFunction_Projects_Supervise.RoleApiFunctions.Add(roleApiFunction_Entity_Unit_Update);
                        
            roleBusinessFunction_Projects_Supervise.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersion_Update);
                        
            roleBusinessFunction_Projects_Supervise.RoleApiFunctions.Add(roleApiFunction_Method_CancelJob);
                        
            roleBusinessFunction_Projects_Supervise.RoleApiFunctions.Add(roleApiFunction_Method_ContinueJob);
                        
            roleBusinessFunction_Projects_Supervise.RoleApiFunctions.Add(roleApiFunction_Method_SetActiveProjectVersion);

            var roleBusinessFunction_Diagnost_EditUnitEvents = new RoleBusinessFunction
            {
                Identifier = "Diagnost_EditUnitEvents",
                Desc = "Загрузка логов технологшических событий"                
            };
            dbContext.RoleBusinessFunctions.Add(roleBusinessFunction_Diagnost_EditUnitEvents);
                        
            roleBusinessFunction_Diagnost_EditUnitEvents.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEventsInterval_Create);
                        
            roleBusinessFunction_Diagnost_EditUnitEvents.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEventsInterval_Update);
                        
            roleBusinessFunction_Diagnost_EditUnitEvents.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEventsInterval_Delete);
                        
            roleBusinessFunction_Diagnost_EditUnitEvents.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEvent_Create);
                        
            roleBusinessFunction_Diagnost_EditUnitEvents.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEvent_Update);
                        
            roleBusinessFunction_Diagnost_EditUnitEvents.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEvent_Delete);
                        
            roleBusinessFunction_Diagnost_EditUnitEvents.RoleApiFunctions.Add(roleApiFunction_Method_GetImportEventsJournalFilesInfo);
                        
            roleBusinessFunction_Diagnost_EditUnitEvents.RoleApiFunctions.Add(roleApiFunction_Method_ImportEventsJournalFiles);
                        
            roleBusinessFunction_Diagnost_EditUnitEvents.RoleApiFunctions.Add(roleApiFunction_Method_CancelJob);
                        
            roleBusinessFunction_Diagnost_EditUnitEvents.RoleApiFunctions.Add(roleApiFunction_Method_ContinueJob);

            var roleBusinessFunction_Diagnost_EditCalculationResults = new RoleBusinessFunction
            {
                Identifier = "Diagnost_EditCalculationResults",
                Desc = "Редактирование реестра результатов анализа остановов"                
            };
            dbContext.RoleBusinessFunctions.Add(roleBusinessFunction_Diagnost_EditCalculationResults);
                        
            roleBusinessFunction_Diagnost_EditCalculationResults.RoleApiFunctions.Add(roleApiFunction_Entity_Result_Create);
                        
            roleBusinessFunction_Diagnost_EditCalculationResults.RoleApiFunctions.Add(roleApiFunction_Entity_Result_Update);
                        
            roleBusinessFunction_Diagnost_EditCalculationResults.RoleApiFunctions.Add(roleApiFunction_Entity_Result_Delete);
                        
            roleBusinessFunction_Diagnost_EditCalculationResults.RoleApiFunctions.Add(roleApiFunction_Entity_ResultEvent_Create);
                        
            roleBusinessFunction_Diagnost_EditCalculationResults.RoleApiFunctions.Add(roleApiFunction_Entity_ResultEvent_Update);
                        
            roleBusinessFunction_Diagnost_EditCalculationResults.RoleApiFunctions.Add(roleApiFunction_Entity_ResultEvent_Delete);
                        
            roleBusinessFunction_Diagnost_EditCalculationResults.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixResult_Create);
                        
            roleBusinessFunction_Diagnost_EditCalculationResults.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixResult_Update);
                        
            roleBusinessFunction_Diagnost_EditCalculationResults.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixResult_Delete);
                        
            roleBusinessFunction_Diagnost_EditCalculationResults.RoleApiFunctions.Add(roleApiFunction_Entity_RowResult_Create);
                        
            roleBusinessFunction_Diagnost_EditCalculationResults.RoleApiFunctions.Add(roleApiFunction_Entity_RowResult_Update);
                        
            roleBusinessFunction_Diagnost_EditCalculationResults.RoleApiFunctions.Add(roleApiFunction_Entity_RowResult_Delete);
                        
            roleBusinessFunction_Diagnost_EditCalculationResults.RoleApiFunctions.Add(roleApiFunction_Entity_ColumnResult_Create);
                        
            roleBusinessFunction_Diagnost_EditCalculationResults.RoleApiFunctions.Add(roleApiFunction_Entity_ColumnResult_Update);
                        
            roleBusinessFunction_Diagnost_EditCalculationResults.RoleApiFunctions.Add(roleApiFunction_Entity_ColumnResult_Delete);
                        
            roleBusinessFunction_Diagnost_EditCalculationResults.RoleApiFunctions.Add(roleApiFunction_Entity_CellResult_Create);
                        
            roleBusinessFunction_Diagnost_EditCalculationResults.RoleApiFunctions.Add(roleApiFunction_Entity_CellResult_Update);
                        
            roleBusinessFunction_Diagnost_EditCalculationResults.RoleApiFunctions.Add(roleApiFunction_Entity_CellResult_Delete);
                        
            roleBusinessFunction_Diagnost_EditCalculationResults.RoleApiFunctions.Add(roleApiFunction_Method_CancelJob);
                        
            roleBusinessFunction_Diagnost_EditCalculationResults.RoleApiFunctions.Add(roleApiFunction_Method_ContinueJob);

            var roleBusinessFunction_Diagnost_Calculate = new RoleBusinessFunction
            {
                Identifier = "Diagnost_Calculate",
                Desc = "Анализ прошедших остановов"                
            };
            dbContext.RoleBusinessFunctions.Add(roleBusinessFunction_Diagnost_Calculate);
                        
            roleBusinessFunction_Diagnost_Calculate.RoleApiFunctions.Add(roleApiFunction_Method_CancelJob);
                        
            roleBusinessFunction_Diagnost_Calculate.RoleApiFunctions.Add(roleApiFunction_Method_ContinueJob);
                        
            roleBusinessFunction_Diagnost_Calculate.RoleApiFunctions.Add(roleApiFunction_Method_CalculateResults);

            var roleBusinessFunction_Monitoring_Edit = new RoleBusinessFunction
            {
                Identifier = "Monitoring_Edit",
                Desc = "Модуль Мониторинг: добавлние/редактирование данных об объектах, комментирование, загрузка сопутствующих документов"                
            };
            dbContext.RoleBusinessFunctions.Add(roleBusinessFunction_Monitoring_Edit);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObject_Create);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObject_Update);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObject_Delete);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventType_Create);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventType_Update);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventType_Delete);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectDbFileReference_Create);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectDbFileReference_Update);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectDbFileReference_Delete);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_PcObject_Create);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_PcObject_Update);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_PcObject_Delete);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectDbFileReference_Create);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectDbFileReference_Update);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectDbFileReference_Delete);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEvent_Create);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEvent_Update);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEvent_Delete);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventDbFileReference_Create);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventDbFileReference_Update);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventDbFileReference_Delete);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_DbFile_Create);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_DbFile_Update);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_DbFile_Delete);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_DbFileContent_Create);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_DbFileContent_Update);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_DbFileContent_Delete);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_ParamInfo_Create);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_ParamInfo_Update);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_ParamInfo_Delete);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_ParamDesc_Create);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_ParamDesc_Update);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_ParamDesc_Delete);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectJournalParam_Create);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectJournalParam_Update);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectJournalParam_Delete);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectJournalParam_Create);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectJournalParam_Update);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectJournalParam_Delete);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_JournalParamValuesCollection_Create);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_JournalParamValuesCollection_Update);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_JournalParamValuesCollection_Delete);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_FloatJournalParamValue_Create);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_FloatJournalParamValue_Update);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_FloatJournalParamValue_Delete);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Method_AddResultToPcObject);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Method_CancelJob);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Method_ContinueJob);
                        
            roleBusinessFunction_Monitoring_Edit.RoleApiFunctions.Add(roleApiFunction_Method_WidgetsEditor);

            var roleBusinessFunction_InformationSecurityEvents_View = new RoleBusinessFunction
            {
                Identifier = "InformationSecurityEvents_View",
                Desc = "Просмотр логов событий ИБ"                
            };
            dbContext.RoleBusinessFunctions.Add(roleBusinessFunction_InformationSecurityEvents_View);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_Unit_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_Project_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersion_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionType_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_ProjectVersionDbFileReference_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrix_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixParam_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixDbFileReference_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_Row_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_Column_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_Cell_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixComment_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_Tag_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_TagConditionInfo_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_TagParam_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_TagCondition_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_TagDbFileReference_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuator_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorParam_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_BaseActuatorDbFileReference_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyController_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerParam_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_SafetyControllerDbFileReference_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_Legend_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_LegendDbFileReference_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_LegendParam_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEventsInterval_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_UnitEvent_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObject_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventType_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectDbFileReference_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_PcObject_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectDbFileReference_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEvent_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectEventDbFileReference_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_DbFile_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_DbFileContent_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_ParamInfo_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_ParamDesc_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_Result_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_ResultEvent_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_CeMatrixResult_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_RowResult_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_ColumnResult_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_CellResult_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_Job_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_AddonStatus_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_UserEvent_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_InformationMessage_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_InformationMessage_View);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_RequestMessage_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_RequestMessage_View);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_BasePcObjectJournalParam_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_PcObjectJournalParam_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_JournalParamValuesCollection_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_FloatJournalParamValue_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_InformationSecurityEvent_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_InformationSecurityEvent_View);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_LicenseFile_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Entity_CryptoEntity_Read);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Method_GetFilterInfo);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Method_Filter);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Method_GetChildrenForSafetyIndex);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Method_GetExportEvents_FilesInfo);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Method_ExportEventsToFile);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Method_GetExportMonitoringWidgetsReport_FilesInfo);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Method_ExportMonitoringWidgetsReportToFile);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Method_GetExportMonitoringEventsReport_FilesInfo);
                        
            roleBusinessFunction_InformationSecurityEvents_View.RoleApiFunctions.Add(roleApiFunction_Method_ExportMonitoringEventsReportToFile);

            var roleBusinessFunction_Roles_View = new RoleBusinessFunction
            {
                Identifier = "Roles_View",
                Desc = "Просмотр настроек ролей пользователей"                
            };
            dbContext.RoleBusinessFunctions.Add(roleBusinessFunction_Roles_View);
                        
            roleBusinessFunction_Roles_View.RoleApiFunctions.Add(roleApiFunction_Entity_Role_View);
                        
            roleBusinessFunction_Roles_View.RoleApiFunctions.Add(roleApiFunction_Entity_RolePermission_View);
                        
            roleBusinessFunction_Roles_View.RoleApiFunctions.Add(roleApiFunction_Entity_RoleBusinessFunction_View);
                        
            roleBusinessFunction_Roles_View.RoleApiFunctions.Add(roleApiFunction_Entity_RoleApiFunction_View);

            var roleBusinessFunction_Roles_Edit = new RoleBusinessFunction
            {
                Identifier = "Roles_Edit",
                Desc = "Настройка ролей пользователей"                
            };
            dbContext.RoleBusinessFunctions.Add(roleBusinessFunction_Roles_Edit);
                        
            roleBusinessFunction_Roles_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_Role_Create);
                        
            roleBusinessFunction_Roles_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_Role_Update);
                        
            roleBusinessFunction_Roles_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_Role_Delete);
                        
            roleBusinessFunction_Roles_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_RolePermission_Create);
                        
            roleBusinessFunction_Roles_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_RolePermission_Update);
                        
            roleBusinessFunction_Roles_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_RolePermission_Delete);
                        
            roleBusinessFunction_Roles_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_RoleBusinessFunction_Create);
                        
            roleBusinessFunction_Roles_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_RoleBusinessFunction_Update);
                        
            roleBusinessFunction_Roles_Edit.RoleApiFunctions.Add(roleApiFunction_Entity_RoleBusinessFunction_Delete);

            Role? role;

            RolePermission rolePermission;

            role = dbContext.Roles.FirstOrDefault(r => r.Identifier == "mpaz-admin-test");
            if (role is null)
            {
                role = new Role();                
                dbContext.Roles.Add(role);
            }

            role.Identifier = "mpaz-admin-test";
            role.Desc = "Администратор";            


            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_View,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_WidgetsLongRunningView,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Administrating,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Projects_Edit,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Projects_Supervise,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Diagnost_EditUnitEvents,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Diagnost_EditCalculationResults,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Diagnost_Calculate,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Monitoring_Edit,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_InformationSecurityEvents_View,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Roles_View,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Roles_Edit,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            role = dbContext.Roles.FirstOrDefault(r => r.Identifier == "mpaz-adminIB-test");
            if (role is null)
            {
                role = new Role();                
                dbContext.Roles.Add(role);
            }

            role.Identifier = "mpaz-adminIB-test";
            role.Desc = "Администратор ИБ";            


            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_View,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_WidgetsLongRunningView,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Administrating,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Projects_Edit,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Projects_Supervise,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Diagnost_EditUnitEvents,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Diagnost_EditCalculationResults,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Diagnost_Calculate,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Monitoring_Edit,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_InformationSecurityEvents_View,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Roles_View,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Roles_Edit,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            role = dbContext.Roles.FirstOrDefault(r => r.Identifier == "mpaz-engineer-test");
            if (role is null)
            {
                role = new Role();                
                dbContext.Roles.Add(role);
            }

            role.Identifier = "mpaz-engineer-test";
            role.Desc = "Инженер";            


            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_View,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_WidgetsLongRunningView,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Administrating,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Projects_Edit,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Projects_Supervise,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Diagnost_EditUnitEvents,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Diagnost_EditCalculationResults,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Diagnost_Calculate,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Monitoring_Edit,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_InformationSecurityEvents_View,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Roles_View,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Roles_Edit,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            role = dbContext.Roles.FirstOrDefault(r => r.Identifier == "mpaz-supervisor-test");
            if (role is null)
            {
                role = new Role();                
                dbContext.Roles.Add(role);
            }

            role.Identifier = "mpaz-supervisor-test";
            role.Desc = "Супервайзер";            


            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_View,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_WidgetsLongRunningView,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Administrating,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Projects_Edit,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Projects_Supervise,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Diagnost_EditUnitEvents,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Diagnost_EditCalculationResults,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Diagnost_Calculate,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Monitoring_Edit,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_InformationSecurityEvents_View,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Roles_View,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Roles_Edit,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            role = dbContext.Roles.FirstOrDefault(r => r.Identifier == "mpaz-viewer-test");
            if (role is null)
            {
                role = new Role();                
                dbContext.Roles.Add(role);
            }

            role.Identifier = "mpaz-viewer-test";
            role.Desc = "Обозреватель";            


            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_View,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_WidgetsLongRunningView,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Administrating,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Projects_Edit,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Projects_Supervise,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Diagnost_EditUnitEvents,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Diagnost_EditCalculationResults,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Diagnost_Calculate,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Monitoring_Edit,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_InformationSecurityEvents_View,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Roles_View,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Roles_Edit,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            role = dbContext.Roles.FirstOrDefault(r => r.Identifier == "mpaz-longrunningviewer-test");
            if (role is null)
            {
                role = new Role();                
                dbContext.Roles.Add(role);
            }

            role.Identifier = "mpaz-longrunningviewer-test";
            role.Desc = "Наблюдатель";            


            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_View,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_WidgetsLongRunningView,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Administrating,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Projects_Edit,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Projects_Supervise,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Diagnost_EditUnitEvents,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Diagnost_EditCalculationResults,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Diagnost_Calculate,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Monitoring_Edit,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_InformationSecurityEvents_View,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Roles_View,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Roles_Edit,          
                IsAllowed = false
            };
            role.RolePermissions.Add(rolePermission);

            role = dbContext.Roles.FirstOrDefault(r => r.Identifier == "SuperUser");
            if (role is null)
            {
                role = new Role();                
                dbContext.Roles.Add(role);
            }

            role.Identifier = "SuperUser";
            role.Desc = "Cуперпользователь";            


            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_View,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_WidgetsLongRunningView,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Administrating,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Projects_Edit,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Projects_Supervise,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Diagnost_EditUnitEvents,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Diagnost_EditCalculationResults,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Diagnost_Calculate,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Monitoring_Edit,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_InformationSecurityEvents_View,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Roles_View,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            rolePermission = new RolePermission
            {
                RoleBusinessFunction = roleBusinessFunction_Roles_Edit,          
                IsAllowed = true
            };
            role.RolePermissions.Add(rolePermission);

            dbContext.SaveChanges();
            #endregion
        }

        #endregion   
    }   
}
