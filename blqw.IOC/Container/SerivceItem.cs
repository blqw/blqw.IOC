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
    public class SerivceItem : IObjectHandle, INotifyPropertyChanged, IObjectReference, IServiceProvider
    {
        private object _systemValue;
        private object _value;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="container"> </param>
        /// <param name="serviceType"> </param>
        /// <param name="value"> </param>
        /// <exception cref="ArgumentNullException"> <paramref name="container" /> is <see langword="null" />. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="serviceType" /> is <see langword="null" />. </exception>
        public SerivceItem(IServiceContainer container, Type serviceType, object value)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
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
        /// 包装值
        /// </summary>
        /// <exception cref="TargetException"> <seealso cref="ServiceCreatorCallback" /> 中出现异常. </exception>
        public object Value
        {
            get
            {
                var call = _value as ServiceCreatorCallback;
                if (call != null)
                {
                    try
                    {
                        _value = call(Container, ServiceType);
                    }
                    catch (Exception ex)
                    {
                        throw new TargetException($"{nameof(ServiceCreatorCallback)}中出现异常", ex);
                    }
                }
                return _value;
            }
            set
            {
                //当服务被置空,如果是系统服务则还原
                if (value == null && _systemValue != null)
                {
                    _value = _systemValue;
                    IsSystem = true;
                    OnPropertyChanged();
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
        /// 将当前服务组件变为系统服务
        /// </summary>
        internal void MakeSystem()
        {
            IsSystem = true;
            _systemValue = _value;
        }

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
        /// <exception cref="TargetException"> <seealso cref="ServiceCreatorCallback" /> 中出现异常. </exception>
        public object GetService(Type serviceType)
        {
            if (serviceType == ServiceType)
            {
                return this;
            }
            var child = (Value as IServiceProvider)?.GetService(ServiceType); //生成新的服务

            if (child == null)
            {
                return null;
            }

            var item = new SerivceItem(Container, serviceType, child);
            if (IsSystem) //生成服务与当前服务(依赖服务)属性必须一致
            {
                item.MakeSystem();
            }
            PropertyChanged += item.Item_PropertyChanged; //当当前服务(依赖服务)发生属性变化时需要通知生成服务
            return item;
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!AutoUpdate) //自动更新
            {
                return;
            }
            var parent = (SerivceItem)sender;//依赖服务
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
        /// <param name="item"></param>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null" />.</exception>
        internal void CopyTo(SerivceItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            foreach (var field in typeof(SerivceItem).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (field.IsLiteral == false)
                {
                    field.SetValue(item,field.GetValue(this));
                }
            }
        }
    }
}