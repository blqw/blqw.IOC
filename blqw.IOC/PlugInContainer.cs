using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private readonly AggregateCatalog _CataLog;

        /// <summary>
        /// 插件部件容器
        /// </summary>
        private readonly CompositionContainer _Container;

        private readonly List<Exception> _Exceptions;
        public PlugInContainer()
        {
            _CataLog = new AggregateCatalog();
            _Container = new CompositionContainer(_CataLog);
            _Exceptions = new List<Exception>();
        }

        public PlugInContainer(ComposablePartCatalog catalog)
            : this()
        {
            AddCatalog(catalog);
        }

        /// <summary>
        /// 向容器中增加自定义插件
        /// </summary>
        /// <param name="plugin">插件</param>
        public void Add(PlugIn plugin)
        {
            plugin.NotNull()?.Throw(nameof(plugin));

            if (Components.Cast<PlugIn>().Contains(plugin))
            {
                return; //如果已经存在则忽略本次添加操作
            }
            var existsed = (PlugIn)Components[plugin.Name];
            if (existsed != null)
            {
                if (existsed.Priority < plugin.Priority)
                {
                    PlugIn.Swap(existsed, plugin);
                }
            }
            base.Add(plugin, plugin.Name);
        }

        /// <summary>
        /// 根据名称获取优先级最高的插件
        /// </summary>
        /// <param name="name">插件名称</param>
        /// <returns></returns>
        public PlugIn this[string name]
        {
            get
            {
                return (PlugIn)Components[name];
            }
        }

        /// <summary>
        /// 获取插件导出项
        /// </summary>
        /// <param name="name">插件名称</param>
        /// <param name="type">插件类型</param>
        /// <returns></returns>
        public IEnumerable<object> GetExports(string name, Type type)
        {
            if (type == null || type == typeof(object))
            {
                if (name == null)
                {
                    throw new ArgumentException($"当{nameof(name)}为null时,{nameof(type)}不能为System.Object");
                }
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
            foreach (var export in _Container.GetExports(type, null, name))
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
                    if (name == null || name == plugin.Name)
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
        /// <param name="name">插件名称</param>
        /// <returns></returns>
        public IEnumerable<object> GetExports(string name)
        {
            name.NotNull()?.Throw(nameof(name));
            return GetExports(name, null);
        }

        /// <summary>
        /// 获取插件导出项
        /// </summary>
        /// <param name="type">插件类型</param>
        /// <returns></returns>
        public IEnumerable<object> GetExports(Type type)
        {
            type.NotNull()?.Throw(nameof(type));
            return GetExports(null, type);
        }

        /// <summary>
        /// 获取插件导出项
        /// </summary>
        /// <typeparam name="T">插件类型</typeparam>
        /// <param name="name">插件名称</param>
        /// <returns></returns>
        public IEnumerable<T> GetExports<T>(string name)
        {
            return GetExports(name, typeof(T)).Cast<T>();
        }

        /// <summary>
        /// 获取插件导出项
        /// </summary>
        /// <typeparam name="T">插件类型</typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetExports<T>()
        {
            return GetExports(null, typeof(T)).Cast<T>();
        }

        /// <summary>
        /// 获取优先级最高的一个插件的导出项
        /// </summary>
        /// <param name="name">插件名称</param>
        /// <param name="type">插件类型</param>
        /// <returns></returns>
        public object GetExport(string name, Type type)
        {
            if (type == typeof(object))
            {
                type = null;
            }
            if (name == null && type == null)
            {
                throw new ArgumentException($"当{nameof(name)}为null时,{nameof(type)}不能为System.Object");
            }
            var plugin = this.Where(p => (name == null || name == p.Name) && (type == null || p.IsAcceptType(type))).Max();
            if (plugin == null)
            {
                return null;
            }
            return plugin.GetValue(type);
        }

        /// <summary>
        /// 获取优先级最高的一个插件的导出项
        /// </summary>
        /// <param name="name">插件名称</param>
        /// <returns></returns>
        public object GetExport(string name)
        {
            name.NotNull()?.Throw(nameof(name));
            return GetExport(name, null);
        }

        /// <summary>
        /// 获取优先级最高的一个插件的导出项
        /// </summary>
        /// <param name="type">插件类型</param>
        /// <returns></returns>
        public object GetExport(Type type)
        {
            type.NotNull()?.Throw(nameof(type));
            return GetExport(null, type);
        }

        /// <summary>
        /// 获取优先级最高的一个插件的导出项
        /// </summary>
        /// <typeparam name="T">插件类型</typeparam>
        /// <param name="name">插件名称</param>
        /// <returns></returns>
        public T GetExport<T>(string name)
        {
            return (T)GetExport(name, typeof(T));
        }

        /// <summary>
        /// 获取优先级最高的一个插件的导出项
        /// </summary>
        /// <typeparam name="T">插件类型</typeparam>
        /// <returns></returns>
        public T GetExport<T>()
        {
            return (T)GetExport(null, typeof(T));
        }

        /// <summary>
        /// 添加插件组件部件目录
        /// </summary>
        /// <param name="catalog">对象的可组合部件目录</param>
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

            if (_CataLog.Catalogs.Contains(catalog) == false)
            {
                _CataLog.Catalogs.Add(catalog);
                foreach (var p in catalog)
                {
                    var part = p.CreatePart();
                    foreach (var definition in part.ExportDefinitions)
                    {
                        try
                        {
                            var plugin = new PlugIn(part, definition);
                            plugin.IsComposition = true;
                            Add(plugin);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex.ToString(), "插件载入失败");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 载入插件的异常
        /// </summary>
        public AggregateException Exceptions
        {
            get
            {
                if (_Exceptions == null)
                {
                    return null;
                }
                return new AggregateException("部分插件加载出现错误", _Exceptions);
            }
        }

        /// <summary>
        /// 向容器中增加自定义插件
        /// </summary>
        /// <param name="component">插件</param>
        public override void Add(IComponent component)
        {
            component.Is<PlugIn>()?.Throw(nameof(component));
            Add((PlugIn)component);
        }

        /// <summary>
        /// 向容器中增加自定义插件
        /// </summary>
        /// <param name="component">插件</param>
        /// <param name="name">插件名称</param>
        public override void Add(IComponent component, string name)
        {
            component.Is<PlugIn>()?.Throw(nameof(component));
            ((PlugIn)component).Name = name;
            Add((PlugIn)component);
        }

        /// <summary>
        /// 确定组件对此容器是否唯一。
        /// </summary>
        /// <param name="component"></param>
        /// <param name="name"></param>
        protected override void ValidateName(IComponent component, string name)
        {
            base.ValidateName(component, null);
        }

        /// <summary>
        /// 枚举所有插件
        /// </summary>
        /// <returns></returns>
        public IEnumerator<PlugIn> GetEnumerator()
        {
            foreach (PlugIn plugin in this.Components)
            {
                yield return plugin;
            }
        }

        /// <summary>
        /// 枚举所有插件
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Components.GetEnumerator();
        }
    }
}
