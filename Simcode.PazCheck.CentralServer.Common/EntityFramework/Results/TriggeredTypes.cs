// Copyright (c) 2021
// All rights reserved by Simcode

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public enum TriggeredTypes
    {
        /// 0 - не должен был и не сработал
        NotActivated = 0,
        ///1 - должен был и сработал во время
        SuccessFirstTriggered,
        ///2 - должен был и сработал во время
        SuccessTriggered,
        ///3 - должен был и сработал с задержкой
        LateTriggered,
        ///4 - должен был, но не сработал
        NotTriggered,
        ///5 - не должен был, но сработал
        FaultTriggered
    }
}
