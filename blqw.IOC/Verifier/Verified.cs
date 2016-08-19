using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blqw.IOC
{
    /// <summary>
    /// 验证结果
    /// </summary>
    struct Verified
    {
        /// <summary>
        /// 初始化 验证失败结果的实例
        /// </summary>
        /// <param name="createException">用于创建异常对象的委托</param>
        public Verified(Func<string, Exception> createException)
        {
            _CreateException = createException;
        }

        /// <summary>
        /// 创建异常对象的委托
        /// </summary>
        private readonly Func<string, Exception> _CreateException;

        /// <summary>
        /// 正确为true,否则为false
        /// </summary>
        public bool Result => _CreateException == null;

        /// <summary>
        /// 当验证结果为false时,返回异常对象,否则为null
        /// </summary>
        /// <param name="paramName">参数的名称</param>
        internal Exception GetException(string paramName)
        {
            return _CreateException?.Invoke(paramName ?? "<未知>");
        }

        /// <summary>
        /// 隐式转换为布尔类型
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator bool (Verified value)
        {
            return value._CreateException == null;
        }

        /// <summary>
        /// 隐式转换为布尔类型
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator bool (Verified? value)
        {
            return value?._CreateException == null;
        }
    }
}
