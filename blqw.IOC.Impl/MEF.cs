using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace blqw.IOC.Impl
{
    /// <summary>
    /// 用于执行MEF相关操作
    /// </summary>
    [Export("MEF")]
    public sealed class MEF
    {
        /// <summary>
        /// 字符串锁
        /// </summary>
        const string _Lock = "O[ON}:z05i$*H75O[bJdnedei#('i_i^";

        /// <summary> 
        /// 是否已初始化完成
        /// </summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// 是否正在初始化
        /// </summary>
        /// <returns></returns>
        public static bool IsInitializeing
        {
            get
            {
                if (IsInitialized)
                {
                    return false;
                }
                if (Monitor.IsEntered(_Lock))
                {
                    return true;
                }
                if (Monitor.TryEnter(_Lock))
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 插件容器
        /// </summary>
        public static CompositionContainer Container { get; } = Initializer();

        /// <summary> 
        /// 初始化
        /// </summary>
        public static CompositionContainer Initializer()
        {
            if (IsInitialized || IsInitializeing)
            {
                return Container;
            }
            try
            {
                if (Debugger.IsAttached
                     && Debug.Listeners.OfType<ConsoleTraceListener>().Any() == false)
                {
                    Debug.Listeners.Add(new ConsoleTraceListener(true));
                }
                var catalog = GetCatalog();
                var container = new SelectionPriorityContainer(catalog);
                var args = new object[] { container };
                foreach (var mef in container.GetExportedValues<object>("MEF"))
                {
                    var type = mef.GetType();
                    if (type == typeof(MEF))
                    {
                        continue;
                    }
                    var p = type.GetProperty("Container");
                    if (p != null && p.PropertyType == typeof(CompositionContainer))
                    {
                        var set = p.GetSetMethod(true);
                        if (set != null)
                        {
                            set.Invoke(null, args);
                        }
                    }
                }
                return container;
            }
            finally
            {
                IsInitialized = true;
                if (Monitor.IsEntered(_Lock))
                    Monitor.Exit(_Lock);
            }
        }

        /// <summary> 获取插件
        /// </summary>
        /// <returns></returns>
        private static ComposablePartCatalog GetCatalog()
        {
            var dir = new DirectoryCatalog(".").FullPath;
            var files = Directory.EnumerateFiles(dir, "*.dll", SearchOption.AllDirectories)
                .Union(Directory.EnumerateFiles(dir, "*.exe", SearchOption.AllDirectories));
            var logs = new AggregateCatalog();
            foreach (var file in files)
            {
                try
                {
                    var asmCat = new AssemblyCatalog(file);
                    if (asmCat.Parts.ToList().Count > 0)
                        logs.Catalogs.Add(asmCat);
                }
                catch (Exception)
                {
                }
            }
            return logs;
        }

        /// <summary>
        /// 导入插件
        /// </summary>
        /// <param name="instance"></param>
        public static void Import(object instance)
        {
            if (instance == null)
            {
                return;
            }

            var type = instance as Type;
            if (type != null)
            {
                Import(type);
                return;
            }

            Container.ComposeParts(instance);
        }

        /// <summary>
        /// 导入插件
        /// </summary>
        /// <param name="type"></param>
        public static void Import(Type type)
        {
            if (type == null)
            {
                return;
            }

            //const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

            ////获取所有标记为 Component 的类
            //var configs = container.GetExportedValues<object>(null);
            //foreach (var item in configs)
            //{
            //    //var type = item.GetType();
            //    //foreach (var f in type.GetFields(flags))
            //    //{
            //    //    var value = GetExportedValue(container, f, f.FieldType);
            //    //    if (value != null)
            //    //    {
            //    //        f.SetValue(null, value);
            //    //    }
            //    //}
            //    //foreach (var p in type.GetProperties(flags))
            //    //{
            //    //    var value = GetExportedValue(container, p, p.PropertyType);
            //    //    if (value != null)
            //    //    {
            //    //        p.SetValue(null, value);
            //    //    }
            //    //}
            //    var a = AttributedModelServices.CreatePartDefinition(item.GetType(), null, true);
            //    var b = AttributedModelServices.CreatePart(item);
            //    var c = AttributedModelServices.GetMetadataView<xxxxx>(b.ExportDefinitions.First().Metadata);
            //    container.ComposeParts(item);
            //}
        }


        class SelectionPriorityContainer : CompositionContainer
        {
            public SelectionPriorityContainer(ComposablePartCatalog catalog)
                :base(catalog)
            {

            }
            protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
            {
                var exports = base.GetExportsCore(definition, atomicComposition);
                if (definition.Cardinality == ImportCardinality.ZeroOrMore)
                {
                    return exports;
                }
                return exports.OrderByDescending(it =>
                {
                    object priority;
                    if (it.Metadata.TryGetValue("Priority", out priority))
                    {
                        return priority;
                    }
                    return 0;
                }).Take(1).ToArray();
            }
        }

        
        /// <summary> 获取插件实体
        /// </summary>
        /// <param name="container">插件容器</param>
        /// <param name="member">字段或属性</param>
        /// <param name="type">字段或属性的类型</param>
        private static object GetExportedValue(CompositionContainer container, MemberInfo member, Type type)
        {
            var attr = member.GetCustomAttribute<ImportAttribute>();
            if (attr == null)
            {
                return null;
            }

            Lazy<object>[] exports;
            var name = attr.ContractName; //名称去空格
            if (name != null)
            {
                name = name.Trim();
                exports = container.GetExports<object>(attr.ContractName).ToArray();
            }
            else
            {
                exports = container.GetExports(attr.ContractType ?? type, null, null).ToArray();
            }


            if (exports.Length == 0)
            {
                return null;
            }

            foreach (var export in exports.Reverse())
            {
                var handler = export.Value as ExportedDelegate;
                if (handler != null)
                {
                    var func = handler.CreateDelegate(type);
                    if (func != null)
                    {
                        return func;
                    }
                }
            }
            return exports.Last().Value;
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="container"></param>
        ///// <param name="member"></param>
        ///// <param name="type"></param>
        ///// <returns></returns>
        //private static object GetAppSettingsValue(CompositionContainer container, MemberInfo member, Type type)
        //{
        //    var attr = member.GetCustomAttribute<ImportAttribute>();
        //    if (attr == null)
        //    {
        //        return null;
        //    }
        //    var appsettings = System.Configuration.ConfigurationManager.AppSettings;
        //    var name = member.ReflectedType.FullName + "." + member.Name;
        //    var setting = appsettings[name] ?? appsettings[attr.ContractName];
        //    if (setting == null)
        //    {
        //        return null;
        //    }
        //    return Component.Converter.Convert(setting, type);
        //}


        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="container"></param>
        //private static void LoadConfig(CompositionContainer container)
        //{
        //    //获取所有标记为 Config 的类
        //    var configs = container.GetExportedValues<object>("Config");
        //    const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;
        //    foreach (var item in configs)
        //    {
        //        var type = item.GetType();
        //        foreach (var f in type.GetFields(flags))
        //        {
        //            var value = GetAppSettingsValue(container, f, f.FieldType);
        //            if (value != null)
        //            {
        //                f.SetValue(null, value);
        //            }
        //        }
        //        foreach (var p in type.GetProperties(flags))
        //        {
        //            var value = GetAppSettingsValue(container, p, p.PropertyType);
        //            if (value != null)
        //            {
        //                p.SetValue(null, value);
        //            }
        //        }
        //    }
        //}
    }
}