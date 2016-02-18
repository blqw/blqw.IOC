using System;
using System.Collections;
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

        static PlugInContainer _PlugIns;
        /// <summary>
        /// 插件容器
        /// </summary>
        public static PlugInContainer PlugIns
        {
            get
            {
                return _PlugIns ?? (_PlugIns = Container == null ? null : new PlugInContainer(Container.Catalog));
            }
        }


        /// <summary>
        /// 插件容器
        /// </summary>
        public static CompositionContainer Container { get; private set; } = Initializer();

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
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;
            

            foreach (var f in type.GetFields(flags))
            {
                if (f.IsLiteral == true)
                {
                    continue;
                }
                ImportData data;
                var import = f.GetCustomAttribute<ImportAttribute>();
                if (import != null)
                {
                    data = new ImportData(import);
                }
                else
                {
                    var importMany = f.GetCustomAttribute<ImportManyAttribute>();
                    if (import == null)
                    {
                        continue;
                    }
                    data = new ImportData(import);
                }

                data.ResultType = f.FieldType;
                var value = GetExportedValue(data);
                f.SetValue(null, value);
            }
            var args = new object[1];
            foreach (var p in type.GetProperties(flags))
            {
                var set = p.GetSetMethod(true);
                if (set == null)
                {
                    continue;
                }

                ImportData data;
                var import = p.GetCustomAttribute<ImportAttribute>();
                if (import != null)
                {
                    data = new ImportData(import);
                }
                else
                {
                    var importMany = p.GetCustomAttribute<ImportManyAttribute>();
                    if (import == null)
                    {
                        continue;
                    }
                    data = new ImportData(import);
                }

                data.ResultType = p.PropertyType;
                var value = GetExportedValue(data);
                args[0] = value;
                set.Invoke(null, args);
            }
        }

        struct ImportData
        {
            public ImportData(ImportAttribute import)
            {
                AllowRecomposition = import.AllowRecomposition;
                ContractName = import.ContractName;
                Cardinality = ImportCardinality.ZeroOrOne;
                ContractType = import.ContractType;
                RequiredCreationPolicy = import.RequiredCreationPolicy;
                Source = import.Source;
                ResultType = null;
            }

            public ImportData(ImportManyAttribute import)
            {
                AllowRecomposition = import.AllowRecomposition;
                ContractName = import.ContractName;
                Cardinality = ImportCardinality.ZeroOrMore;
                ContractType = import.ContractType;
                RequiredCreationPolicy = import.RequiredCreationPolicy;
                Source = import.Source;
                ResultType = null;
            }

            public bool AllowRecomposition;
            public ImportCardinality Cardinality;
            public string ContractName;
            public Type ContractType;
            public CreationPolicy RequiredCreationPolicy;
            public ImportSource Source;
            public Type ResultType;
        }

        private static object GetExportedValue(ImportData importData)
        {
            Lazy<object>[] exports;
            if (importData.ContractType == null)
            {
                exports = Container.GetExports<object>(importData.ContractName).ToArray();
            }
            else if (importData.ContractName == null)
            {
                exports = Container.GetExports(importData.ContractType ?? importData.ResultType, null, null).ToArray();
            }
            else
            {
                exports = Container.GetExports(importData.ContractType, null, importData.ContractName).ToArray();
            }

            if (importData.Cardinality == ImportCardinality.ExactlyOne)
            {
                switch (exports.Length)
                {
                    case 1:
                        return ConvertExportedValue(exports[0].Value, importData.ResultType);
                    case 0:
                        throw new NotSupportedException("要求返回1个插件,但没有找到任何匹配的输出插件");
                    default:
                        throw new NotSupportedException("要求返回1个插件,但找到多个匹配的输出插件");
                }
            }

            if (exports.Length == 0)
            {
                return null;
            }

            if (importData.Cardinality == ImportCardinality.ZeroOrOne)
            {
                foreach (var export in exports.Reverse())
                {
                    var value = ConvertExportedValue(export.Value, importData.ResultType);
                    if (value != null)
                    {
                        return value;
                    }
                }
                return null;
            }

            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(importData.ResultType));

            foreach (var export in exports)
            {
                var value = ConvertExportedValue(export.Value, importData.ResultType);
                if (value != null)
                {
                    list.Add(value);
                }
            }
            return list;
        }

        private static object ConvertExportedValue(object value, Type type)
        {
            if (value == null)
            {
                return null;
            }
            if (type.IsInstanceOfType(value))
            {
                return value;
            }
            var handler = value as ExportedDelegate;
            if (handler != null && type.IsSubclassOf(typeof(Delegate)))
            {
                return handler.CreateDelegate(type);
            }
            return null;
        }

        class SelectionPriorityContainer : CompositionContainer
        {
            public SelectionPriorityContainer(ComposablePartCatalog catalog)
                : base(catalog)
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

        #region 拓展功能

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

        #endregion

    }
}