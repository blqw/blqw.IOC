using System;
using System.Diagnostics;

namespace blqw.IOC
{
    /// <summary>
    /// 用于操作 <seealso cref="LogLevels" />
    /// </summary>
    internal sealed class LogLevel
    {
        private readonly int _flags;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="level"> 需要转换的字符串 </param>
        /// <param name="defaultLevel"> 默认值 </param>
        public LogLevel(string level, int defaultLevel)
        {
            LogLevels levels;
            _flags = Enum.TryParse(level, out levels) ? defaultLevel : (int)levels;
        }

        /// <summary>
        /// 是否完整包含 <paramref name="level" />
        /// </summary>
        public bool HasFull(SourceLevels level) => (_flags | (int)level) == _flags;

        /// <summary>
        /// 是否包含部分 <paramref name="level" />
        /// </summary>
        public bool HasPart(SourceLevels level) => (_flags & (int)level) != 0;

        /// <summary>
        /// 是否完整包含 <paramref name="type" />
        /// </summary>
        public bool HasFull(TraceEventType type) => (_flags | (int)type) == _flags;

        /// <summary>
        /// 是否包含部分 <paramref name="type" />
        /// </summary>
        public bool HasPart(TraceEventType type) => (_flags & (int)type) != 0;

        /// <summary>
        /// 返回表示当前对象的字符串。
        /// </summary>
        /// <returns>表示当前对象的字符串。</returns>
        public override string ToString() => ((TraceEventType)_flags).ToString();
    }
}