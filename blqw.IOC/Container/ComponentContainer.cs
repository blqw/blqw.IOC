using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Reflection;

namespace blqw.IOC
{
    /// <summary>
    /// 组件容器
    /// </summary>
    /// <typeparam name="TKey"> 组件键的类型 </typeparam>
    /// <typeparam name="TValue"> 组件的类型 </typeparam>
    public sealed class ComponentContainer<TKey, TValue>
    {
        /// <summary>
        /// 用于获取组件的键
        /// </summary>
        private readonly Func<TValue, TKey> _getKey;

        /// <summary>
        /// 用户保存所有组件
        /// </summary>
        private readonly ConcurrentDictionary<TKey, TValue> _items;

        /// <summary>
        /// 用于从 <seealso cref="PlugIn" /> 中获取组件
        /// </summary>
        private readonly Func<PlugIn, TValue> _select;

        /// <summary>
        /// 初始化容器
        /// </summary>
        /// <param name="getKey"> 用于获取组件的键 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="getKey" /> is <see langword="null" />. </exception>
        /// <exception cref="TargetException"> 从PlugIn中获取组件出现异常 </exception>
        public ComponentContainer(Func<TValue, TKey> getKey)
            : this(null, getKey)
        {
        }

        /// <summary>
        /// 初始化容器
        /// </summary>
        /// <param name="getKey"> 用于获取组件的键 </param>
        /// <param name="select"> 用于从 <seealso cref="PlugIn" /> 中获取组件 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="getKey" /> is <see langword="null" />. </exception>
        /// <exception cref="TargetException"> 从PlugIn中获取组件出现异常 </exception>
        public ComponentContainer(Func<PlugIn, TValue> select, Func<TValue, TKey> getKey)
        {
            if (getKey == null)
            {
                throw new ArgumentNullException(nameof(getKey));
            }
            if (select == null)
            {
                var id = AttributedModelServices.GetTypeIdentity(typeof(TValue));
                _select = p => p.TypeIdentity == id ? p.GetValue<TValue>() : default(TValue);
            }
            else
            {
                _select = select;
            }
            _getKey = getKey;
            _items = new ConcurrentDictionary<TKey, TValue>();
            Reload();
        }

        /// <summary>
        /// 获取与指定的键关联的组件。
        /// </summary>
        /// <param name="key"> 组件的键 </param>
        /// <returns> </returns>
        public TValue this[TKey key]
        {
            get
            {
                if (key == null)
                {
                    return default(TValue);
                }
                TValue value;
                _items.TryGetValue(key, out value);
                return value;
            }
        }

        /// <summary>
        /// 容器中组件的个数
        /// </summary>
        public int Count => _items.Count;

        /// <summary>
        /// 重新载入所有组件
        /// </summary>
        /// <exception cref="TargetException"> 从PlugIn中获取组件出现异常 </exception>
        /// <exception cref="TargetException"> 获取组件的键出现异常 </exception>
        /// <exception cref="ArgumentNullException"> 获取组件的键是空的 </exception>
        public void Reload()
        {
            _items.Clear();
            foreach (var plugIn in MEF.PlugIns)
            {
                var value = Select(plugIn);
                if (value == null)
                {
                    continue;
                }
                var key = GetKey(value);
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }
                _items[key] = value;
            }
        }

        /// <summary>
        /// 获取插件中的组件
        /// </summary>
        /// <param name="plugIn"> </param>
        /// <exception cref="TargetException"> 从PlugIn中获取组件出现异常 </exception>
        private TValue Select(PlugIn plugIn)
        {
            try
            {
                return _select(plugIn);
            }
            catch (Exception ex)
            {
                const string messgae = "从PlugIn中获取组件出现异常";
                LogServices.Logger?.Write(TraceEventType.Error, messgae, ex);
                throw new TargetException(messgae, ex);
            }
        }

        /// <summary>
        /// 获取组件中的键
        /// </summary>
        /// <param name="value"> </param>
        /// <exception cref="TargetException"> 获取组件的键出现异常 </exception>
        /// <returns> </returns>
        private TKey GetKey(TValue value)
        {
            try
            {
                return _getKey(value);
            }
            catch (Exception ex)
            {
                const string messgae = "获取组件的键出现异常";
                LogServices.Logger?.Write(TraceEventType.Error, messgae, ex);
                throw new TargetException(messgae, ex);
            }
        }
    }
}