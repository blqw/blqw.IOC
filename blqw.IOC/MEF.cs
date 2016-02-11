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

namespace blqw.IOC
{
    /// <summary>
    /// 用于执行MEF相关操作
    /// </summary>
    [Export("Component")]
    public sealed class MEF
    {

        const string GLOBAL_KEY = "O[ON}:z05i$*H75O[bJdnedei#('i_i^";

        /// <summary> 获取默认值
        /// </summary>
        [Import("MEF_Initialized_" + GLOBAL_KEY)]
        static readonly bool IsInitialized = false;

        [Export("MEF_Initialized_" + GLOBAL_KEY)]
        [ExportMetadata("Priority", int.MaxValue)]
        static bool _IsInitialized = true;

        private static bool IsLoading()
        {
            if (Monitor.IsEntered(GLOBAL_KEY))
            {
                return true;
            }
            if (Monitor.TryEnter(GLOBAL_KEY))
            {
                return false;
            }
            return true;
        }

        public static PlugInContainer PlugIns { get; private set; }

        /// <summary> 初始化
        /// </summary>
        public static void Initializer()
        {
            if (IsInitialized || IsLoading())
            {
                return;
            }
            PlugIns = new PlugInContainer();
            try
            {
                if (Debugger.IsAttached)
                {
                    Debug.Listeners.Add(new ConsoleTraceListener(true));
                }
                var catalog = GetCatalog();
                PlugIns.AddCatalog(catalog);
                //foreach (var item in catalog)
                //{
                //    PlugIns.Adds(item.CreatePart());
                //}
                //过滤优先级太低的插件
                //catalog = FilterLower(catalog);
                //将插件加入容器
                //var container = new CompositionContainer(catalog);
                //LoadComponent(container);
                //LoadConfig(container);
            }
            finally
            {
                if (Monitor.IsEntered(GLOBAL_KEY))
                    Monitor.Exit(GLOBAL_KEY);
            }
        }

        /// <summary> 获取插件
        /// </summary>
        static ComposablePartCatalog AllCatalogs;

        /// <summary> 获取插件
        /// </summary>
        /// <returns></returns>
        private static ComposablePartCatalog GetCatalog()
        {
            if (AllCatalogs != null) return AllCatalogs;
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
            return AllCatalogs = logs;
        }

        /// <summary> 过滤优先级太低的插件
        /// </summary>
        /// <param name="catalog"></param>
        /// <returns></returns>
        private static ComposablePartCatalog FilterLower(ComposablePartCatalog catalog)
        {
            var priorities = new Dictionary<string, int>(); //优先级列表
            //选举优先级最高的插件
            foreach (var p in catalog.Parts)
            {
                foreach (var e in p.ExportDefinitions)
                {
                    object value;
                    e.Metadata.TryGetValue("Priority", out value);
                    int v1;
                    int.TryParse(value + "", out v1);
                    int V2;
                    priorities.TryGetValue(e.ContractName, out V2);
                    priorities[e.ContractName] = Math.Max(v1, V2);
                }
            }

            //过滤插件
            return new FilteredCatalog(catalog, p =>
            {
                bool r = true;
                foreach (var e in p.ExportDefinitions)
                {
                    object value;
                    e.Metadata.TryGetValue("Priority", out value);
                    int n;
                    int.TryParse(value + "", out n);
                    if (priorities[e.ContractName] == n)
                    {
                        return true;
                    }
                    r = false;
                }
                return r;
            });
        }

        /// <summary> 获取插件实体
        /// </summary>
        /// <param name="container">插件容器</param>
        /// <param name="member">字段或属性</param>
        /// <param name="type">字段或属性的类型</param>
        private static object GetExportedValue(CompositionContainer container, MemberInfo member, Type type)
        {
            var attr = (ImportAttribute)Attribute.GetCustomAttribute(member, typeof(ImportAttribute));
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

        private static object GetAppSettingsValue(CompositionContainer container, MemberInfo member, Type type)
        {
            var attr = (ImportAttribute)Attribute.GetCustomAttribute(member, typeof(ImportAttribute));
            if (attr == null)
            {
                return null;
            }
            var appsettings = System.Configuration.ConfigurationManager.AppSettings;
            var name = member.ReflectedType.FullName + "." + member.Name;
            var setting = appsettings[name] ?? appsettings[attr.ContractName];
            if (setting == null)
            {
                return null;
            }
            return null;
            //return Component.Converter.Convert(setting, type);
        }

        private static void LoadComponent(CompositionContainer container)
        {
            //获取所有标记为 Component 的类
            //var configs = new[] { new Component() }.Union(container.GetExportedValues<object>("Component"));
            var configs = container.GetExportedValues<object>("Component");
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;
            foreach (var item in configs)
            {
                var type = item.GetType();
                foreach (var f in type.GetFields(flags))
                {
                    var value = GetExportedValue(container, f, f.FieldType);
                    if (value != null)
                    {
                        f.SetValue(null, value);
                    }
                }
                foreach (var p in type.GetProperties(flags))
                {
                    var value = GetExportedValue(container, p, p.PropertyType);
                    if (value != null)
                    {
                        p.SetValue(null, value, null);
                    }
                }
                container.ComposeParts(item);
            }
        }

        private static void LoadConfig(CompositionContainer container)
        {
            //获取所有标记为 Config 的类
            var configs = container.GetExportedValues<object>("Config");
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;
            foreach (var item in configs)
            {
                var type = item.GetType();
                foreach (var f in type.GetFields(flags))
                {
                    var value = GetAppSettingsValue(container, f, f.FieldType);
                    if (value != null)
                    {
                        f.SetValue(null, value);
                    }
                }
                foreach (var p in type.GetProperties(flags))
                {
                    var value = GetAppSettingsValue(container, p, p.PropertyType);
                    if (value != null)
                    {
                        p.SetValue(null, value, null);
                    }
                }
            }
        }
    }
}