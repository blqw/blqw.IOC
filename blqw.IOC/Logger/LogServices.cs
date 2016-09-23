using System.Diagnostics;

namespace blqw.IOC
{
    /// <summary>
    /// MEF日志记录器静态类
    /// </summary>
    public static class LogServices
    {
        /// <summary>
        /// 全局日志记录器, 默认级别: <see cref="SourceLevels.Warning"/> | <see cref="SourceLevels.ActivityTracing"/>
        /// </summary>
        public static TraceSource Logger { get; set; } = new TraceSource("blqw.IOC", SourceLevels.Warning | SourceLevels.ActivityTracing).Initialize();
    }
}