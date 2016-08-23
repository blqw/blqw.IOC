using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace blqw.IOC
{
    /// <summary>
    /// 插件容器
    /// </summary>
    public sealed class PlugInContainer : Container, IEnumerable<PlugIn>
    {
        /// <summary>
        /// 插件部件目录
        /// </summary>
        private readonly AggregateCatalog _cataLog;

        /// <summary>
        /// 插件部件容器
        /// </summary>
        private readonly CompositionContainer _container;

        /// <summary>
        /// 异常集合
        /// </summary>
        private readonly List<Exception> _exceptions;

        /// <summary>
        /// 初始化插件容器
        /// </summary>
        public PlugInContainer()
        {
            _cataLog = new AggregateCatalog();
            _container = new CompositionContainer(_cataLog);
            _exceptions = new List<Exception>();
        }

        /// <summary>
        /// 初始化插件容器
        /// </summary>
        /// <param name="catalog"> 插件目录 </param>
        public PlugInContainer(ComposablePartCatalog catalog)
            : this()
        {
            AddCatalog(catalog);
        }

        /// <summary>
        /// 根据名称获取优先级最高的插件
        /// </summary>
        /// <param name="name"> 插件名称 </param>
        /// <returns> </returns>
        public PlugIn this[string name] => (PlugIn) Components[name];

        /// <summary>
        /// 载入插件的异常
        /// </summary>
        public AggregateException Exceptions
            => _exceptions == null ? null : new AggregateException("部分插件加载出现错误", _exceptions);

        /// <summary>
        /// 枚举所有插件
        /// </summary>
        /// <returns> </returns>
        public IEnumerator<PlugIn> GetEnumerator() => Components.Cast<PlugIn>().GetEnumerator();

        /// <summary>
        /// 枚举所有插件
        /// </summary>
        /// <returns> </returns>
        IEnumerator IEnumerable.GetEnumerator() => Components.GetEnumerator();

        /// <summary>
        /// 向容器中增加自定义插件
        /// </summary>
        /// <param name="plugin"> 插件 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="plugin" /> is <see langword="null" />. </exception>
        public void Add(PlugIn plugin)
        {
            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            if (Components.Cast<PlugIn>().Contains(plugin))
            {
                return; //如果已经存在则忽略本次添加操作
            }
            var existsed = (PlugIn) Components[plugin.Name];
            if (existsed == null)
            {
                base.Add(plugin, plugin.Name);
            }
            else if (existsed.Priority < plugin.Priority)
            {
                PlugIn.Swap(existsed, plugin);
            }
        }

        /// <summary>
        /// 获取插件导出项
        /// </summary>
        /// <param name="name"> 插件名称 </param>
        /// <param name="type"> 插件类型 </param>
        /// <returns> </returns>
        /// <exception cref="ArgumentException">
        /// 当 <paramref name="name" /> 为null时,<paramref name="type" /> 不能是
        /// <seealso cref="object" />
        /// </exception>
        public IEnumerable<object> GetExports(string name, Type type)
        {
            if ((name == null) && (type == typeof(object)))
            {
                throw new ArgumentException($"当{nameof(name)}为null时,{nameof(type)}不能是 System.Object");
            }
            if ((type == null) || (type == typeof(object)))
            {
                foreach (PlugIn plugin in Components)
                {
                    if (name == plugin.Name)
                    {
                        var value = plugin.GetValue(type);
                        if (value != null)
                        {
                            yield return value;
                        }
                    }
                }
                yield break;
            }
            foreach (var export in _container.GetExports(type, null, name))
            {
                var handler = export.Value as ExportedDelegate;
                if (handler != null)
                {
                    var func = handler.CreateDelegate(type);
                    if (func != null)
                    {
                        yield return func;
                    }
                }
                else
                {
                    yield return export.Value;
                }
            }
            foreach (PlugIn plugin in Components)
            {
                if (plugin.IsComposition == false)
                {
                    if ((name == null) || (name == plugin.Name))
                    {
                        var value = plugin.GetValue(type);
                        if (value != null)
                        {
                            yield return value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取插件导出项
        /// </summary>
        /// <param name="name"> 插件名称 </param>
        /// <returns> </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="name" /> is <see langword="null" />. </exception>
        public IEnumerable<object> GetExports(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            return GetExports(name, null);
        }

        /// <summary>
        /// 获取插件导出项
        /// </summary>
        /// <param name="type"> 插件类型 </param>
        /// <returns> </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="type" /> is <see langword="null" />. </exception>
        public IEnumerable<object> GetExports(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return GetExports(null, type);
        }

        /// <summary>
        /// 获取插件导出项
        /// </summary>
        /// <typeparam name="T"> 插件类型 </typeparam>
        /// <param name="name"> 插件名称 </param>
        /// <returns> </returns>
        public IEnumerable<T> GetExports<T>(string name)
        {
            return GetExports(name, typeof(T)).Cast<T>();
        }

        /// <summary>
        /// 获取插件导出项
        /// </summary>
        /// <typeparam name="T"> 插件类型 </typeparam>
        /// <returns> </returns>
        public IEnumerable<T> GetExports<T>()
        {
            return GetExports(null, typeof(T)).Cast<T>();
        }

        /// <summary>
        /// 获取优先级最高的一个插件的导出项
        /// </summary>
        /// <param name="name"> 插件名称 </param>
        /// <param name="type"> 插件类型 </param>
        /// <returns> </returns>
        /// <exception cref="ArgumentException">
        /// 当 <paramref name="name" /> 为 `null` 时, <paramref name="type" /> 不能为`null`或`
        /// <see cref="object" />`.
        /// </exception>
        public object GetExport(string name, Type type)
        {
            if (type == typeof(object))
            {
                type = null;
            }
            if ((name == null) && (type == null))
            {
                throw new ArgumentException($"当{nameof(name)}为null时,{nameof(type)}不能为`null`或`System.Object`");
            }
            foreach (
                var plugin in
                this.Where(p => ((name == null) || (name == p.Name)) && ((type == null) || p.IsTrueOf(type)))
                    .OrderByDescending(p => p.Priority))
            {
                return plugin.GetValue(type);
            }
            return null;
        }

        /// <summary>
        /// 获取优先级最高的一个插件的导出项
        /// </summary>
        /// <param name="name"> 插件名称 </param>
        /// <returns> </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="name" /> is <see langword="null" />. </exception>
        public object GetExport(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            return GetExport(name, null);
        }

        /// <summary>
        /// 获取优先级最高的一个插件的导出项
        /// </summary>
        /// <param name="type"> 插件类型 </param>
        /// <returns> </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="name" /> is <see langword="null" />. </exception>
        public object GetExport(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return GetExport(null, type);
        }

        /// <summary>
        /// 获取优先级最高的一个插件的导出项
        /// </summary>
        /// <typeparam name="T"> 插件类型 </typeparam>
        /// <param name="name"> 插件名称 </param>
        /// <returns> </returns>
        public T GetExport<T>(string name)
        {
            return (T) GetExport(name, typeof(T));
        }

        /// <summary>
        /// 获取优先级最高的一个插件的导出项
        /// </summary>
        /// <typeparam name="T"> 插件类型 </typeparam>
        /// <returns> </returns>
        public T GetExport<T>()
        {
            return (T) GetExport(null, typeof(T));
        }

        /// <summary>
        /// 添加插件组件部件目录
        /// </summary>
        /// <param name="catalog"> 对象的可组合部件目录 </param>
        public void AddCatalog(ComposablePartCatalog catalog)
        {
            var agg = catalog as AggregateCatalog;
            if (agg != null)
            {
                foreach (var log in agg.Catalogs)
                {
                    AddCatalog(log);
                }
                return;
            }

            if (_cataLog.Catalogs.Contains(catalog) == false)
            {
                _cataLog.Catalogs.Add(catalog);
                foreach (var p in catalog)
                {
                    var part = p.CreatePart();
                    foreach (var definition in part.ExportDefinitions)
                    {
                        try
                        {
                            var plugin = new PlugIn(part, definition) {IsComposition = true};
                            Add(plugin);
                        }
                        catch (Exception ex)
                        {
                            LogServices.Logger?.Error("插件载入失败", ex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 向容器中增加自定义插件
        /// </summary>
        /// <param name="component"> 插件 </param>
        /// <exception cref="InvalidCastException"> <paramref name="component" /> 不是 <seealso cref="PlugIn" /> 类型 </exception>
        public override void Add(IComponent component)
        {
            var plugin = component as PlugIn;
            if (plugin != null)
            {
                Add(plugin);
            }
            else
            {
                throw new InvalidCastException($"{nameof(component)}不是{typeof(PlugIn).FullName}类型");
            }
        }

        /// <summary>
        /// 向容器中增加自定义插件
        /// </summary>
        /// <param name="component"> 插件 </param>
        /// <param name="name"> 插件名称 </param>
        /// <exception cref="InvalidCastException"> <paramref name="component" /> 不是 <seealso cref="PlugIn" /> 类型 </exception>
        public override void Add(IComponent component, string name)
        {
            var plugin = component as PlugIn;
            if (plugin != null)
            {
                plugin.Name = name;
                Add(plugin);
            }
            else
            {
                throw new InvalidCastException($"{nameof(component)}不是{typeof(PlugIn).FullName}类型");
            }
        }

        /// <summary>
        /// 确定组件对此容器是否唯一。
        /// </summary>
        /// <param name="component"> </param>
        /// <param name="name"> </param>
        protected override void ValidateName(IComponent component, string name)
        {
            base.ValidateName(component, null);
        }
    }
}