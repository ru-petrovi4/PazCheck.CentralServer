

namespace Simcode.PazCheck.CentralServer.Common.EntityFramework
{
    public enum TriggeredType
    {
        /// <summary>
        ///     0 - не должен был и не сработал
        /// </summary>
        NotActivated = 0,

        /// <summary>
        ///     1 - должен был, но сработал раньше
        /// </summary>
        SuccessFirstTriggered,

        /// <summary>
        ///     2 - должен был и сработал во время
        /// </summary>
        SuccessTriggered,

        /// <summary>
        ///     3 - должен был и сработал с задержкой
        /// </summary>
        LateTriggered,

        /// <summary>
        ///     4 - должен был, но не сработал
        /// </summary>
        NotTriggered,

        /// <summary>
        ///     5 - не должен был, но сработал
        /// </summary>
        FaultTriggered
    }
}
