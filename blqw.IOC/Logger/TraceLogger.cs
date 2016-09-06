using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace blqw.IOC
{
    /// <summary>
    /// 默认日志记录器
    /// </summary>
    public class TraceLogger : TraceSource, ILogger
    {
        private readonly NameFilter _filter;
        private readonly LogLevel _level;

        /// <summary>
        /// 使用指定的源名称初始化 <see cref="T:System.Diagnostics.TraceSource" /> 类的新实例。
        /// </summary>
        /// <param name="name"> 源的名称，通常为应用程序的名称。 </param>
        /// <param name="defaultLevel"> 枚举的按位组合，指定要跟踪的默认源级别 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="name" /> 为 null。 </exception>
        /// <exception cref="ArgumentException"> <paramref name="name" /> 为空字符串 ("")。 </exception>
        public TraceLogger(string name, LogLevels defaultLevel)
            : base(name, (SourceLevels) defaultLevel)
        {
            _filter = new NameFilter(Attributes["Filter"], Attributes["FilterRegex"]);
            _level = new LogLevel(Attributes["Level"], (int) (Switch?.Level ?? SourceLevels.All));

            //如果只有一个监听器 且是默认的监听器,则同步 Trace.Listeners
            if ((Listeners.Count == 1) && Listeners[0] is DefaultTraceListener)
            {
                SyncTraceListeners();
            }

            EntryDebugIfAttached();
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="eventType"> 日志类型 </param>
        /// <param name="message"> 日志消息 </param>
        /// <param name="ex"> 异常对象 </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="message" /> 为 null 。 </exception>
        /// <exception cref="ArgumentException"> <paramref name="message" /> 为空字符串 ("")或连续的空白。 </exception>
        /// <exception cref="RegexMatchTimeoutException"> 正则表达式匹配发生超时 </exception>
        /// <exception cref="ObjectDisposedException"> 终止期间尝试跟踪事件。 </exception>
        public void Write(TraceEventType eventType, string message, Exception ex = null, int line = 0,
            string member = "",
            string file = "")
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException($"{nameof(message)}为空字符串 (\"\")或连续的空白。", nameof(message));
            }
            var feature = Path.GetFileNameWithoutExtension(file);
            if (GlobalLoggerFilter.ShouldTrace(Name, feature, eventType)
                && _level.HasPart(eventType)
                && (_filter.IsMatch(feature) == false))
            {
                TraceEvent(eventType, line, $"[{feature}.{member}]{message}");
            }
        }

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
        /// <exception cref="ArgumentException"> <paramref name="getMessage" /> 返回值为null或连续的空白。 </exception>
        /// <exception cref="Exception"> <paramref name="getMessage" /> 执行出现异常。 </exception>
        /// <exception cref="RegexMatchTimeoutException"> 正则表达式匹配发生超时 </exception>
        /// <exception cref="ObjectDisposedException"> 终止期间尝试跟踪事件。 </exception>
        public void Write(TraceEventType eventType, Func<string> getMessage, Exception ex = null, int line = 0,
            string member = "",
            string file = "")
        {
            if (getMessage == null)
            {
                throw new ArgumentNullException(nameof(getMessage));
            }
            var message = getMessage();
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException($"{nameof(getMessage)}返回值为null或连续的空白。", nameof(getMessage));
            }
            var feature = Path.GetFileNameWithoutExtension(file);
            if (GlobalLoggerFilter.ShouldTrace(Name, feature, eventType)
                && _level.HasPart(eventType)
                && (_filter.IsMatch(feature) == false))
            {
                TraceEvent(eventType, line, $"[{feature}.{member}]{getMessage}");
            }
        }

        /// <summary>
        /// 同步 <seealso cref="Trace.Listeners" /> 和 <seealso cref="TraceLogger.Listeners" /> 中的对象
        /// 如果无法同步,则进行复制
        /// </summary>
        private void SyncTraceListeners()
        {
            var field = typeof(TraceSource).GetField("listeners", BindingFlags.Instance | BindingFlags.NonPublic)
                        ?? typeof(TraceSource)
                            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                            .FirstOrDefault(it => it.FieldType == typeof(TraceListenerCollection));
            if (field?.IsLiteral == false)
            {
                try
                {
                    field.SetValue(this, Trace.Listeners);
                    return;
                }
                catch
                {
                    // ignored
                }
            }

            Listeners.Clear();
            Listeners.AddRange(Trace.Listeners);
        }

        /// <summary>
        /// 如果处于调试器附加进程状态,则追加调试日志侦听器
        /// </summary>
        private void EntryDebugIfAttached()
        {
            //非调试模式
            if (Debugger.IsAttached == false)
            {
                return;
            }

            //winform 如果存在 ConsoleTraceListener 不附加 DefaultTraceListener
            if (PlatformServices.IsWinForm && Listeners.OfType<ConsoleTraceListener>().Any())
            {
                return;
            }

            if (Listeners.OfType<DefaultTraceListener>().Any() == false)
            {
                //可以在"输出"窗口看到所有的日志
                Listeners.Add(new DefaultTraceListener());
            }
        }

        /// <summary>
        /// 获取跟踪源所支持的自定义特性。
        /// </summary>
        /// <returns> 对跟踪源支持的自定义特性进行命名的字符串数组；如果不存在自定义特性，则为 null。 </returns>
        protected override string[] GetSupportedAttributes() => new[] { "Filter", "FilterRegex", "Level" };
    }
}