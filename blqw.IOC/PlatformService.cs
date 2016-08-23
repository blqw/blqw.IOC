using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blqw.IOC
{
    /// <summary>
    /// 当前运行平台检查
    /// </summary>
    public static class PlatformService
    {
        static PlatformService()
        {
            //判断当前程序的配置文件,如果是web.config,则认为是web应用程序
            //否则认为应用程序,附加ConsoleTraceListener
            //这样就可以在输出窗口看到所有的日志
            if ("web.config" == Path.GetFileName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile)?.ToLowerInvariant())
            {
                IsWeb = true;
            }
            else
            {
                //如果 Console.Title 没有抛出异常,就是Console应用程序
                try
                {
                    Console.Title.GetHashCode();
                    IsConsole = true;
                }
                catch(IOException)
                {
                    //Console.Title 抛出异常,是 WinForm应用程序
                    IsWinForm = true;
                }
            }

        }
        /// <summary>
        /// 当前环境是Web应用程序
        /// </summary>
        public static bool IsWeb { get; }
        /// <summary>
        /// 当前运行环境是Console应用程序
        /// </summary>
        public static bool IsConsole { get; }
        /// <summary>
        /// 当前运行环境是winform应用程序
        /// </summary>
        public static bool IsWinForm { get; }
    }
}
