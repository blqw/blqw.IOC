using System;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Runtime.Serialization;

namespace blqw.IOC
{
    /// <summary>
    /// 组件服务
    /// </summary>
    public static class ComponentServices
    {
        [Import("CreateGetter")]
        public static readonly Func<MemberInfo, Func<object, object>> GetGeter = m =>
        {
            switch (m.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)m).GetValue;
                case MemberTypes.Property:
                    return ((PropertyInfo)m).GetValue;
            }
            return null;
        };

        [Import("CreateSetter")]
        public static readonly Func<MemberInfo, Action<object, object>> GetSeter = m =>
        {
            switch (m.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)m).SetValue;
                case MemberTypes.Property:
                    return ((PropertyInfo)m).SetValue;
            }
            return null;
        };

        /// <summary>
        /// 获取默认值
        /// </summary>
        [Import("GetDefaultValue")]
        public static readonly Func<Type, object> GetDefaultValue =
            t =>
                t == null || t.IsValueType == false || t.IsGenericTypeDefinition ||
                Nullable.GetUnderlyingType(t) != null
                    ? null
                    : Activator.CreateInstance(t);

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
        public static readonly Func<Type, string, object> ToJsonObject =
            (type, json) =>
            {
                if (Json != null)
                    return Json.Convert(json, type);

                var method = Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json")?.GetMethod("DeserializeObject", new[] { typeof(string), typeof(Type) });
                if (method?.ReturnType == typeof(object) && method?.IsStatic == true)
                {
                    var dele = (Func<string, Type, object>)method.CreateDelegate(typeof(Func<string, Type, object>));

                    typeof(ComponentServices).GetField("ToJsonObject").SetValue(null, ((Func<Type, string, object>)((t, j) => dele(j, t))));
                    return ToJsonObject(type, json);
                }
                throw new NotSupportedException($"{nameof(ComponentServices)}.{nameof(Json)}为null,该功能无法使用");
            };


        /// <summary>
        /// 用于将Json字符串转为实体对象的方法
        /// </summary>
        [Import("ToJsonString")]
        public static readonly Func<object, string> ToJsonString = obj =>
        {
            if (Json != null) return Json.ToString(obj);

            var method = Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json")?.GetMethod("SerializeObject", new[] { typeof(object) });
            if (method?.ReturnType == typeof(string) && method?.IsStatic == true)
            {
                var dele = method.CreateDelegate(typeof(Func<object, string>));
                typeof(ComponentServices).GetField("ToJsonString").SetValue(null, dele);
                return ToJsonString(obj);
            }
            throw new NotSupportedException($"{nameof(ComponentServices)}.{nameof(Json)}为null,该功能无法使用");
        };


        /// <summary>
        /// 获取动态类型
        /// </summary>
        [Import("GetDynamic")]
        public static readonly Func<object, dynamic> GetDynamic = o => o;

        static ComponentServices()
        {
            MEF.Import(typeof(ComponentServices));
        }
    }
}