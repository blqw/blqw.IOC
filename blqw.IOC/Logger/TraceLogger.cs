using System;
using System.Collections;
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
        private readonly SourceLevels _level;

        private static readonly Hashtable _LevelMap = InitLevelMap();

        private static Hashtable InitLevelMap()
        {
            var map = new Hashtable(StringComparer.OrdinalIgnoreCase);
            foreach (SourceLevels value in Enum.GetValues(typeof(SourceLevels)))
            {
                map[value.ToString("D")] = value;
                map[value.ToString("G")] = value;
            }
            map["debug"] = SourceLevels.Verbose;
            map["info"] = SourceLevels.Information;
            map["warn"] = SourceLevels.Warning;
            map["fail"] = SourceLevels.Error;
            map["block"] = SourceLevels.Critical;
            map["test"] = SourceLevels.Verbose;
            return map;
        }

        /// <summary>
        /// 使用指定的源名称初始化 <see cref="T:System.Diagnostics.TraceSource" /> 类的新实例。
        /// </summary>
        /// <param name="name"> 源的名称，通常为应用程序的名称。 </param>
        /// <param name="defaultLevel"> 枚举的按位组合，指定要跟踪的默认源级别 </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name" /> 为 null。
        /// </exception>
        public TraceLogger(string name, SourceLevels defaultLevel)
            : base(name, defaultLevel)
        {
            _filter = new NameFilter(Attributes["SourceFilter"], Attributes["SourceFilterRegex"]);
            _level = (SourceLevels?)_LevelMap[Attributes["Level"] + ""] ?? Switch?.Level ?? SourceLevels.All;

            //如果只有一个监听器 且是默认的监听器,则同步 Trace.Listeners
            if ((Listeners.Count == 1) && Listeners[0] is DefaultTraceListener)
            {
                SyncTraceListeners();
            }

            if (Debugger.IsAttached == false)
            {
                return;
            }

            //Web和Console 附加 DefaultTraceListener
            //winform 附加 ConsoleTraceListener
            //可以在"输出"窗口看到所有的日志
            if (PlatformService.IsWeb || PlatformService.IsConsole)
            {
                if (Listeners.OfType<DefaultTraceListener>().Any() == false)
                {
                    Listeners.Add(new DefaultTraceListener());
                }
            }
            else if (Listeners.OfType<ConsoleTraceListener>().Any() == false)
            {
                Listeners.Add(new ConsoleTraceListener());
            }
        }
        

        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="message"> </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="RegexMatchTimeoutException"> 过滤器正则表达式匹配发生超时。 </exception>
        /// <exception cref="ObjectDisposedException"> 终止期间尝试跟踪事件。 </exception>
        public void Debug(string message, int line = 0, string member = "", string file = "")
            => TraceWrite(TraceEventType.Verbose, SourceLevels.Verbose, message, line, member, file);

        /// <summary>
        /// 提示日志
        /// </summary>
        /// <param name="message"> </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="RegexMatchTimeoutException"> 过滤器正则表达式匹配发生超时。 </exception>
        /// <exception cref="ObjectDisposedException"> 终止期间尝试跟踪事件。 </exception>
        public void Information(string message, int line = 0, string member = "", string file = "")
            => TraceWrite(TraceEventType.Information, SourceLevels.Information, message, line, member, file);

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="message"> </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="RegexMatchTimeoutException"> 过滤器正则表达式匹配发生超时。 </exception>
        /// <exception cref="ObjectDisposedException"> 终止期间尝试跟踪事件。 </exception>
        public void Warning(string message, int line = 0, string member = "", string file = "")
            => TraceWrite(TraceEventType.Warning, SourceLevels.Warning, message, line, member, file);

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="message"> </param>
        /// <param name="ex"> </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="RegexMatchTimeoutException"> 过滤器正则表达式匹配发生超时。 </exception>
        /// <exception cref="ObjectDisposedException"> 终止期间尝试跟踪事件。 </exception>
        public void Error(string message, Exception ex, int line = 0, string member = "", string file = "")
        {
            var source = Path.GetFileNameWithoutExtension(file);
            if (GlobalLoggerFilter.ShouldTrace(source, Name, SourceLevels.Error)
                && _level.HasFlag(SourceLevels.Error)
                && (_filter.IsMatch(source) == false))
            {
                TraceData(TraceEventType.Error, line, new { source, member, message, ex.Source });
                TraceData(TraceEventType.Error, line, ex);
            }
        }

        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="getMessage"></param>
        /// <param name="line">行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        public void Debug(Func<string> getMessage, int line = 0, string member = "", string file = "")
            => TraceWrite(TraceEventType.Verbose, SourceLevels.Verbose, getMessage, line, member, file);

        /// <summary>
        /// 提示日志
        /// </summary>
        /// <param name="getMessage"></param>
        /// <param name="line">行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        public void Information(Func<string> getMessage, int line = 0, string member = "", string file = "")
            => TraceWrite(TraceEventType.Information, SourceLevels.Information, getMessage, line, member, file);

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="getMessage"></param>
        /// <param name="line">行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        public void Warning(Func<string> getMessage, int line = 0, string member = "", string file = "")
            => TraceWrite(TraceEventType.Warning, SourceLevels.Warning, getMessage, line, member, file);

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="getMessage"></param>
        /// <param name="ex"> </param>
        /// <param name="line">行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        public void Error(Func<string> getMessage, Exception ex, int line = 0, string member = "", string file = "")
        {
            var source = Path.GetFileNameWithoutExtension(file);
            if (GlobalLoggerFilter.ShouldTrace(source, Name, SourceLevels.Error)
                && _level.HasFlag(SourceLevels.Error)
                && (_filter.IsMatch(source) == false))
            {
                TraceData(TraceEventType.Error, line, new { source, member, message = getMessage(), ex.Source });
                TraceData(TraceEventType.Error, line, ex);
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
                }
                catch
                {
                    Listeners.Clear();
                    Listeners.AddRange(Trace.Listeners);
                }
            }
            else
            {
                Listeners.Clear();
                Listeners.AddRange(Trace.Listeners);
            }
        }

        /// <summary> 获取跟踪源所支持的自定义特性。 </summary>
        /// <returns> 对跟踪源支持的自定义特性进行命名的字符串数组；如果不存在自定义特性，则为 null。 </returns>
        protected override string[] GetSupportedAttributes() => new[] { "SourceFilter", "SourceFilterRegex", "Level" };

        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="eventType"> 日志时间类型 </param>
        /// <param name="level"> 日志事件等级 </param>
        /// <param name="message"> 消息 </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="RegexMatchTimeoutException"> 过滤器正则表达式匹配发生超时。 </exception>
        /// <exception cref="ObjectDisposedException"> 终止期间尝试跟踪事件。 </exception>
        public void TraceWrite(TraceEventType eventType, SourceLevels level, string message, int line, string member, string file)
        {
            var source = Path.GetFileNameWithoutExtension(file);
            if (GlobalLoggerFilter.ShouldTrace(source, Name, level)
                && _level.HasFlag(level)
                && (_filter.IsMatch(source) == false))
            {
                TraceEvent(eventType, line, $"[{source}.{member}]{message}");
            }
        }


        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="eventType"> 日志时间类型 </param>
        /// <param name="level"> 日志事件等级 </param>
        /// <param name="message"> 消息 </param>
        /// <param name="line"> 行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        /// <exception cref="RegexMatchTimeoutException"> 过滤器正则表达式匹配发生超时。 </exception>
        /// <exception cref="ObjectDisposedException"> 终止期间尝试跟踪事件。 </exception>
        public void TraceWrite(TraceEventType eventType, SourceLevels level, Func<string> getMessage, int line, string member, string file)
        {
            var source = Path.GetFileNameWithoutExtension(file);
            if (GlobalLoggerFilter.ShouldTrace(source, Name, level)
                && _level.HasFlag(level)
                && (_filter.IsMatch(source) == false))
            {
                TraceEvent(eventType, line, $"[{source}.{member}]{getMessage()}");
            }
        }
    }
}