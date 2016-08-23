using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blqw.IOC
{
    /// <summary>
    /// MEF日志记录器静态类
    /// </summary>
    public static class LogService
    {
        /// <summary>
        /// 全局日志记录器, 默认级别: 警告
        /// </summary>
        public static ILogger Logger { get; set; } = new TraceLogger("blqw.IOC", SourceLevels.Warning);
    }
}
