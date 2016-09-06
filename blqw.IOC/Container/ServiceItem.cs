using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Runtime.Serialization;

namespace blqw.IOC
{
    /// <summary>
    /// 这是一个包装类,当包装对象发生更新时,会触发固定事件
    /// </summary>
    public sealed class ServiceItem : IObjectHandle, INotifyPropertyChanged, IObjectReference, IServiceProvider
    {
        /// <summary>
        /// 系统值
        /// </summary>
        /// <remarks>当系统对象被替换后,还原时使用</remarks>
        private object _systemValue;
        /// <summary>
        /// 当前值
        /// </summary>
        private object _value;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="container"> 服务组件容器 </param>
        /// <param name="serviceType"> 服务组件类型 </param>
        /// <param name="value"> 服务组件 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="container" /> is <see langword="null" />. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="serviceType" /> is <see langword="null" />. </exception>
        /// <exception cref="ArgumentException"><see cref="serviceType"/> 为 <seealso cref="ServiceItem"/> 类型.</exception>
        public ServiceItem(IServiceContainer container, Type serviceType, object value)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            if (serviceType == typeof(ServiceItem))
            {
                throw new ArgumentException($"{nameof(serviceType)}不能是{nameof(ServiceItem)}类型");
            }
            Container = container;
            ServiceType = serviceType;
            _value = value;
        }

        /// <summary>
        /// 服务容器
        /// </summary>
        public IServiceContainer Container { get; }

        /// <summary>
        /// 服务约定类型
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        /// 服务组件
        /// </summary>
        /// <exception cref="Exception"> 获取服务组件时,如果构造函数中传入的 value 为 <seealso cref="ServiceCreatorCallback" /> 类型,且执行中出现异常. </exception>
        /// <remarks>如果构造函数中传入的 value 为 <seealso cref="ServiceCreatorCallback" /> 类型,则执行委托得到真实值</remarks>
        public object Value
        {
            get
            {
                var call = _value as ServiceCreatorCallback;
                if (call != null)
                {
                    _value = call(Container, ServiceType);
                }
                return _value;
            }
            set
            {
                //当服务被置空,如果是系统服务则还原
                if ((value == null) && (_systemValue != null))
                {
                    if (IsSystem)
                    {
                        return;
                    }
                    _value = _systemValue;
                    IsSystem = true;
                }
                else if (_value != value)
                {
                    //系统服务器被替换后,标识为"非系统服务"
                    if (IsSystem)
                    {
                        IsSystem = false;
                    }
                    _value = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 是否为系统服务(默认:false)
        /// </summary>
        public bool IsSystem { get; private set; }

        /// <summary>
        /// 是否自动更新值(默认:true)
        /// </summary>
        public bool AutoUpdate { get; internal set; } = true;

        /// <summary>
        /// 在属性值更改时发生。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        object IObjectHandle.Unwrap() => _value;

        object IObjectReference.GetRealObject(StreamingContext context) => _value;

        /// <summary>
        /// 获取指定类型的服务对象。
        /// </summary>
        /// <returns>
        /// <paramref name="serviceType" /> 类型的服务对象。- 或 -如果没有 <paramref name="serviceType" /> 类型的服务对象，则为 null。
        /// </returns>
        /// <param name="serviceType"> 一个对象，它指定要获取的服务对象的类型。 </param>
        /// <exception cref="Exception"> 构造函数中传入的 value 为 <seealso cref="ServiceCreatorCallback" /> 类型,且执行中出现异常. </exception>
        public object GetService(Type serviceType)
        {
            if (serviceType == ServiceType)
            {
                return this;
            }
            return (Value as IServiceProvider)?.GetService(ServiceType); //生成新的服务
        }

        /// <summary>
        /// 获取指定类型的服务对象。
        /// </summary>
        /// <returns>
        /// <paramref name="serviceType" /> 类型的服务对象。- 或 -如果没有 <paramref name="serviceType" /> 类型的服务对象，则为 null。
        /// </returns>
        /// <param name="serviceType"> 一个对象，它指定要获取的服务对象的类型。 </param>
        /// <exception cref="Exception"> 构造函数中传入的 value 为 <seealso cref="ServiceCreatorCallback" /> 类型,且执行中出现异常. </exception>
        /// <exception cref="ArgumentException"><see cref="serviceType"/> 为 <seealso cref="ServiceItem"/> 类型.</exception>
        public ServiceItem GetServiceItem(Type serviceType)
        {
            if (serviceType == ServiceType)
            {
                throw new ArgumentException($"{nameof(serviceType)}不能是{nameof(ServiceItem)}类型");
            }
            var child = (Value as IServiceProvider)?.GetService(ServiceType); //生成新的服务

            if (child == null)
            {
                return null;
            }

            var item = new ServiceItem(Container, serviceType, child);
            if (IsSystem) //生成服务与当前服务(依赖服务)属性必须一致
            {
                item.MakeSystem();
            }
            PropertyChanged += item.Item_PropertyChanged; //当当前服务(依赖服务)发生属性变化时需要通知生成服务
            return item;
        }

        /// <summary>
        /// 将当前服务组件变为系统服务
        /// </summary>
        internal void MakeSystem()
        {
            IsSystem = true;
            _systemValue = _value;
        }


        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!AutoUpdate) //自动更新
            {
                return;
            }
            var parent = (ServiceItem)sender; //依赖服务
            var child = (parent.Value as IServiceProvider)?.GetService(parent.ServiceType); //生成新的服务
            if (child != null) //如果无法生成新的服务,则保留旧服务
            {
                parent.IsSystem = IsSystem; //生成服务与依赖服务属性必须一致
                parent.Value = child;
            }
        }

        /// <summary>
        /// 属性变化时触发
        /// </summary>
        /// <param name="propertyName"> </param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// 拷贝当前服务项的所有属性,到新对象
        /// </summary>
        /// <param name="item"> </param>
        /// <exception cref="ArgumentNullException"> <paramref name="item" /> is <see langword="null" />. </exception>
        /// <exception cref="FieldAccessException">
        /// 在 .NET for Windows Store 应用程序 或 可移植类库 中，请改为捕获基类异常
        /// <see cref="T:System.MemberAccessException" />。调用方没有访问此字段的权限。
        /// </exception>
        internal void CopyTo(ServiceItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            foreach (
                var field in
                typeof(ServiceItem).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (field.IsLiteral == false)
                {
                    field.SetValue(item, field.GetValue(this));
                }
            }
        }
    }
}