using System;

namespace Simcode.PazCheck.CentralServer.Common
{
    public class ProjectEntitiesCollectionInfo
    {
        public int ProjectId { get; set; }

        /// <summary>
        ///     null or 0 for unsaved version
        /// </summary>
        public UInt32? ProjectVersionNum { get; set; }

        public EntitiesCollectionInfo? CeMatrices { get; set; }

        public EntitiesCollectionInfo? Tags { get; set; }

        public EntitiesCollectionInfo? BaseActuators { get; set; }

        public EntitiesCollectionInfo? SafetyControllers { get; set; }

        public EntitiesCollectionInfo? Legends { get; set; }

        public bool FullExport_ProjectVersion { get; set; }
    }
}
