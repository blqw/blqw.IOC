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
    [Export("Component")]
    class Component
    {
        /// <summary> 获取动态类型
        /// </summary>
        [Import("GetDynamic")]
        public static readonly Func<object, dynamic> GetDynamic = o => {
            throw new NotImplementedException("需要Top.Convert3插件");
        };

        /// <summary> 序列化
        /// </summary>
        [Import("SerializeToBytes")]
        public static readonly Func<object, byte[]> Serialize = o => {
            throw new NotImplementedException("需要Top.Serializable插件");
        };


        /// <summary> 反序列化
        /// </summary>
        [Import("DeserializeFormBytes")]
        public static readonly Func<byte[], object> Deserialize = o => {
            throw new NotImplementedException("需要Top.Serializable插件");
        };

        /// <summary> 获取转换器
        /// </summary>
        [Import()]
        public static readonly IFormatterConverter Converter = null;
    }
}
