using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
                var import = GetImportDefinition(f, f.FieldType);
                if (import == null)
                {
                    continue;
                }
                var value = GetExportedValue(import, f.FieldType);
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
                var import = GetImportDefinition(p, p.PropertyType);
                if (import == null)
                {
                    continue;
                }
                var value = GetExportedValue(import, p.PropertyType);
                args[0] = value;
                set.Invoke(null, args);
            }
        }

        private static ImportDefinition GetImportDefinition(MemberInfo member, Type resultType)
        {
            var import = member.GetCustomAttribute<ImportAttribute>();
            if (import != null)
            {
                return new ImportDefinition(
                    GetExpression(import.ContractName, import.ContractType, resultType),
                    import.ContractName,
                    ImportCardinality.ZeroOrOne,
                    false,
                    true,
                    null);
            }
            var importMany = member.GetCustomAttribute<ImportManyAttribute>();
            if (import != null)
            {
                return new ImportDefinition(
                    GetExpression(import.ContractName, import.ContractType, resultType),
                    import.ContractName,
                    ImportCardinality.ZeroOrMore,
                    false,
                    true,
                    null);
            }
            return null;
        }




        static readonly MethodInfo _ContainsKey = typeof(IDictionary<string, object>).GetMethod("ContainsKey");

        static readonly MethodInfo _getItem = typeof(IDictionary<string, object>).GetProperties().Where(it => it.GetIndexParameters()?.Length > 0).Select(it => it.GetGetMethod()).First();

        private static Expression<Func<ExportDefinition, bool>> GetExpression(string name, Type contractType, Type resultType)
        {
            var p = Expression.Parameter(typeof(ExportDefinition), "p");
            Expression left = null;
            Expression right = null;
            var type = contractType ?? typeof(object);
            if (name != null)
            {

                var a = Expression.Property(p, "ContractName");
                left = Expression.Equal(a, Expression.Constant(name));
            }
            else if(type == null)
            {
                resultType = type;
            }

            if (type != typeof(object))
            {
                var t = AttributedModelServices.GetTypeIdentity(type);
                var metadata = Expression.Property(p, "Metadata");
                var typeIdentity = Expression.Constant("TypeIdentity");
                var containsKey = Expression.Call(metadata, _ContainsKey, typeIdentity);

                var getItem = Expression.Call(metadata, _getItem, typeIdentity);

                right = Expression.AndAlso(containsKey, Expression.Equal(getItem, Expression.Constant(t)));
            }

            if (left == null && right == null)
            {
                return Expression.Lambda<Func<ExportDefinition, bool>>(Expression.Constant(true), p);
            }

            if (left == null)
            {
                return Expression.Lambda<Func<ExportDefinition, bool>>(right, p);
            }

            if (right == null)
            {
                return Expression.Lambda<Func<ExportDefinition, bool>>(left, p);
            }

            var c = Expression.AndAlso(left, right);
            return Expression.Lambda<Func<ExportDefinition, bool>>(c, p);
        }


        private static object GetExportedValue(ImportDefinition import, Type resultType)
        {
            var exports = Container.GetExports(import);

            if (import.Cardinality == ImportCardinality.ZeroOrMore)
            {
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(resultType));

                foreach (var export in exports)
                {
                    var value = ConvertExportedValue(export.Value, resultType);
                    if (value != null)
                    {
                        list.Add(value);
                    }
                }
                return list;
            }
            
            return ConvertExportedValue(exports.FirstOrDefault()?.Value, resultType);
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
                //var exports = base.GetExportsCore(definition, atomicComposition);
                var exports = base.GetExportsCore(
                                new ImportDefinition(
                                    definition.Constraint,
                                    definition.ContractName,
                                    ImportCardinality.ZeroOrMore,
                                    definition.IsRecomposable,
                                    definition.IsPrerequisite,
                                    definition.Metadata
                                ), atomicComposition);

                if (definition.Cardinality == ImportCardinality.ZeroOrMore)
                {
                    return exports;
                }

                //返回优先级最高的一个或者没有
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