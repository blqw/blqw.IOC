using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace blqw.IOC
{
    /// <summary>
    /// 默认日志记录器
    /// </summary>
    public class LoggerSource : TraceSource
    {
        /// <summary>
        /// 使用指定的源名称初始化 <see cref="T:System.Diagnostics.TraceSource" /> 类的新实例。
        /// </summary>
        /// <param name="name"> 源的名称，通常为应用程序的名称。 </param>
        /// <param name="defaultLevel"> 枚举的按位组合，指定要跟踪的默认源级别 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="name" /> 为 null。 </exception>
        /// <exception cref="ArgumentException"> <paramref name="name" /> 为空字符串 ("")。 </exception>
        public LoggerSource(string name, SourceLevels defaultLevel)
            : base(name, defaultLevel)
        {
            this.Initialize();
        }
    }
}