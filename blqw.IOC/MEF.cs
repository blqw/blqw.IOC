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
    sealed class MEF
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
        [Export("MEF.PlugIns")]
        public static PlugInContainer PlugIns { get; } = Initializer();

        /// <summary> 
        /// 初始化
        /// </summary>
        public static PlugInContainer Initializer()
        {
            if (IsInitialized || IsInitializeing)
            {
                return PlugIns;
            }
            var plugins = new PlugInContainer();
            try
            {
                if (Debugger.IsAttached
                    && Debug.Listeners.OfType<ConsoleTraceListener>().Any() == false)
                {
                    Debug.Listeners.Add(new ConsoleTraceListener(true));
                }
                var catalog = GetCatalog();
                plugins.AddCatalog(catalog);
            }
            finally
            {
                IsInitialized = true;
                if (Monitor.IsEntered(_Lock))
                    Monitor.Exit(_Lock);
            }
            return plugins;
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

    }
}