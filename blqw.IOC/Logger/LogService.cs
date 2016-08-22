using System;
using System.Collections.Generic;
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
        /// 全局日志记录器
        /// </summary>
        public static ILogger Logger { get; set; } = new DefaultLogger();
    }
}
