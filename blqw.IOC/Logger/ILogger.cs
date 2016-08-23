using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace blqw.IOC
{
    /// <summary>
    /// 简单日志接口
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="line">行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        void Debug(string message, [CallerLineNumber] int line = 0, [CallerMemberName] string member = "", [CallerFilePath]string file = "");

        /// <summary>
        /// 提示日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="line">行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        void Information(string message, [CallerLineNumber]int line = 0, [CallerMemberName]string member = "", [CallerFilePath]string file = "");

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="line">行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        void Warning(string message, [CallerLineNumber]int line = 0, [CallerMemberName]string member = "", [CallerFilePath]string file = "");

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"> </param>
        /// <param name="line">行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        void Error(string message, Exception ex, [CallerLineNumber]int line = 0, [CallerMemberName]string member = "", [CallerFilePath]string file = "");


        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="getMessage"></param>
        /// <param name="line">行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        void Debug(Func<string> getMessage, [CallerLineNumber] int line = 0, [CallerMemberName] string member = "", [CallerFilePath]string file = "");

        /// <summary>
        /// 提示日志
        /// </summary>
        /// <param name="getMessage"></param>
        /// <param name="line">行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        void Information(Func<string> getMessage, [CallerLineNumber]int line = 0, [CallerMemberName]string member = "", [CallerFilePath]string file = "");

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="getMessage"></param>
        /// <param name="line">行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        void Warning(Func<string> getMessage, [CallerLineNumber]int line = 0, [CallerMemberName]string member = "", [CallerFilePath]string file = "");

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="getMessage"></param>
        /// <param name="ex"> </param>
        /// <param name="line">行号 </param>
        /// <param name="member"> 调用方法或属性 </param>
        /// <param name="file"> 文件名 </param>
        void Error(Func<string> getMessage, Exception ex, [CallerLineNumber]int line = 0, [CallerMemberName]string member = "", [CallerFilePath]string file = "");
    }
}
