using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blqw.IOC
{
    /// <summary>
    /// 日志等级筛选,兼容 <seealso cref="System.Diagnostics.TraceEventType"/> 与 <seealso cref="System.Diagnostics.SourceLevels"/>,
    /// 并提供更多关键字
    /// </summary>
    [Flags]
    public enum LogLevel
    {

        /// <summary>
        /// 只允许 <see cref="System.Diagnostics.TraceEventType.Error"/> 事件通过。
        /// </summary>
        _Error = 2,
        /// <summary>
        /// 同 <see cref="_Error"/>。
        /// </summary>
        _Fail = 2,
        /// <summary>
        /// 只允许 <see cref="System.Diagnostics.TraceEventType.Warning"/> 事件通过。
        /// </summary>
        _Warning = 4,
        /// <summary>
        /// 同 <see cref="_Warning"/>。
        /// </summary>
        _Warn = 4,
        /// <summary>
        /// 只允许 <see cref="System.Diagnostics.TraceEventType.Information"/> 事件通过。
        /// </summary>
        _Information = 8,
        /// <summary>
        /// 同 <see cref="_Information"/>。
        /// </summary>
        _Info = 8,
        /// <summary>
        /// 只允许 <see cref="System.Diagnostics.TraceEventType.Verbose"/> 事件通过。
        /// </summary>
        _Verbose = 16,
        /// <summary>
        /// 同 <see cref="_Verbose"/>。
        /// </summary>
        _Debug = 16,
        /// <summary>
        /// 同 <see cref="_Verbose"/>。
        /// </summary>
        _Test = 16,
        /// <summary>
        /// 允许所有事件通过。
        /// </summary>
        All = -1,
        /// <summary>
        /// 不允许任何事件通过。
        /// </summary>
        Off = 0,
        /// <summary>
        /// 同 <see cref="Off"/>。
        /// </summary>
        Close = Off,
        /// <summary>
        /// 同 <see cref="Off"/>。
        /// </summary>
        None = Off,
        /// <summary>
        /// 只允许 <see cref="System.Diagnostics.TraceEventType.Critical"/> 事件通过。
        /// </summary>
        Critical = 1,
        /// <summary>
        /// 同 <see cref="Critical"/>。
        /// </summary>
        Block = Critical,
        /// <summary>
        /// 允许 <see cref="System.Diagnostics.TraceEventType.Critical"/> 和 <see cref="System.Diagnostics.TraceEventType.Error"/> 事件通过。
        /// </summary>
        Error = 3,
        /// <summary>
        /// 同 <see cref="Error"/>。
        /// </summary>
        Fail = Error,
        /// <summary>
        /// 允许 <see cref="System.Diagnostics.TraceEventType.Critical"/>、<see cref="System.Diagnostics.TraceEventType.Error"/> 
        /// 和 <see cref="System.Diagnostics.TraceEventType.Warning"/> 事件通过。
        /// </summary>
        Warning = 7,
        /// <summary>
        /// 同 <see cref="Warning"/>。
        /// </summary>
        Warn = Warning,
        /// <summary>
        /// 允许 <see cref="System.Diagnostics.TraceEventType.Critical"/>、<see cref="System.Diagnostics.TraceEventType.Error"/>、
        /// <see cref="System.Diagnostics.TraceEventType.Warning"/> 和 <see cref="System.Diagnostics.TraceEventType.Information"/> 事件通过。
        /// </summary>
        Information = 15,
        /// <summary>
        /// 同 <see cref="Info"/>。
        /// </summary>
        Info = Information,
        /// <summary>
        /// 允许 <see cref="System.Diagnostics.TraceEventType.Critical"/>、<see cref="System.Diagnostics.TraceEventType.Error"/>、
        /// <see cref="System.Diagnostics.TraceEventType.Warning"/>、<see cref="System.Diagnostics.TraceEventType.Information"/>
        /// 和 <see cref="System.Diagnostics.TraceEventType.Verbose"/> 事件通过。
        /// </summary>
        Verbose = 31,
        /// <summary>
        /// 同 <see cref="Verbose"/>。
        /// </summary>
        Debug = Verbose,
        /// <summary>
        /// 同 <see cref="Debug"/>。
        /// </summary>
        Test = Verbose,
        /// <summary>
        /// 只允许 <see cref="System.Diagnostics.TraceEventType.Start"/> 事件通过。
        /// </summary>
        Start = 256,
        /// <summary>
        /// 只允许 <see cref="System.Diagnostics.TraceEventType.Stop"/> 事件通过。
        /// </summary>
        Stop = 512,
        /// <summary>
        /// 只允许 <see cref="System.Diagnostics.TraceEventType.Suspend"/> 事件通过。
        /// </summary>
        Suspend = 1024,
        /// <summary>
        /// 只允许 <see cref="System.Diagnostics.TraceEventType.Resume"/> 事件通过。
        /// </summary>
        Resume = 2048,
        /// <summary>
        /// 只允许 <see cref="System.Diagnostics.TraceEventType.Transfer"/> 事件通过。
        /// </summary>
        Transfer = 4096,
        /// <summary>
        /// 允许 <see cref="System.Diagnostics.TraceEventType.Stop"/>、<see cref="System.Diagnostics.TraceEventType.Start"/>、
        /// <see cref="System.Diagnostics.TraceEventType.Suspend"/>、<see cref="System.Diagnostics.TraceEventType.Transfer"/>
        /// 和 <see cref="System.Diagnostics.TraceEventType.Resume"/> 事件通过。
        /// </summary>
        ActivityTracing = 65280
    }

}
