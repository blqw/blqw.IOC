using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace blqw.IOC
{
    /// <summary>
    /// 一组 <seealso cref="TraceSource" /> 的扩展方法
    /// </summary>
    public static class TraceSourceExtensions
    {
        static TraceSourceExtensions()
        {
            DebuggerIfAttached(Trace.Listeners);
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="source"> </param>
        /// <param name="eventType"> 日志类型 </param>
        /// <param name="message"> 日志消息 </param>
        /// <param name="ex"> 异常对象 </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="message" /> 为 null 。 </exception>
        /// <exception cref="ArgumentException"> <paramref name="message" /> 为空字符串 ("")或连续的空白。 </exception>
        /// <exception cref="ObjectDisposedException"> 终止期间尝试跟踪事件。 </exception>
        public static void Write(this TraceSource source, TraceEventType eventType, string message, Exception ex = null,
            [CallerLineNumber] int line = 0, [CallerMemberName] string member = "", [CallerFilePath] string file = "")
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException($"{nameof(message)}为空字符串 (\"\")或连续的空白。", nameof(message));
            }
            if (ex == null)
            {
                source.TraceData(eventType, 0, message);
            }
            else
            {
                source.TraceData(eventType, 0, message, $"{file}:{line},{member}", ex.ToString());
            }
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="source"> </param>
        /// <param name="eventType"> 日志类型 </param>
        /// <param name="getMessage"> 用户获取日志的委托 </param>
        /// <param name="ex"> </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="getMessage" /> 为 null。 </exception>
        /// <exception cref="ArgumentException"> <paramref name="getMessage" /> 返回值为null或连续的空白。 </exception>
        /// <exception cref="Exception"> <paramref name="getMessage" /> 执行出现异常。 </exception>
        /// <exception cref="ObjectDisposedException"> 终止期间尝试跟踪事件。 </exception>
        public static void Write(this TraceSource source, TraceEventType eventType, Func<string> getMessage, Exception ex = null,
            [CallerLineNumber] int line = 0, [CallerMemberName] string member = "", [CallerFilePath] string file = "")
        {
            if (getMessage == null)
            {
                throw new ArgumentNullException(nameof(getMessage));
            }

            if (source.Switch.ShouldTrace(eventType) == false)
            {
                return;
            }

            var message = getMessage();
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException($"{nameof(getMessage)}返回值为null或连续的空白。", nameof(getMessage));
            }

            if (ex == null)
            {
                source.TraceData(eventType, 0, message);
            }
            else
            {
                source.TraceData(eventType, 0, message, $"{file}:{line},{member}", ex.ToString());
            }
        }

        /// <summary>
        /// 同步 <seealso cref="Trace.Listeners" /> 和 <seealso cref="TraceSourceBase.Listeners" /> 中的对象
        /// 如果无法同步,则进行复制
        /// </summary>
        private static void SyncTraceListeners(TraceSource source)
        {
            //如果只有一个监听器 且是默认的监听器,则同步 Trace.Listeners
            if ((source.Listeners.Count != 1) || (source.Listeners[0] is DefaultTraceListener == false))
            {
                return;
            }
            var field = typeof(TraceSource).GetField("listeners", BindingFlags.Instance | BindingFlags.NonPublic)
                        ?? typeof(TraceSource)
                            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                            .FirstOrDefault(it => it.FieldType == typeof(TraceListenerCollection));
            if (field?.IsLiteral == false)
            {
                try
                {
                    field.SetValue(source, Trace.Listeners);
                    return;
                }
                catch
                {
                    // ignored
                }
            }

            source.Listeners.Clear();
            source.Listeners.AddRange(Trace.Listeners);
        }

        /// <summary>
        /// 如果处于调试器附加进程状态,则追加调试日志侦听器
        /// </summary>
        private static void DebuggerIfAttached(TraceListenerCollection listeners)
        {
            //非调试模式
            if (Debugger.IsAttached == false)
            {
                return;
            }

            //winform 如果存在 ConsoleTraceListener 不附加 DefaultTraceListener
            if (PlatformServices.IsWinForm && listeners.OfType<ConsoleTraceListener>().Any())
            {
                return;
            }

            if (listeners.OfType<DefaultTraceListener>().Any() == false)
            {
                //可以在"输出"窗口看到所有的日志
                listeners.Add(new DefaultTraceListener());
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="source"> </param>
        /// <returns> </returns>
        public static TraceSource Initialize(this TraceSource source)
        {
            SyncTraceListeners(source);
            DebuggerIfAttached(source.Listeners);
            return source;
        }
    }
}