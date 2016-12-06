using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace blqw.IOC
{
    /// <summary>
    /// 用于执行MEF相关操作
    /// </summary>
    public static class MEF
    {
        /// <summary>
        /// 用于描述 <seealso cref="IDictionary{TKey,TValue}.ContainsKey" /> 方法，用于生成筛选插件的lambda表达式
        /// </summary>
        private static readonly MethodInfo _ContainsKey = typeof(IDictionary<string, object>).GetMethod("ContainsKey");

        /// <summary>
        /// 用于描述 <seealso cref="IDictionary{TKey,TValue}.this" /> get方法，用于生成筛选插件的lambda表达式
        /// </summary>
        private static readonly MethodInfo _GetItem =
            typeof(IDictionary<string, object>).GetProperties()
                .Where(it => it.GetIndexParameters().Length > 0)
                .Select(it => it.GetGetMethod())
                .First();

        /// <summary>
        /// 静态构造函数,初始化插件容器
        /// </summary>
        /// <remarks> 这里不使用直接属性赋值,是因为在某些情况下会出现未知的问题 </remarks>
        static MEF()
        {
            ReInitialization();
        }


        public static void ReInitialization()
        {
            LogServices.Logger?.Write(TraceEventType.Start, "开始初始化MEF");
            var sw = Stopwatch.StartNew();
            var container = GetContainer();
            sw.Stop();
            var time = sw.Elapsed.TotalMilliseconds;
            LogServices.Logger?.Write(TraceEventType.Verbose,
                () => "===插件列表===" + Environment.NewLine + string.Join(Environment.NewLine, Container.Catalog.Parts) +
                      Environment.NewLine + $"=== 共{Container.Catalog.Count()}个 ===");
            LogServices.Logger?.Write(TraceEventType.Stop, () => $"MEF初始化完成, 耗时 {time} ms");
            Container = container;
            _plugIns = null;
        }

        /// <summary>
        /// 插件容器
        /// </summary>
        public static CompositionContainer Container { get; private set; }

        /// <summary>
        /// 尝试添加程序集到哈希表,添加成功返回true,如果程序集已经存在或<paramref name="loaded" />为null,或<paramref name="assembly" />为null,则返回 false,
        /// </summary>
        /// <param name="loaded"> 哈希表 </param>
        /// <param name="assembly"> 添加到哈希表的程序集 </param>
        /// <returns> </returns>
        private static bool TryAdd(this ISet<string> loaded, Assembly assembly)
        {
            if ((loaded == null) || (assembly == null))
            {
                return false;
            }
            var key = assembly.ManifestModule.ModuleVersionId + "," + assembly.ManifestModule.MDStreamVersion;
            if (loaded.Contains(key))
            {
                return false;
            }
            loaded.Add(key);
            return true;
        }


        /// <summary>
        /// 获取插件容器
        /// </summary>
        /// <returns> </returns>
        private static CompositionContainer GetContainer()
        {
            var dir = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory ?? new DirectoryCatalog(".").FullPath;
            var files = new HashSet<string>(
                Directory.EnumerateFiles(dir, "*.dll", SearchOption.AllDirectories)
                    .Union(Directory.EnumerateFiles(dir, "*.exe", SearchOption.AllDirectories))
                , StringComparer.OrdinalIgnoreCase);
            var catalogs = new AggregateCatalog();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var loaded = new HashSet<string>();
            foreach (var a in assemblies)
            {
                if (a.CanLoad() && loaded.TryAdd(a))
                {
                    a.LoadTypes()?.ForEach(catalogs.Catalogs.Add);
                }
                if (a.IsDynamic == false)
                {
                    Uri filePath;
                    if (Uri.TryCreate(a.EscapedCodeBase, UriKind.Absolute, out filePath) && filePath.IsLoopback)
                    {
                        files.Remove(filePath.LocalPath);
                    }
                    files.Remove(a.Location);
                }
            }

            LogServices.Logger?.Write(TraceEventType.Start, $"扫描动态文件 -> 文件个数:{files.Count}");
            if (files.Count > 0)
            {
                var domain = AppDomain.CreateDomain("mef");
                LogServices.Logger?.Write(TraceEventType.Start, "新建临时程序域");
                foreach (var file in files)
                {
                    try
                    {
                        var bytes = File.ReadAllBytes(file);
                        var ass = domain.Load(bytes);
                        if (loaded.TryAdd(ass) && ass.CanLoad())
                        {
                            Assembly.Load(bytes).LoadTypes()?.ForEach(catalogs.Catalogs.Add);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogServices.Logger?.Write(TraceEventType.Error, $"文件加载失败{file}", ex);
                    }
                }
                LogServices.Logger?.Write(TraceEventType.Stop, "卸载程序域");
                AppDomain.Unload(domain);
            }
            LogServices.Logger?.Write(TraceEventType.Stop, "文件处理完成");
            return new SelectionPriorityContainer(catalogs);
        }

        /// <summary>
        /// 过滤系统组件,动态组件,全局缓存组件
        /// </summary>
        /// <param name="assembly"> </param>
        /// <returns> </returns>
        private static bool CanLoad(this Assembly assembly)
        {
            return (assembly.IsDynamic == false)
                    && (assembly.ManifestModule.Name != "<未知>")
                    && (assembly.GlobalAssemblyCache == false)
                    && (assembly.FullName.StartsWith("System.", StringComparison.OrdinalIgnoreCase) == false)
                    && (assembly.FullName.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) == false);
        }

        /// <summary>
        /// 从程序集中加载所有的类型
        /// </summary>
        /// <param name="assembly"> </param>
        /// <returns> </returns>
        private static List<ComposablePartCatalog> LoadTypes(this Assembly assembly)
        {
            if (assembly == null)
            {
                return null;
            }
            LogServices.Logger?.Write(TraceEventType.Start, $"开始装载程序集 -> {assembly.FullName}");
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types;
            }
            catch (Exception ex)
            {
                LogServices.Logger?.Write(TraceEventType.Error, "程序集装载失败", ex);
                return null;
            }
            var list = new List<ComposablePartCatalog>();
            string typeName = null;
            foreach (var type in types)
            {
                try
                {
                    if ((type == null) || Regex.IsMatch(type.FullName, "[^a-zA-Z_`0-9.+]"))
                    {
                        continue;
                    }
                    typeName = type.FullName;
                    list.Add(new TypeCatalog(type));
                    LogServices.Logger?.Write(TraceEventType.Verbose, $"类型装载完成 -> {typeName}");
                }
                catch (Exception ex)
                {
                    if (typeName != null)
                    {
                        LogServices.Logger?.Write(TraceEventType.Error, $"类型装载失败 -> {typeName}", ex);
                    }
                }
            }
            LogServices.Logger?.Write(TraceEventType.Stop, $"程序集装载完成 -> {assembly.FullName}");
            return list;
        }


        /// <summary>
        /// 导入插件
        /// </summary>
        /// <param name="instance"> </param>
        public static void Import(object instance)
        {
            var type = instance as Type;
            if (type != null)
            {
                Import(type);
                return;
            }
            try
            {
                Container.ComposeParts(instance);
                return;
            }
            catch (CompositionException ex)
            {
                LogServices.Logger.Write(TraceEventType.Error, "组合插件失败", ex);
            }
            Import(instance.GetType(), instance);
        }

        /// <summary>
        /// 导入插件
        /// </summary>
        /// <param name="type"> </param>
        /// <param name="instance"> </param>
        /// <exception cref="FieldAccessException">
        /// 在 .NET for Windows Store 应用程序 或 可移植类库 中，请改为捕获基类异常
        /// <see cref="T:System.MemberAccessException" />。调用方没有访问此字段的权限。
        /// </exception>
        public static void Import(Type type, object instance = null)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            if (instance == null)
            {
                flags |= BindingFlags.Static;
            }
            else
            {
                flags |= BindingFlags.Instance;
            }

            foreach (var f in type.GetFields(flags))
            {
                if (f.IsLiteral)
                {
                    continue;
                }
                var import = GetImportDefinition(f, f.FieldType);
                if (import == null)
                {
                    continue;
                }
                var value = GetExportedValue(import);
                if (value != null)
                {
                    f.SetValue(instance, value);
                }
            }
            var args = new object[1];
            foreach (var p in type.GetProperties(flags))
            {
                if (p.GetIndexParameters().Length > 0)
                {
                    continue;
                }
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
                var value = GetExportedValue(import);
                if (value != null)
                {
                    args[0] = value;
                    set.Invoke(instance, args);
                }
            }
        }

        /// <summary>
        /// 根据属性或字段极其类型,返回导入插件的描述信息
        /// </summary>
        /// <param name="member"> 属性或字段 </param>
        /// <param name="memberType"> 属性或字段的类型 </param>
        /// <returns> </returns>
        private static ImportDefinitionImpl GetImportDefinition(MemberInfo member, Type memberType)
        {
            return GetImportDefinition(member.GetCustomAttribute<ImportAttribute>(), memberType)
                   ?? GetImportDefinition(member.GetCustomAttribute<ImportManyAttribute>(), memberType);
        }

        /// <summary>
        /// 根据 <see cref="ImportAttribute" />,返回导入插件的描述信息
        /// </summary>
        /// <param name="import"> 导入描述 </param>
        /// <param name="memberType"> 属性或字段的类型 </param>
        /// <returns> </returns>
        private static ImportDefinitionImpl GetImportDefinition(ImportAttribute import, Type memberType)
        {
            if (import == null)
            {
                return null;
            }

            var name = import.ContractName ?? AttributedModelServices.GetTypeIdentity(import.ContractType ?? memberType);
            return new ImportDefinitionImpl(
                GetExpression(name, import.ContractType ?? memberType),
                import.ContractName,
                ImportCardinality.ZeroOrOne,
                false,
                true,
                null)
            {
                MemberType = memberType,
                ExportedType = memberType
            };
        }

        /// <summary>
        /// 根据 <see cref="ImportManyAttribute" />,返回导入插件的描述信息
        /// </summary>
        /// <param name="import"> 导入描述 </param>
        /// <param name="memberType"> 属性或字段的类型 </param>
        /// <returns> </returns>
        private static ImportDefinitionImpl GetImportDefinition(ImportManyAttribute import, Type memberType)
        {
            if (import == null)
            {
                return null;
            }

            var t = import.ContractType ?? GetActualType(memberType);
            if (t == null)
            {
                return null;
            }
            var name = import.ContractName ?? AttributedModelServices.GetTypeIdentity(t);
            return new ImportDefinitionImpl(
                GetExpression(name, t),
                import.ContractName,
                ImportCardinality.ZeroOrMore,
                false,
                true,
                null)
            {
                MemberType = memberType,
                ExportedType = t
            };
        }

        /// <summary>
        /// 获取当前集合类型的实际元素类型
        /// </summary>
        /// <param name="resultType"> 集合类型 </param>
        /// <returns> </returns>
        private static Type GetActualType(Type resultType)
        {
            if (resultType == null)
            {
                return null;
            }
            if (resultType.IsArray)
            {
                return resultType.GetElementType();
            }

            Type actualType = null; //实际插件类型
            if (resultType.IsInterface)
            {
                actualType = GetInerfaceElementType(resultType);
            }
            foreach (var @interface in resultType.GetInterfaces())
            {
                var elementType = GetInerfaceElementType(@interface);
                if (elementType == null)
                {
                    continue;
                }
                if (actualType == elementType)
                {
                    continue;
                }
                if ((actualType == typeof(object)) || (actualType == null))
                {
                    actualType = elementType;
                }
                else if (elementType != typeof(object))
                {
                    return null;
                }
            }
            return actualType;
        }

        private static Type GetInerfaceElementType(Type interfaceType)
        {
            Type elementType = null;
            if (interfaceType.IsGenericType)
            {
                var raw = interfaceType.GetGenericTypeDefinition();
                if ((raw == typeof(ICollection<>)) || (raw == typeof(IEnumerable<>)))
                {
                    elementType = interfaceType.GetGenericArguments()[0];
                }
            }
            else if ((interfaceType == typeof(ICollection))
                     || (interfaceType == typeof(IEnumerable)))
            {
                elementType = typeof(object);
            }
            return elementType;
        }

        /// <summary>
        /// 获取根据插件导入名称约定和类型约束相匹配导出插件的筛选表达式
        /// </summary>
        /// <param name="contractName"> 约定名称 </param>
        /// <param name="contractType"> 约定类型 </param>
        /// <returns> </returns>
        private static Expression<Func<ExportDefinition, bool>> GetExpression(string contractName, Type contractType)
        {
            var p = Expression.Parameter(typeof(ExportDefinition), "p");
            var typeid = AttributedModelServices.GetTypeIdentity(contractType);
            var a = Expression.Property(p, "ContractName");
            var left = Expression.Equal(a, Expression.Constant(contractName ?? typeid));
            if (contractType == typeof(object))
            {
                return Expression.Lambda<Func<ExportDefinition, bool>>(left, p);
            }
            var metadata = Expression.Property(p, "Metadata");
            var typeIdentity = Expression.Constant("ExportTypeIdentity");
            var containsKey = Expression.Call(metadata, _ContainsKey, typeIdentity);

            var getItem = Expression.Call(metadata, _GetItem, typeIdentity);
            var equals = typeof(string).GetMethod("Equals", new[] { typeof(object) });
            var right = Expression.AndAlso(containsKey, Expression.Call(Expression.Constant(typeid), equals, getItem));

            var c = Expression.AndAlso(left, right);
            return Expression.Lambda<Func<ExportDefinition, bool>>(c, p);
        }

        /// <summary>
        /// 根据导入描述获,和返回类型取导出插件的值
        /// </summary>
        /// <param name="import"> 导入描述 </param>
        /// <returns> </returns>
        private static object GetExportedValue(ImportDefinitionImpl import)
        {
            var exports = Container.GetExports(import);

            if (import.Cardinality == ImportCardinality.ZeroOrMore)
            {
                if (import.MemberType.IsArray || import.MemberType.IsInterface)
                {
                    var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(import.ExportedType));
                    foreach (var export in exports)
                    {
                        var value = ConvertExportedValue(() => export.Value, import.ExportedType);
                        if (value != null)
                        {
                            list.Add(value);
                        }
                    }

                    if (import.MemberType.IsArray)
                    {
                        var array = Array.CreateInstance(import.ExportedType, list.Count);
                        list.CopyTo(array, 0);
                        return array;
                    }
                    return list;
                }
                else
                {
                    dynamic list = Activator.CreateInstance(import.MemberType);
                    foreach (var export in exports)
                    {
                        dynamic value = ConvertExportedValue(() => export.Value, import.ExportedType);
                        if (value != null)
                        {
                            list.Add(value);
                        }
                    }
                    return list;
                }
            }

            return ConvertExportedValue(() => exports.FirstOrDefault()?.Value, import.ExportedType);
        }

        private static object ConvertExportedValue(Func<object> getValue, Type exportedType)
        {
            try
            {
                var value = getValue();
                if (value == null)
                {
                    return null;
                }
                if (exportedType.IsInstanceOfType(value))
                {
                    return value;
                }
                var handler = value as ExportedDelegate;
                if ((handler != null) && exportedType.IsSubclassOf(typeof(Delegate)))
                {
                    return handler.CreateDelegate(exportedType);
                }
            }
            catch (Exception ex)
            {
                LogServices.Logger.Write(TraceEventType.Error, "组合插件失败", ex);
            }

            return null;
        }

        /// <summary>
        /// 增加2个额外属性
        /// </summary>
        private class ImportDefinitionImpl : ImportDefinition
        {
            public ImportDefinitionImpl(Expression<Func<ExportDefinition, bool>> constraint, string contractName,
                ImportCardinality cardinality, bool isRecomposable, bool isPrerequisite,
                IDictionary<string, object> metadata)
                : base(constraint, contractName, cardinality, isRecomposable, isPrerequisite, metadata)
            {
            }

            /// <summary>
            /// 导入插件的字段或属性的类型
            /// </summary>
            public Type MemberType { get; set; }

            /// <summary>
            /// 导出插件的类型
            /// </summary>
            public Type ExportedType { get; set; }
        }

        /// <summary>
        /// 按优先级过滤插件
        /// </summary>
        private class SelectionPriorityContainer : CompositionContainer
        {
            public SelectionPriorityContainer(ComposablePartCatalog catalog)
                : base(catalog)
            {
            }

            protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition,
                AtomicComposition atomicComposition)
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
                //Trace.WriteLine(definition.Constraint.ToString(), "1");

                if (definition.Cardinality == ImportCardinality.ZeroOrMore)
                {
                    return exports;
                }

                //返回优先级最高的一个或者没有
                return exports?.OrderByDescending(it =>
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

        private static PlugInContainer _plugIns;

        /// <summary>
        /// 插件容器
        /// </summary>
        public static PlugInContainer PlugIns
        {
            get
            {
                if (_plugIns == null)
                {
                    return _plugIns = new PlugInContainer(Container.Catalog);
                }
                else if (_plugIns.Any(it => it.Invalid)) //如果存在无效插件,则重载MEF
                {
                    ReInitialization();
                    return _plugIns = new PlugInContainer(Container.Catalog);
                }
                return _plugIns;
            }
        }

        #endregion
    }
}