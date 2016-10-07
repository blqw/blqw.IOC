using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace blqw.IOC
{
    /// <summary>
    /// 插件
    /// </summary>
    public sealed class PlugIn : Component, IComparable<PlugIn>
    {
        /// <summary>
        /// 初始化插件
        /// </summary>
        /// <param name="part"></param>
        /// <param name="definition"></param>
        /// <exception cref="ArgumentNullException"><paramref name="part"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="definition"/> is <see langword="null" />.</exception>
        /// <exception cref="ComposablePartException">获取由 <see cref="T:System.ComponentModel.Composition.Primitives.ExportDefinition" /> 描述的导出对象时出错。</exception>
        public PlugIn(ComposablePart part, ExportDefinition definition)
        {
            if (part == null) throw new ArgumentNullException(nameof(part));
            if (definition == null) throw new ArgumentNullException(nameof(definition));

            Name = definition.ContractName;
            Metadata = definition.Metadata;
            InnerValue = part.GetExportedValue(definition);
            Priority = GetMetadata("Priority", 0);
            TypeIdentity = GetMetadata<string>("ExportTypeIdentity", null);
            IsMethod = InnerValue is ExportedDelegate;
            if (IsMethod)
            {
                InnerValue = typeof(ExportedDelegate).GetField("_method", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(InnerValue);
            }
            else if (InnerValue != null)
            {
                var type = InnerValue.GetType();
                if (TypeIdentity != null)
                {
                    Type = type.Module.GetType(TypeIdentity, false, false) ?? type;
                }
                else
                {
                    Type = type;
                }
            }
        }

        /// <summary>
        /// 插件名称
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// 插件类型
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// 插件类型名称
        /// </summary>
        public string TypeIdentity { get; }

        /// <summary>
        /// 自定义插件(由用户手动实例化)
        /// </summary>
        public bool IsCustom { get; internal set; }

        /// <summary>
        /// 是否是一个方法
        /// </summary>
        public bool IsMethod { get; }

        /// <summary>
        /// 插件的值
        /// </summary>
        private object InnerValue { get; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; }

        /// <summary>
        /// 协定元数据
        /// </summary>
        public IDictionary<string, object> Metadata { get; }

        /// <summary>
        /// 获取插件元数据的值,如果元数据不存在或者类型不正确,则返回 defaultValue值
        /// </summary>
        /// <typeparam name="T">元数据值的类型</typeparam>
        /// <param name="name">元数据值的名称</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public T GetMetadata<T>(string name, T defaultValue = default(T))
        {
            object value = null;
            if (Metadata?.TryGetValue(name, out value) == true)
            {
                if (value is T)
                {
                    return (T)value;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// 判断是否是可用类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsTrueOf(Type type)
        {
            if (type == null)
            {
                return true;
            }
            if (type == typeof(object))
            {
                return true;
            }
            if (IsMethod && type.IsSubclassOf(typeof(Delegate)))
            {
                var method = type.GetMethod("Invoke");
                if (CompareMethodSign(method))
                {
                    return true;
                }
            }

            if (type.IsInstanceOfType(InnerValue))
            {
                return true;
            }

            return Type?.IsSubclassOf(type) == true;
        }

        /// <summary>
        /// 获取插件
        /// </summary>
        /// <param name="type">插件类型</param>
        /// <returns></returns>
        public object GetValue(Type type)
        {
            if (type == null)
            {
                return InnerValue;
            }
            if (IsMethod && type.IsSubclassOf(typeof(Delegate)))
            {
                var method = type.GetMethod("Invoke");
                if (CompareMethodSign(method))
                {
                    return ((MethodInfo)InnerValue).CreateDelegate(type);
                }
            }

            if (type.IsInstanceOfType(InnerValue))
            {
                return InnerValue;
            }

            return Type.IsSubclassOf(type) ? InnerValue : null;
        }

        /// <summary>
        /// 获取插件
        /// </summary>
        /// <typeparam name="T">用于描述插件类型的泛型参数</typeparam>
        /// <returns></returns>
        public T GetValue<T>() => (T)GetValue(typeof(T));

        /// <summary>
        /// 比较方法签名
        /// </summary>
        /// <param name="method">用于比较的方法</param>
        /// <returns></returns>
        private bool CompareMethodSign(MethodInfo method)
        {
            if (IsMethod == false || method == null)
            {
                return false;
            }
            var raw = (MethodInfo)InnerValue;
            if (raw.ReturnType != method.ReturnType)
            {
                return false;
            }
            var p1 = raw.GetParameters();
            var p2 = method.GetParameters();
            if (p1.Length != p2.Length)
            {
                return false;
            }
            for (int i = 0; i < p1.Length; i++)
            {
                if (p1[i].ParameterType != p2[i].ParameterType)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 交换2个对象中的属性值
        /// </summary>
        /// <param name="plugin1"></param>
        /// <param name="plugin2"></param>
        /// <exception cref="ArgumentNullException"><paramref name="plugin1"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="plugin2"/> is <see langword="null" />.</exception>
        internal static void Swap(PlugIn plugin1, PlugIn plugin2)
        {
            if (plugin1 == null) throw new ArgumentNullException(nameof(plugin1));
            if (plugin2 == null) throw new ArgumentNullException(nameof(plugin2));

            foreach (var field in typeof(PlugIn).GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (field.IsLiteral) //常量
                {
                    continue;
                }
                var v1 = field.GetValue(plugin1);
                var v2 = field.GetValue(plugin2);
                field.SetValue(plugin1, v2);
                field.SetValue(plugin2, v1);
            }

        }

        /// <summary>
        /// 比较2个插件的优先级
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(PlugIn other) => other == null ? 1 : this.Priority.CompareTo(other.Priority);

    }
}
