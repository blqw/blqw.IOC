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
    /// <summary>
    /// 组件服务
    /// </summary>
    public static class ComponentServices
    {
        static ComponentServices()
        {
            MEF.Import(typeof(ComponentServices));
        }


        /// <summary> 
        /// 获取默认值
        /// </summary>
        [Import("GetDefaultValue")]
        public static readonly Func<Type, object> GetDefaultValue = t => t == null || t.IsValueType == false || t.IsGenericTypeDefinition || Nullable.GetUnderlyingType(t) != null ? null : Activator.CreateInstance(t);
        
        /// <summary> 
        /// 获取转换器
        /// </summary>
        [Import("Convert3")]
        public static readonly IFormatterConverter Converter = new DefalutComponent.Converter();

        [Import("JsonConverter")]
        public static readonly IFormatterConverter Json;

        [Import("XmlConverter")]
        public static readonly IFormatterConverter Xml;

        [Import("BinaryConverter")]
        public static readonly IFormatterConverter Binary;

        /// <summary> 
        /// 包装反射对象
        /// </summary>
        [Import("MemberInfoWrapper")]
        public static readonly Func<MemberInfo, MemberInfo> WrapMamber = m => m;
        
        /// <summary> 
        /// 用于将Json字符串转为实体对象的方法
        /// </summary>
        [Import("ToJsonObject")]
        public readonly static Func<Type, string, object> ToJsonObject =
            delegate (Type type, string json)
            {
                return Json.Convert(json, type);
            };

        /// <summary> 
        /// 用于将Json字符串转为实体对象的方法
        /// </summary>
        [Import("ToJsonString")]
        public readonly static Func<object, string> ToJsonString =
            delegate (object obj)
            {
                return Json.ToString(obj);
            };


        /// <summary> 获取动态类型
        /// </summary>
        [Import("GetDynamic")]
        public static readonly Func<object, dynamic> GetDynamic ;




    }
}
