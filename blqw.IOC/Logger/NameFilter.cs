using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace blqw.IOC
{
    /// <summary>
    /// 名称过滤器
    /// </summary>
    public sealed class NameFilter
    {
        /// <summary>
        /// 需要过滤的名称,不区分大小写
        /// </summary>
        private readonly HashSet<string> _names;

        /// <summary>
        /// 需要过滤名称的正则表达式
        /// </summary>
        private readonly Regex _regex;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="names"> 需要过滤的名称,不区分大小写,以逗号分割,忽略前后空白符 </param>
        /// <param name="regex"> 需要过滤名称的正则表达式 </param>
        public NameFilter(string names, string regex)
        {
            if (string.IsNullOrWhiteSpace(names) == false)
            {
                _names = new HashSet<string>(names.Split(',').Select(it => it.Trim()), StringComparer.OrdinalIgnoreCase);
            }
            if (string.IsNullOrWhiteSpace(regex) == false)
            {
                _regex = new Regex(regex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        /// <summary>
        /// 是否有匹配
        /// </summary>
        /// <param name="name"> 判断名称是否被匹配 </param>
        /// <returns> </returns>
        /// <exception cref="RegexMatchTimeoutException"> 正则表达式匹配发生超时。 </exception>
        public bool IsMatch(string name)
            => (name != null) && ((_names?.Contains(name) == true) || (_regex?.IsMatch(name) == true));
    }
}