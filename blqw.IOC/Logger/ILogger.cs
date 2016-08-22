using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blqw.IOC
{
    /// <summary>
    /// 简单日志
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="id"> </param>
        /// <param name="message"></param>
        void Debug(int id, string message);

        /// <summary>
        /// 提示日志
        /// </summary>
        /// <param name="id"> </param>
        /// <param name="message"></param>
        void Information(int id, string message);

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="id"> </param>
        /// <param name="message"></param>
        void Warning(int id, string message);

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="id"> </param>
        /// <param name="message"></param>
        /// <param name="ex"> </param>
        void Error(int id, string message, Exception ex);
    }
}
