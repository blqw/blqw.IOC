using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace blqw.IOC
{

    class DefalutComponent
    {
        public static object Convert(object value, Type type, bool throwError)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (type == typeof(string))
            {
                return value.ToString();
            }
            var str = value as string;
            if (str == null)
            {
                if (value == null)
                {
                    return null;
                }
                try
                {
                    return System.Convert.ChangeType(value, type);
                }
                catch
                {
                    if (throwError)
                    {
                        throw;
                    }
                    return null;
                }
            }
            if (type == typeof(Guid))
            {
                Guid g;
                if (Guid.TryParse(str, out g))
                {
                    return g;
                }
            }
            else if (type == typeof(Uri))
            {
                Uri u;
                if (Uri.TryCreate(str, UriKind.RelativeOrAbsolute, out u))
                {
                    return u;
                }
            }
            else if (type == typeof(TimeSpan))
            {
                TimeSpan t;
                if (TimeSpan.TryParse(str, out t))
                {
                    return t;
                }
            }
            else if (type == typeof(Type))
            {
                return Type.GetType(str, false, true);
            }
            else
            {
                try
                {
                    return System.Convert.ChangeType(value, type);
                }
                catch
                {
                    if (throwError)
                    {
                        throw;
                    }
                    return null;
                }
            }
            if (throwError)
            {
                throw new InvalidCastException($"字符串: {str} 转为类型:{ComponentServices.Converter?.ToString(type) ?? type?.ToString()}失败");
            }
            return null;

        }

        public class Converter : FormatterConverter, IFormatterConverter
        {

            #region IFormatterConverter 成员

            object IFormatterConverter.Convert(object value, Type type)
            {
                return DefalutComponent.Convert(value, type, true);
            }

            #endregion
        }

    }
}
