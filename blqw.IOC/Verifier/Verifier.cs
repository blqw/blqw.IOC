using System;

namespace blqw.IOC
{
    /// <summary>
    /// 验证器
    /// </summary>
    internal static class Verifier
    {
        /// <summary>
        /// 如果验证结果为false,则抛出异常
        /// </summary>
        /// <param name="result"> </param>
        /// <param name="paramName"> </param>
        /// <exception cref="Exception"> 如果<paramref name="result" />结果为false,则抛出异常 </exception>
        public static void Throw(this Verified result, string paramName)
        {
            if (result.Result == false)
                throw result.GetException(paramName);
        }

        /// <summary>
        /// 验证value是否为null,验证通过返回null
        /// </summary>
        /// <param name="value"> 待验证的值 </param>
        public static Verified? NotNull(this object value)
        {
            if (value == null)
                return new Verified(name => new ArgumentNullException(name));
            return null;
        }

        /// <summary>
        /// 验证value值是否为类型 <typeparamref name="T" /> ,验证通过返回null
        /// </summary>
        /// <typeparam name="T"> 用于验证value类型的泛型参数 </typeparam>
        /// <param name="value"> 待验证的值 </param>
        public static Verified? Is<T>(this object value)
        {
            if (value is T == false)
                return new Verified(name => new ArgumentException($"参数必须是 {typeof(T)} 类型", name));
            return null;
        }
    }
}