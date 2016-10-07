using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
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
        /// <param name="catalog"> 插件目录 </param>
        public PlugInContainer(ComposablePartCatalog catalog)
        {
            _cataLog = new AggregateCatalog();
            _container = new CompositionContainer(_cataLog);
            _exceptions = new List<Exception>();
            AddCatalog(catalog);
        }

        /// <summary>
        /// 根据名称获取优先级最高的插件
        /// </summary>
        /// <param name="name"> 插件名称 </param>
        /// <returns> </returns>
        public PlugIn this[string name] => (PlugIn)Components[name];

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
            var existsed = (PlugIn)Components[plugin.Name];
            if (existsed?.Priority < plugin.Priority)
            {
                PlugIn.Swap(existsed, plugin);
            }
            base.Add(plugin, plugin.Name);
        }

        /// <summary>
        /// 获取导出插件
        /// </summary>
        /// <param name="name">插件约定名称</param>
        /// <param name="type">插件约定类型</param>
        /// <param name="exactType">是否精确匹配类型 </param>
        /// <returns></returns>
        public IEnumerable<PlugIn> GetPlugIns(string name, Type type,bool exactType = true)
        {
            if (type == null)
            {
                type = typeof(object);
            }
            if (string.IsNullOrEmpty(name) && (type == typeof(object)))
            {
                throw new ArgumentNullException($"{nameof(name)} and {nameof(type)}");
            }
            var query = MEF.PlugIns.AsQueryable();
            if (string.IsNullOrEmpty(name) == false)
            {
                query = query.Where(p => p.Name == name);
            }
            if (type != typeof(object))
            {
                if (exactType)
                {
                    var id = AttributedModelServices.GetTypeIdentity(type);
                    query = query.Where(p => p.TypeIdentity == id);
                }
                else
                {
                    query = query.Where(p => p.IsTrueOf(type));
                }
            }
            return query.OrderByDescending(p => p.Priority);
        }

        /// <summary>
        /// 获取导出插件
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerable<PlugIn> GetPlugIns(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            return GetPlugIns(name, null);
        }

        /// <summary>
        /// 获取导出插件
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<PlugIn> GetPlugIns(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return GetPlugIns(null, type);
        }

        /// <summary>
        /// 获取插件导出项
        /// </summary>
        /// <param name="name"> 插件名称 </param>
        /// <param name="type"> 插件类型 </param>
        /// <returns> </returns>
        public IEnumerable<object> GetExports(string name, Type type) => GetPlugIns(name, type).Select(it => it.GetValue(type)).Where(it => it != null);
        

        /// <summary>
        /// 获取插件导出项
        /// </summary>
        /// <param name="name"> 插件名称 </param>
        /// <returns> </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="name" /> is <see langword="null" />. </exception>
        public IEnumerable<object> GetExports(string name) => GetPlugIns(name, null).Select(it => it.GetValue(null)).Where(it => it != null);

        /// <summary>
        /// 获取插件导出项
        /// </summary>
        /// <param name="type"> 插件类型 </param>
        /// <returns> </returns>
        /// <exception cref="ArgumentNullException"> <paramref name="type" /> is <see langword="null" />. </exception>
        public IEnumerable<object> GetExports(Type type) => GetPlugIns(null, type).Select(it => it.GetValue(type)).Where(it => it != null);

        /// <summary>
        /// 获取插件导出项
        /// </summary>
        /// <typeparam name="T"> 插件类型 </typeparam>
        /// <param name="name"> 插件名称 </param>
        /// <returns> </returns>
        public IEnumerable<T> GetExports<T>(string name) => GetExports(name, typeof(T)).Cast<T>();

        /// <summary>
        /// 获取插件导出项
        /// </summary>
        /// <typeparam name="T"> 插件类型 </typeparam>
        /// <returns> </returns>
        public IEnumerable<T> GetExports<T>() => GetExports(null, typeof(T)).Cast<T>();

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
            return this.Where(p => ((name == null) || (name == p.Name)) && ((type == null) || p.IsTrueOf(type)))
                        .OrderByDescending(p => p.Priority)
                        .Select(plugin => plugin.GetValue(type))
                        .FirstOrDefault();
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
        /// <exception cref="ArgumentNullException"> <paramref name="type" /> is <see langword="null" />. </exception>
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
        public T GetExport<T>(string name) => (T)GetExport(name, typeof(T));

        /// <summary>
        /// 获取优先级最高的一个插件的导出项
        /// </summary>
        /// <typeparam name="T"> 插件类型 </typeparam>
        /// <returns> </returns>
        public T GetExport<T>() => (T)GetExport(null, typeof(T));

        /// <summary>
        /// 添加插件组件部件目录
        /// </summary>
        /// <param name="catalog"> 对象的可组合部件目录 </param>
        public void AddCatalog(ComposablePartCatalog catalog)
        {
            if (catalog.Any() == false)
            {
                return;
            }
            var agg = catalog as AggregateCatalog;
            if (agg != null)
            {
                foreach (var log in agg.Catalogs)
                {
                    AddCatalog(log);
                }
                return;
            }

            if (_cataLog.Catalogs.Contains(catalog))
            {
                return;
            }
            _cataLog.Catalogs.Add(catalog);
            foreach (var p in catalog)
            {
                var part = p.CreatePart();
                foreach (var definition in part.ExportDefinitions)
                {
                    try
                    {
                        var plugin = new PlugIn(part, definition) { IsCustom = true };
                        Add(plugin);
                    }
                    catch (Exception ex)
                    {
                        LogServices.Logger?.Write(TraceEventType.Error, "插件载入失败", ex);
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