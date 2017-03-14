using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace blqw.IOC
{
    /// <summary>
    /// 用于 <see cref="CallContext"/> 的功能拓展
    /// </summary>
    public static class CallContextEx
    {
        private static readonly Func<Hashtable> _getLogicalDatastore;
        private static readonly Func<Hashtable> _getIllogicalDatastore;

        static CallContextEx()
        {
            var flags = (BindingFlags)(-1);

            var current = typeof(Thread).GetProperty("CurrentThread", flags);
            var getec = typeof(Thread).GetMethod("GetMutableExecutionContext", flags);
            var lcc = typeof(ExecutionContext).GetProperty("LogicalCallContext", flags);
            var icc = typeof(ExecutionContext).GetProperty("IllogicalCallContext", flags);
            var lccdata = lcc.PropertyType.GetProperty("Datastore", flags);
            var iccdata = icc.PropertyType.GetProperty("Datastore", flags);

            {
                var var1 = Expression.Property(null, current);
                var var2 = Expression.Call(var1, getec);
                var var3 = Expression.Property(var2, lcc);
                var var4 = Expression.Property(var3, lccdata);
                _getLogicalDatastore = Expression.Lambda<Func<Hashtable>>(var4).Compile();
            }


            {

                var var1 = Expression.Property(null, current);
                var var2 = Expression.Call(var1, getec);
                var var3 = Expression.Property(var2, icc);
                var var4 = Expression.Property(var3, iccdata);
                _getIllogicalDatastore = Expression.Lambda<Func<Hashtable>>(var4).Compile();
            }

        }

        /// <summary>
        /// 获取使用 <see cref="CallContext.SetData"/> 方法存储的所有对象的名称
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetDataNames()
        {
            foreach (var key in _getIllogicalDatastore().Keys)
            {
                yield return (string)key;
            }
        }

        /// <summary>
        /// 获取使用 <see cref="CallContext.LogicalSetData"/> 方法存储的所有对象的名称
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetLogicalDataNames()
        {
            foreach (var key in _getLogicalDatastore().Keys)
            {
                yield return (string)key;
            }
        }

        /// <summary>
        /// 获取使用 <see cref="CallContext.LogicalSetData"/> 方法存储的所有对象存档文件
        /// </summary>
        /// <returns></returns>
        public static DataArchive ArchiveLogicalData()
        {
            return new DataArchive(_getLogicalDatastore());
        }

        /// <summary>
        /// 清空使用 <see cref="CallContext.LogicalSetData"/> 方法存储的所有对象
        /// </summary>
        /// <returns></returns>
        public static void ClearLogicalData()
        {
            foreach (var name in GetLogicalDataNames().ToArray())
            {
                CallContext.FreeNamedDataSlot(name);
            }
        }

        /// <summary>
        /// 获取使用 <see cref="CallContext.LogicalSetData"/> 方法存储的所有对象存档文件, 并清空已存储的对象
        /// </summary>
        /// <returns></returns>
        public static DataArchive ArchiveAndClearLogicalData()
        {
            var arc = ArchiveLogicalData();
            ClearLogicalData();
            return arc;
        }

        /// <summary>
        /// 使用存档文件恢复 <see cref="CallContext"/> 逻辑上下文中存储的对象
        /// </summary>
        /// <param name="archive"></param>
        public static void RestoreLogicalData(DataArchive archive) => archive.Restore();

        /// <summary>
        /// 数据存档文件
        /// </summary>
        public struct DataArchive
        {
            private readonly Hashtable _table;

            /// <summary>
            /// 新建存档
            /// </summary>
            /// <param name="table"></param>
            internal DataArchive(Hashtable table)
            {
                _table = null;
                if (table?.Count > 0)
                {
                    _table = (Hashtable)table.Clone();
                }
            }

            /// <summary>
            /// 恢复存档
            /// </summary>
            public void Restore()
            {
                if (_table == null || _table.Count == 0)
                {
                    return;
                }
                foreach (DictionaryEntry entry in _table)
                {
                    CallContext.LogicalSetData((string)entry.Key, entry.Value);
                }
                _table.Clear();
            }
        }
    }
}
