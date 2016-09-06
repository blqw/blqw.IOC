using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace blqw.IOC
{
    /// <summary>
    /// 简单日志接口
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="eventType"> 日志类型 </param>
        /// <param name="message"> 日志消息 </param>
        /// <param name="ex"> 异常对象 </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="message" /> 为 null。 </exception>
        /// <exception cref="ArgumentException"> <paramref name="message" /> 为空字符串 ("")。 </exception>
        void Write(TraceEventType eventType, string message, Exception ex = null, [CallerLineNumber] int line = 0,
            [CallerMemberName] string member = "",
            [CallerFilePath] string file = "");

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="eventType"> 日志类型 </param>
        /// <param name="getMessage"> 用户获取日志的委托 </param>
        /// <param name="ex"> </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="getMessage" /> 为 null。 </exception>
        /// <exception cref="ArgumentException"> <paramref name="getMessage" /> 返回值为null或空字符串("")。 </exception>
        /// <exception cref="Exception"> <paramref name="getMessage" /> 执行返回异常。 </exception>
        void Write(TraceEventType eventType, Func<string> getMessage, Exception ex = null,
            [CallerLineNumber] int line = 0, [CallerMemberName] string member = "",
            [CallerFilePath] string file = "");
    }
}