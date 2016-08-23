using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace blqw.IOC
{
    /// <summary>
    /// 全局日志过滤器
    /// </summary>
    public sealed class GlobalLoggerFilter : TraceSource
    {
        /// <summary>
        /// 单例,全局唯一实例
        /// </summary>
        private static readonly GlobalLoggerFilter Instance = new GlobalLoggerFilter();

        /// <summary>
        /// 日志跟踪等级
        /// </summary>
        private readonly SourceLevels _level;

        /// <summary>
        /// 日志模块过滤
        /// </summary>
        private readonly NameFilter _moduleFilter;

        /// <summary>
        /// 日志源文件过滤
        /// </summary>
        private readonly NameFilter _sourceFilter;

        /// <summary>
        /// 使用源的指定名称和执行跟踪的默认源级别初始化 <see cref="T:System.Diagnostics.TraceSource" /> 类的新实例。
        /// </summary>
        private GlobalLoggerFilter()
            : base("blqw", SourceLevels.All)
        {
            _sourceFilter = new NameFilter(Attributes["SourceFilter"], Attributes["SourceFilterRegex"]);
            _moduleFilter = new NameFilter(Attributes["ModuleFilter"], Attributes["ModuleFilterRegex"]);

            var level = Attributes["Level"];
            if (string.IsNullOrEmpty(level) || (Enum.TryParse(level, out _level) == false))
            {
                _level = Switch?.Level ?? SourceLevels.All;
            }
        }

        /// <summary>
        /// 获取跟踪源所支持的自定义特性。
        /// </summary>
        /// <returns>
        /// 对跟踪源支持的自定义特性进行命名的字符串数组；如果不存在自定义特性，则为 null。
        /// </returns>
        protected override string[] GetSupportedAttributes()
            => new[] {"SourceFilter", "SourceFilterRegex", "Level", "ModuleFilter", "ModuleFilterRegex"};

        /// <summary>
        /// 确定是否应记录日志
        /// </summary>
        /// <param name="source"> </param>
        /// <param name="module"> </param>
        /// <param name="level"> </param>
        /// <returns> </returns>
        /// <exception cref="RegexMatchTimeoutException"> 正则表达式匹配发生超时 </exception>
        public static bool ShouldTrace(string source, string module, SourceLevels level)
            => Instance._level.HasFlag(level)
               && (Instance._sourceFilter.IsMatch(source) == false)
               && (Instance._moduleFilter.IsMatch(module) == false);
    }
}