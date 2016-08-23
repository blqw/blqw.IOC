using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace blqw.IOC
{
    /// <summary>
    /// 组件服务
    /// </summary>
    public static class ComponentServices
    {
        /// <summary>
        /// 静态构造函数
        /// </summary>
        static ComponentServices()
        {
            MEF.Import(typeof(ComponentServices));
        }

        [Import("CreateGetter")]
        public static readonly Func<MemberInfo, Func<object, object>> GetGeter = m =>
        {
            switch (m.MemberType)
            {
                case MemberTypes.Property:
                {
                    var property = (PropertyInfo) m;
                    if (property.GetIndexParameters().Length > 0)
                    {
                        return null;
                    }
                    var o = Expression.Parameter(typeof(object), "o");
                    Debug.Assert(property.DeclaringType != null, "property.DeclaringType != null");
                    var cast = Expression.Convert(o, property.DeclaringType);
                    var p = Expression.Property(cast, property);
                    if (property.CanRead)
                    {
                        var ret = Expression.Convert(p, typeof(object));
                        return Expression.Lambda<Func<object, object>>(ret, o).Compile();
                    }
                }
                    break;
                case MemberTypes.Field:
                {
                    var field = (FieldInfo) m;
                    var o = Expression.Parameter(typeof(object), "o");
                    Debug.Assert(field.DeclaringType != null, "field.DeclaringType != null");
                    var cast = Expression.Convert(o, field.DeclaringType);
                    var p = Expression.Field(cast, field);
                    var ret = Expression.Convert(p, typeof(object));
                    return Expression.Lambda<Func<object, object>>(ret, o).Compile();
                }
            }
            return null;
        };

        [Import("CreateSetter")]
        public static readonly Func<MemberInfo, Action<object, object>> GetSeter = m =>
        {
            switch (m.MemberType)
            {
                case MemberTypes.Property:
                {
                    var property = (PropertyInfo) m;
                    var type = property.PropertyType;
                    if (property.GetIndexParameters().Length > 0)
                    {
                        return null;
                    }
                    var o = Expression.Parameter(typeof(object), "o");
                    Debug.Assert(property.DeclaringType != null, "property.DeclaringType != null");
                    var cast = Expression.Convert(o, property.DeclaringType);
                    var p = Expression.Property(cast, property);
                    if (property.CanWrite)
                    {
                        var v = Expression.Parameter(typeof(object), "v");
                        var val = Expression.Convert(v, type);
                        var assign = Expression.MakeBinary(ExpressionType.Assign, p, val);
                        var ret = Expression.Convert(assign, typeof(object));
                        return Expression.Lambda<Action<object, object>>(ret, o, v).Compile();
                    }
                    if (property.DeclaringType.IsGenericType &&
                        property.DeclaringType.Name.StartsWith("<>f__AnonymousType")) //匿名类型
                    {
                        var fieldName = $"<{property.Name}>i__Field";
                        m = property.DeclaringType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                        if (m != null)
                        {
                            goto case MemberTypes.Field;
                        }
                    }
                }
                    break;
                case MemberTypes.Field:
                {
                    var field = (FieldInfo) m;
                    if (field.IsInitOnly)
                    {
                        return field.SetValue;
                    }
                    var type = field.FieldType;
                    var o = Expression.Parameter(typeof(object), "o");
                    Debug.Assert(field.DeclaringType != null, "field.DeclaringType != null");
                    var cast = Expression.Convert(o, field.DeclaringType);
                    var p = Expression.Field(cast, field);
                    if (field.IsLiteral == false)
                    {
                        var v = Expression.Parameter(typeof(object), "v");
                        var val = Expression.Convert(v, type);
                        var assign = Expression.MakeBinary(ExpressionType.Assign, p, val);
                        var ret2 = Expression.Convert(assign, typeof(object));
                        return Expression.Lambda<Action<object, object>>(ret2, o, v).Compile();
                    }
                }
                    break;
            }
            return null;
        };

        /// <summary>
        /// 获取默认值
        /// </summary>
        [Import("GetDefaultValue")]
        public static readonly Func<Type, object> GetDefaultValue =
            t =>
                (t == null) || (t.IsValueType == false) || t.IsGenericTypeDefinition ||
                (Nullable.GetUnderlyingType(t) != null)
                    ? null
                    : Activator.CreateInstance(t);

        /// <summary>
        /// 获取转换器
        /// </summary>
        [Import("Convert3")]
        public static readonly IFormatterConverter Converter = new DefalutComponent.Converter();

        /// <summary>
        /// Json转换器
        /// </summary>
        [Import("JsonConverter")]
        public static readonly IFormatterConverter Json;

        /// <summary>
        /// XML转换器
        /// </summary>
        [Import("XmlConverter")]
        public static readonly IFormatterConverter Xml;

        /// <summary>
        /// 二进制流转换器
        /// </summary>
        [Import("BinaryConverter")]
        public static readonly IFormatterConverter Binary;

        /// <summary>
        /// Json列化器
        /// </summary>
        [Import("JsonFormatter")]
        public static readonly IFormatter JsonFormatter;

        /// <summary>
        /// XML列化器
        /// </summary>
        [Import("XmlFormatter")]
        public static readonly IFormatter XmlFormatter;

        /// <summary>
        /// 二进制流序列化器
        /// </summary>
        [Import("BinaryFormatter")]
        public static readonly IFormatter BinaryFormatter = new BinaryFormatter();

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
                {
                    return Json.Convert(json, type);
                }

                var method = Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json")?
                    .GetMethod("DeserializeObject", new[] {typeof(string), typeof(Type)});
                if ((method?.ReturnType == typeof(object)) && method.IsStatic)
                {
                    var dele = (Func<string, Type, object>) method.CreateDelegate(typeof(Func<string, Type, object>));

                    typeof(ComponentServices).GetField("ToJsonObject")
                        .SetValue(null, (Func<Type, string, object>) ((t, j) => dele(j, t)));
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
            if (Json != null)
            {
                return Json.ToString(obj);
            }

            var method = Type.GetType("Newtonsoft.Json.JsonConvert, Newtonsoft.Json")?
                .GetMethod("SerializeObject", new[] {typeof(object)});
            if ((method?.ReturnType == typeof(string)) && method.IsStatic)
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

    }
}