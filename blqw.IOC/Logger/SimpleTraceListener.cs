using System;
using System.Diagnostics;

namespace blqw.IOC
{
    /// <summary>
    /// 一个被简化的日志侦听器基类
    /// </summary>
    public abstract class SimpleTraceListener : TraceListener
    {
        /// <summary>
        /// 在派生类中被重写时，向在该派生类中所创建的侦听器写入指定消息。
        /// </summary>
        /// <param name="cache"> 包含当前进程 ID、线程 ID 以及堆栈跟踪信息的 <see cref="T:System.Diagnostics.TraceEventCache" /> 对象。 </param>
        /// <param name="sourceOrCategory"> 标识输出时使用的名称或类别名称，通常为生成跟踪事件的应用程序的名称。 </param>
        /// <param name="eventType"> <see cref="T:System.Diagnostics.TraceEventType" /> 值之一，指定引发跟踪的事件类型。 </param>
        /// <param name="id"> 事件的数值标识符。 </param>
        /// <param name="message"> 要写入的消息。 </param>
        /// <param name="data1"> 要发出的跟踪数据。 </param>
        /// <param name="data"> 要作为数据发出的对象数组。 </param>
        public virtual void Write(TraceEventCache cache, string sourceOrCategory, TraceEventType eventType, int id, string message, object data1, object[] data)
            => WriteLine(cache, sourceOrCategory, eventType, id, message, data1, data);

        /// <summary>
        /// 在派生类中被重写时，向在该派生类中所创建的侦听器写入指定消息。
        /// </summary>
        /// <param name="cache"> 包含当前进程 ID、线程 ID 以及堆栈跟踪信息的 <see cref="T:System.Diagnostics.TraceEventCache" /> 对象。 </param>
        /// <param name="sourceOrCategory"> 标识输出时使用的名称或类别名称，通常为生成跟踪事件的应用程序的名称。 </param>
        /// <param name="eventType"> <see cref="T:System.Diagnostics.TraceEventType" /> 值之一，指定引发跟踪的事件类型。 </param>
        /// <param name="id"> 事件的数值标识符。 </param>
        /// <param name="message"> 要写入的消息。 </param>
        /// <param name="data1"> 要发出的跟踪数据。 </param>
        /// <param name="data"> 要作为数据发出的对象数组。 </param>
        public abstract void WriteLine(TraceEventCache cache, string sourceOrCategory, TraceEventType eventType, int id, string message, object data1, object[] data);

        /// <summary>
        /// 在派生类中被重写时，向在该派生类中所创建的侦听器写入指定消息。
        /// </summary>
        /// <param name="message"> 要写入的消息。 </param>
        public sealed override void Write(string message) => WriteImpl(TraceEventType.Verbose, message: message);

        /// <summary>
        /// 实现 <see cref="M:System.Object.ToString" /> 类时，向所创建的侦听器写入对象的 <see cref="T:System.Diagnostics.TraceListener" /> 方法值。
        /// </summary>
        /// <param name="o"> 要为其编写完全限定类名的 <see cref="T:System.Object" />。 </param>
        public sealed override void Write(object o) => WriteImpl(TraceEventType.Verbose, data1: o);

        /// <summary>
        /// 实现 <see cref="T:System.Diagnostics.TraceListener" /> 类时，向所创建的侦听器写入类别名称和消息。
        /// </summary>
        /// <param name="message"> 要写入的消息。 </param>
        /// <param name="category"> 用于组织输出的类别名称。 </param>
        public sealed override void Write(string message, string category) => WriteImpl(TraceEventType.Verbose, message: message, sourceOrCategory: category);

        /// <summary>
        /// 实现 <see cref="T:System.Diagnostics.TraceListener" /> 类时，向所创建的侦听器写入类别名称和对象的 <see cref="M:System.Object.ToString" /> 方法值。
        /// </summary>
        /// <param name="o"> 要为其编写完全限定类名的 <see cref="T:System.Object" />。 </param>
        /// <param name="category"> 用于组织输出的类别名称。 </param>
        public sealed override void Write(object o, string category) => WriteImpl(TraceEventType.Verbose, data1: o, sourceOrCategory: category);

        /// <summary>
        /// 实现 <see cref="T:System.Diagnostics.TraceListener" /> 类时，向所创建的侦听器发出错误信息。
        /// </summary>
        /// <param name="message"> 要发出的消息。 </param>
        public sealed override void Fail(string message) => WriteLineImpl(TraceEventType.Error, message: message);

        /// <summary>
        /// 实现 <see cref="T:System.Diagnostics.TraceListener" /> 类时，向所创建的侦听器发出错误信息和详细错误信息。
        /// </summary>
        /// <param name="message"> 要发出的消息。 </param>
        /// <param name="detailMessage"> 要发出的详细消息。 </param>
        public sealed override void Fail(string message, string detailMessage) => WriteLineImpl(TraceEventType.Error, message: message, data1: detailMessage);

        /// <summary>
        /// 向特定于侦听器的输出中写入跟踪信息、数据对象和事件信息。
        /// </summary>
        /// <param name="eventCache"> 包含当前进程 ID、线程 ID 以及堆栈跟踪信息的 <see cref="T:System.Diagnostics.TraceEventCache" /> 对象。 </param>
        /// <param name="source"> 标识输出时使用的名称，通常为生成跟踪事件的应用程序的名称。 </param>
        /// <param name="eventType">
        /// <see cref="T:System.Diagnostics.TraceEventType" /> 值之一，指定引发跟踪的事件类型。
        /// </param>
        /// <param name="id"> 事件的数值标识符。 </param>
        /// <param name="data"> 要发出的跟踪数据。 </param>
        public sealed override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
            => WriteLineImpl(eventType, eventCache, source, id, data1: data);

        private void WriteImpl(TraceEventType eventType, TraceEventCache cache = null, string sourceOrCategory = null, int id = 0, string message = null, object[] args = null, object data1 = null, object[] data = null)
        {
            if (Filter.ShouldTrace(cache, sourceOrCategory, eventType, id, message, args, data1, data))
            {
                if (args != null)
                {
                    message = string.Format(message, args);
                }
                Write(cache, sourceOrCategory, eventType, id, message, data1, data);
            }
        }

        private void WriteLineImpl(TraceEventType eventType, TraceEventCache cache = null, string sourceOrCategory = null, int id = 0, string message = null, object[] args = null, object data1 = null, object[] data = null)
        {
            if (Filter.ShouldTrace(cache, sourceOrCategory, eventType, id, message, args, data1, data))
            {
                if (args != null)
                {
                    message = string.Format(message, args);
                }
                WriteLine(cache, sourceOrCategory, eventType, id, message, data1, data);
            }
        }

        /// <summary>
        /// 向特定于侦听器的输出中写入跟踪信息、数据对象的数组和事件信息。
        /// </summary>
        /// <param name="eventCache"> 包含当前进程 ID、线程 ID 以及堆栈跟踪信息的 <see cref="T:System.Diagnostics.TraceEventCache" /> 对象。 </param>
        /// <param name="source"> 标识输出时使用的名称，通常为生成跟踪事件的应用程序的名称。 </param>
        /// <param name="eventType">
        /// <see cref="T:System.Diagnostics.TraceEventType" /> 值之一，指定引发跟踪的事件类型。
        /// </param>
        /// <param name="id"> 事件的数值标识符。 </param>
        /// <param name="data"> 要作为数据发出的对象数组。 </param>
        /// <PermissionSet>
        ///     <IPermission
        ///         class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///         version="1" Unrestricted="true" />
        ///     <IPermission
        ///         class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///         version="1" Flags="UnmanagedCode" />
        /// </PermissionSet>
        public sealed override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
            => WriteLineImpl(eventType, eventCache, source, id, data: data);

        /// <summary>
        /// 向特定于侦听器的输出写入跟踪和事件信息。
        /// </summary>
        /// <param name="eventCache"> 包含当前进程 ID、线程 ID 以及堆栈跟踪信息的 <see cref="T:System.Diagnostics.TraceEventCache" /> 对象。 </param>
        /// <param name="source"> 标识输出时使用的名称，通常为生成跟踪事件的应用程序的名称。 </param>
        /// <param name="eventType">
        /// <see cref="T:System.Diagnostics.TraceEventType" /> 值之一，指定引发跟踪的事件类型。
        /// </param>
        /// <param name="id"> 事件的数值标识符。 </param>
        /// <PermissionSet>
        ///     <IPermission
        ///         class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///         version="1" Unrestricted="true" />
        ///     <IPermission
        ///         class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///         version="1" Flags="UnmanagedCode" />
        /// </PermissionSet>
        public sealed override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
            => WriteLineImpl(eventType, eventCache, source, id);

        /// <summary>
        /// 向特定于侦听器的输出中写入跟踪信息、消息和事件信息。
        /// </summary>
        /// <param name="eventCache"> 包含当前进程 ID、线程 ID 以及堆栈跟踪信息的 <see cref="T:System.Diagnostics.TraceEventCache" /> 对象。 </param>
        /// <param name="source"> 标识输出时使用的名称，通常为生成跟踪事件的应用程序的名称。 </param>
        /// <param name="eventType">
        /// <see cref="T:System.Diagnostics.TraceEventType" /> 值之一，指定引发跟踪的事件类型。
        /// </param>
        /// <param name="id"> 事件的数值标识符。 </param>
        /// <param name="message"> 要写入的消息。 </param>
        /// <PermissionSet>
        ///     <IPermission
        ///         class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///         version="1" Unrestricted="true" />
        ///     <IPermission
        ///         class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///         version="1" Flags="UnmanagedCode" />
        /// </PermissionSet>
        public sealed override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
            => WriteLineImpl(eventType, eventCache, source, id, message);

        /// <summary>
        /// 向特定于侦听器的输出中写入跟踪信息、格式化对象数组和事件信息。
        /// </summary>
        /// <param name="eventCache"> 包含当前进程 ID、线程 ID 以及堆栈跟踪信息的 <see cref="T:System.Diagnostics.TraceEventCache" /> 对象。 </param>
        /// <param name="source"> 标识输出时使用的名称，通常为生成跟踪事件的应用程序的名称。 </param>
        /// <param name="eventType">
        /// <see cref="T:System.Diagnostics.TraceEventType" /> 值之一，指定引发跟踪的事件类型。
        /// </param>
        /// <param name="id"> 事件的数值标识符。 </param>
        /// <param name="format"> 一个格式字符串，其中包含零个或多个格式项，它们对应于 <paramref name="args" /> 数组中的对象。 </param>
        /// <param name="args"> 包含零个或多个要格式化的对象的 object 数组。 </param>
        /// <PermissionSet>
        ///     <IPermission
        ///         class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///         version="1" Unrestricted="true" />
        ///     <IPermission
        ///         class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///         version="1" Flags="UnmanagedCode" />
        /// </PermissionSet>
        public sealed override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
            => WriteLineImpl(eventType, eventCache, source, id, format, args);

        /// <summary>
        /// 向侦听器特定的输出中写入跟踪信息、消息、相关活动标识和事件信息。
        /// </summary>
        /// <param name="eventCache"> 包含当前进程 ID、线程 ID 以及堆栈跟踪信息的 <see cref="T:System.Diagnostics.TraceEventCache" /> 对象。 </param>
        /// <param name="source"> 标识输出时使用的名称，通常为生成跟踪事件的应用程序的名称。 </param>
        /// <param name="id"> 事件的数值标识符。 </param>
        /// <param name="message"> 要写入的消息。 </param>
        /// <param name="relatedActivityId"> 标识相关活动的 <see cref="T:System.Guid" /> 对象。 </param>
        public sealed override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
            => WriteLineImpl(TraceEventType.Transfer, eventCache, source, id, data1: relatedActivityId);

        /// <summary>
        /// 实现 <see cref="T:System.Diagnostics.TraceListener" /> 类时，向所创建的侦听器写入对象的 <see cref="M:System.Object.ToString" />
        /// 方法值，后跟行结束符。
        /// </summary>
        /// <param name="o"> 要为其编写完全限定类名的 <see cref="T:System.Object" />。 </param>
        public sealed override void WriteLine(object o) => WriteLineImpl(TraceEventType.Verbose, data1: o);

        /// <summary>
        /// 在派生类中被重写时，向在该派生类中所创建的侦听器写入消息，后跟行结束符。
        /// </summary>
        /// <param name="message"> 要写入的消息。 </param>
        public sealed override void WriteLine(string message) => WriteLineImpl(TraceEventType.Verbose, message: message);

        /// <summary>
        /// 实现 <see cref="T:System.Diagnostics.TraceListener" /> 类时，向所创建的侦听器写入类别名称和消息，后跟行结束符。
        /// </summary>
        /// <param name="message"> 要写入的消息。 </param>
        /// <param name="category"> 用于组织输出的类别名称。 </param>
        public sealed override void WriteLine(string message, string category) => WriteLineImpl(TraceEventType.Verbose, message: message,sourceOrCategory: category);

        /// <summary>
        /// 实现 <see cref="T:System.Diagnostics.TraceListener" /> 类时，向所创建的侦听器写入类别名称和对象的 <see cref="M:System.Object.ToString" />
        /// 方法值，后跟行结束符。
        /// </summary>
        /// <param name="o"> 要为其编写完全限定类名的 <see cref="T:System.Object" />。 </param>
        /// <param name="category"> 用于组织输出的类别名称。 </param>
        public sealed override void WriteLine(object o, string category) => WriteLineImpl(TraceEventType.Verbose, data1: o, sourceOrCategory: category);
    }
}