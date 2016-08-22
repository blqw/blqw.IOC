using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blqw.IOC
{
    /// <summary>
    /// 默认日志记录器
    /// </summary>
    public sealed class DefaultLogger : ILogger
    {
        /// <summary>
        /// 日志跟踪
        /// </summary>
        private readonly TraceSource _trace = new TraceSource("blqw.IOC", SourceLevels.All);

        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="id"> </param>
        /// <param name="message"></param>
        /// <exception cref="ObjectDisposedException">终止期间尝试跟踪事件。</exception>
        public void Debug(int id, string message)
        {
            _trace.TraceEvent(TraceEventType.Verbose, id, message);
        }

        /// <summary>
        /// 提示日志
        /// </summary>
        /// <param name="id"> </param>
        /// <param name="message"></param>
        /// <exception cref="ObjectDisposedException">终止期间尝试跟踪事件。</exception>
        public void Information(int id, string message)
        {
            _trace.TraceEvent(TraceEventType.Information, id, message);
        }

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="id"> </param>
        /// <param name="message"></param>
        /// <exception cref="ObjectDisposedException">终止期间尝试跟踪事件。</exception>
        public void Warning(int id, string message)
        {
            _trace.TraceEvent(TraceEventType.Warning, id, message);
        }

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="id"> </param>
        /// <param name="message"></param>
        /// <param name="ex"> </param>
        /// <exception cref="ObjectDisposedException">终止期间尝试跟踪事件。</exception>
        public void Error(int id, string message, Exception ex)
        {
            _trace.TraceEvent(TraceEventType.Error, id, message);
        }

    }
}
