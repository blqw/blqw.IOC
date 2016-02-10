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
    public sealed class PlugIn : Component
    {
        /// <summary>
        /// 初始化插件
        /// </summary>
        /// <param name="part"></param>
        /// <param name="definition"></param>
        public PlugIn(ComposablePart part, ExportDefinition definition)
        {
            part.NotNull()?.Throw(nameof(part));
            definition.NotNull()?.Throw(nameof(definition));
            Name = definition.ContractName;
            Metadata = definition.Metadata;
            Value = part.GetExportedValue(definition);
            Priority = GetMetadata("Priority", 0);
            TypeIdentity = GetMetadata<string>("ExportTypeIdentity", null);
            IsMethod = Value is ExportedDelegate;
            if (IsMethod)
            {
                Value = typeof(ExportedDelegate).GetField("_method", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Value);
            }
            else if (Value != null)
            {
                var type = Value.GetType();
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
        public Type Type { get; private set; }

        /// <summary>
        /// 插件类型名称
        /// </summary>
        public string TypeIdentity { get; private set; }

        /// <summary>
        /// 是否是一个方法
        /// </summary>
        public bool IsMethod { get; private set; }

        /// <summary>
        /// 插件
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; private set; }

        /// <summary>
        /// 协定元数据
        /// </summary>
        public IDictionary<string, object> Metadata { get; private set; }

        /// <summary>
        /// 获取插件元数据的值,如果元数据不存在或者类型不正确,则返回 defaultValue值
        /// </summary>
        /// <typeparam name="T">元数据值的类型</typeparam>
        /// <param name="name">元数据值的名称</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public T GetMetadata<T>(string name, T defaultValue)
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
        /// 如果当前插件是一个方法,则创建指定委托后返回,否则返回null
        /// </summary>
        /// <param name="delegateType">委托类型</param>
        /// <returns></returns>
        public Delegate CreateDelegate(Type delegateType)
        {
            delegateType.NotNull()?.Throw(nameof(delegateType));
            if (IsMethod)
            {
                return ((MethodInfo)Value).CreateDelegate(delegateType);
            }
            return null;
        }

        /// <summary>
        /// 比较方法签名
        /// </summary>
        /// <param name="method">用于比较的方法</param>
        /// <returns></returns>
        public bool CompareMethodSign(MethodInfo method)
        {
            if (IsMethod == false || method == null)
            {
                return false;
            }
            var raw = (MethodInfo)Value;
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
        /// 获取插件元数据的特性
        /// </summary>
        /// <param name="name">元数据值的名称</param>
        /// <returns></returns>
        //public IEnumerable<object> GetMetadatas(string name)
        //{
        //    return null;
        //}




    }
}
