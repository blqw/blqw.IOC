using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace blqw.IOC
{
    /// <summary>
    /// 名称过滤器
    /// </summary>
    public sealed class NameFilter
    {
        private readonly HashSet<string> _names;
        private readonly Regex _regex;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="names"> 需要过滤的名称,不区分大小写 </param>
        /// <param name="regex"> 需要过滤名称的正则表达式 </param>
        public NameFilter(string names, string regex)
        {
            if (string.IsNullOrWhiteSpace(names) == false)
            {
                _names = new HashSet<string>(names.Split(','), StringComparer.OrdinalIgnoreCase);
            }
            if (string.IsNullOrWhiteSpace(regex) == false)
            {
                _regex = new Regex(regex, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        /// <summary> 
        /// 是否有匹配
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        /// <exception cref="RegexMatchTimeoutException">正则表达式匹配发生超时。</exception>
        public bool IsMatch(string text) => text != null && (_names?.Contains(text) == true || _regex?.IsMatch(text) == true);
    }
}
