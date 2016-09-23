using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;
using System.Linq;

namespace blqw.IOC
{
    /// <summary>
    /// 服务容器基类
    /// </summary>
    public abstract class ServiceContainer : IServiceContainer
    {
        /// <summary>
        /// 用于保存所有组件
        /// </summary>
        private readonly ConcurrentDictionary<Type, ServiceItem> _items;

        /// <summary>
        /// 初始化服务容器
        /// </summary>
        /// <param name="serviceName"> 服务约定名称 </param>
        /// <param name="serviceType"> 服务约定基类或接口类型 </param>
        /// <param name="typeComparer"> 比较2个类型服务的优先级 </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="serviceName" /> and <paramref name="serviceType" /> is all
        /// <see langword="null" />.
        /// </exception>
        /// <exception cref="OverflowException"> 匹配插件数量超过字典的最大容量 (<see cref="F:System.Int32.MaxValue" />)。 </exception>
        protected ServiceContainer(string serviceName, Type serviceType, IComparer<Type> typeComparer)
        {
            TypeComparer = typeComparer;
            if (string.IsNullOrEmpty(serviceName) && (serviceType == null))
            {
                throw new ArgumentNullException($"{nameof(serviceName)} and {nameof(serviceType)}");
            }
            var query = MEF.PlugIns.AsQueryable();
            if (string.IsNullOrEmpty(serviceName) == false)
            {
                query = query.Where(p => p.Name == serviceName);
            }
            if (serviceType != null)
            {
                var id = AttributedModelServices.GetTypeIdentity(serviceType);
                query = query.Where(p => p.TypeIdentity == id);
            }
            _items = new ConcurrentDictionary<Type, ServiceItem>();
            foreach (var p in query)
            {
                var value = p.GetValue(serviceType);
                if (value == null)
                {
                    continue;
                }
                var type = GetServiceType(p, value);
                if (type == null)
                {
                    continue;
                }
                var item = new ServiceItem(this, type, value);
                item.MakeSystem(); //默认为系统插件
                _items.TryAdd(type, item);
            }
        }

        /// <summary>
        /// 获取插件的服务类型 <see cref="Type"/>, 默认 <code>plugIn.GetMetadata&lt;Type&gt;("ServiceType")</code>
        /// </summary>
        /// <param name="plugIn"> 插件 </param>
        /// <param name="value"> 插件值 </param>
        /// <returns></returns>
        protected virtual Type GetServiceType(PlugIn plugIn, object value) => plugIn.GetMetadata<Type>("ServiceType");

        /// <summary>
        /// 用于比较服务之间的优先级
        /// </summary>
        private IComparer<Type> TypeComparer { get; }

        /// <summary>
        /// 获取指定类型的服务对象。
        /// </summary>
        /// <returns>
        /// <paramref name="serviceType" /> 类型的服务对象。
        /// </returns>
        /// <param name="serviceType"> 一个对象，它指定要获取的服务对象的类型。 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="serviceType" /> is <see langword="null" />. </exception>
        /// <exception cref="OverflowException"> 字典中已包含元素的最大数目 (<see cref="F:System.Int32.MaxValue" />)。 </exception>
        public object GetService(Type serviceType) => GetServiceItem(serviceType)?.Value;

        /// <summary>
        /// 获取指定类型的服务对象的包装对象。
        /// </summary>
        /// <paramref name="serviceType" /> 类型的服务对象。
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"> <paramref name="serviceType" /> is <see langword="null" />. </exception>
        /// <exception cref="OverflowException"> 字典中已包含元素的最大数目 (<see cref="F:System.Int32.MaxValue" />)。 </exception>
        public ServiceItem GetServiceItem(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            var item = _items.GetOrAdd(serviceType, CreateServiceItem); //获取服务项, 不会为空
            if ((item.Value != null) || (item.AutoUpdate == false)) //如果值不为空,或者不需要自动更新,则直接返回 item
            {
                return item;
            }
            //尝试更新服务项
            lock (item)
            {
                if (item.AutoUpdate)
                {
                    item.AutoUpdate = false; //防止死循环
                    var newItem = CreateServiceItem(serviceType);
                    newItem.CopyTo(item);
                }
            }
            return item;
        }

        /// <summary>
        /// 将指定的服务添加到服务容器中。
        /// </summary>
        /// <param name="serviceType"> 要添加的服务类型。 </param>
        /// <param name="serviceInstance"> 要添加的服务类型的实例。此对象必须实现 <paramref name="serviceType" /> 参数所指示的类型或从其继承。 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="serviceType" /> is <see langword="null" />. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="serviceInstance" /> is <see langword="null" />. </exception>
        /// <exception cref="OverflowException"> 字典中已包含元素的最大数目 (<see cref="int.MaxValue" />)。 </exception>
        public void AddService(Type serviceType, object serviceInstance)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            if (serviceInstance == null)
            {
                throw new ArgumentNullException(nameof(serviceInstance));
            }
            _items.AddOrUpdate(serviceType
                , k => new ServiceItem(this, serviceType, serviceInstance)
                , (k, v) =>
                {
                    v.AutoUpdate = false;
                    v.Value = serviceInstance;
                    return v;
                });
        }

        /// <summary>
        /// 将指定服务添加到服务容器中，并可选择将该服务提升到任何父服务容器。
        /// </summary>
        /// <param name="serviceType"> 要添加的服务类型。 </param>
        /// <param name="serviceInstance"> 要添加的服务类型的实例。此对象必须实现 <paramref name="serviceType" /> 参数所指示的类型或从其继承。 </param>
        /// <param name="promote"> true，则将此请求提升到任何父服务容器；否则为 false。 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="serviceType" /> is <see langword="null" />. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="serviceInstance" /> is <see langword="null" />. </exception>
        /// <exception cref="OverflowException"> 字典中已包含元素的最大数目 (<see cref="int.MaxValue" />)。 </exception>
        public void AddService(Type serviceType, object serviceInstance, bool promote)
            => AddService(serviceType, serviceInstance);

        /// <summary>
        /// 将指定的服务添加到服务容器中。
        /// </summary>
        /// <param name="serviceType"> 要添加的服务类型。 </param>
        /// <param name="callback"> 用于创建服务的回调对象。这允许将服务声明为可用，但将对象的创建延迟到请求该服务之后。 </param>
        /// <exception cref="OverflowException"> 字典中已包含元素的最大数目 (<see cref="int.MaxValue" />)。 </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="serviceType" /> is <see langword="null" />. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="callback" /> is <see langword="null" />. </exception>
        public void AddService(Type serviceType, ServiceCreatorCallback callback)
            => AddService(serviceType, (object)callback);

        /// <summary>
        /// 将指定服务添加到服务容器中，并可选择将该服务提升到父服务容器。
        /// </summary>
        /// <param name="serviceType"> 要添加的服务类型。 </param>
        /// <param name="callback"> 用于创建服务的回调对象。这允许将服务声明为可用，但将对象的创建延迟到请求该服务之后。 </param>
        /// <param name="promote"> true，则将此请求提升到任何父服务容器；否则为 false。 </param>
        /// <exception cref="OverflowException"> 字典中已包含元素的最大数目 (<see cref="int.MaxValue" />)。 </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="serviceType" /> is <see langword="null" />. </exception>
        /// <exception cref="ArgumentNullException"> <paramref name="callback" /> is <see langword="null" />. </exception>
        public void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
            => AddService(serviceType, (object)callback);

        /// <summary>
        /// 从服务容器中移除指定的服务类型。
        /// </summary>
        /// <param name="serviceType"> 要移除的服务类型。 </param>
        /// <exception cref="ArgumentNullException"> <paramref name="serviceType" /> is <see langword="null" />. </exception>
        /// <exception cref="NotSupportedException"> 无法删除系统服务组件 </exception>
        public void RemoveService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            ServiceItem item;
            if (_items.TryGetValue(serviceType, out item) == false)
            {
                return;
            }
            if (item.IsSystem)
            {
                throw new NotSupportedException("无法删除系统服务组件");
            }
            item.AutoUpdate = true; //服务被删除后需要设置为自动更新,不然该服务会无法正常运行
            item.Value = null;
        }

        /// <summary>
        /// 从服务容器中移除指定的服务类型，并可选择将该服务提升到父服务容器。
        /// </summary>
        /// <param name="serviceType"> 要移除的服务类型。 </param>
        /// <param name="promote"> true，则将此请求提升到任何父服务容器；否则为 false。 </param>
        /// <exception cref="ArgumentNullException"> serviceType is <see langword="null" />. </exception>
        /// <exception cref="NotSupportedException"> 无法删除系统服务组件 </exception>
        public void RemoveService(Type serviceType, bool promote)
            => RemoveService(serviceType);

        /// <summary>
        /// 根据 <paramref name="serviceType" /> 创建一个 <seealso cref="ServiceItem" />
        /// </summary>
        /// <param name="serviceType"> 服务组件类型 </param>
        /// <returns> </returns>
        private ServiceItem CreateServiceItem(Type serviceType)
        {
            var ee = Match(serviceType);
            while (ee.MoveNext())
            {
                var item = ee.Current.GetServiceItem(serviceType);
                if (item == null)
                {
                    item = ee.Current;
                }
                else if (ReferenceEquals(item, ee.Current))
                {
                    return item;
                }
                var container = item.Value as IServiceContainer;
                if (container == null)
                {
                    return item;
                }
                while (ee.MoveNext())
                {
                    container.AddService(item.ServiceType, item, false);
                }
                return item;
            }
            return GetServiceItem(typeof(object));
        }

        /// <summary>
        /// 获取所有匹配类型的服务组件
        /// </summary>
        /// <param name="serviceType"> </param>
        /// <returns> </returns>
        private IEnumerator<ServiceItem> Match(Type serviceType)
        {
            ServiceItem item;

            //精确匹配当前类 或 泛型定义 (优先级最高)
            if (_items.TryGetValue(serviceType, out item))
            {
                yield return item;
            }
            else
            {
                item = MatchGeneric(serviceType);
                if (item != null)
                {
                    yield return item;
                }
            }

            //匹配父类和接口
            var baseTypes = GetBaseType(serviceType).Union(serviceType.GetInterfaces());
            if (TypeComparer != null)
            {
                baseTypes = baseTypes.OrderByDescending(it => it, TypeComparer);
            }

            foreach (var interfaceType in baseTypes)
            {
                if (_items.TryGetValue(interfaceType, out item))
                {
                    yield return item;
                }
                else
                {
                    item = MatchGeneric(interfaceType);
                    if (item != null)
                    {
                        yield return item;
                    }
                }
            }

            //匹配object定义
            _items.TryGetValue(typeof(object), out item);
            yield return item;
        }

        /// <summary>
        /// 枚举所有父类
        /// </summary>
        /// <param name="type"> </param>
        /// <returns> </returns>
        private static IEnumerable<Type> GetBaseType(Type type)
        {
            var baseType = type.BaseType ?? typeof(object);
            while (baseType != typeof(object))
            {
                yield return baseType;
                baseType = baseType.BaseType ?? typeof(object);
            }
        }

        /// <summary>
        /// 获取与 <paramref name="genericType" /> 的泛型定义类型匹配的 <see cref="ServiceItem" />,如果
        /// <paramref name="genericType" /> 不是泛型,返回 null
        /// </summary>
        /// <param name="genericType"> 用于匹配的 <see cref="Type" /> </param>
        /// <returns> </returns>
        private ServiceItem MatchGeneric(Type genericType)
        {
            if (genericType.IsGenericType && (genericType.IsGenericTypeDefinition == false))
            {
                ServiceItem item;
                if (_items.TryGetValue(genericType.GetGenericTypeDefinition(), out item))
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// 服务组件个数
        /// </summary>
        public int Count => _items.Count;
    }
}