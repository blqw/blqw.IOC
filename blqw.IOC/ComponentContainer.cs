using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blqw.IOC
{
    public sealed class ComponentContainer<TKey, TValue>
    {
        private readonly Func<TValue, TKey> _getKey;
        private readonly Func<PlugIn, TValue> _select;
        private readonly ConcurrentDictionary<TKey, TValue> _items;

        /// <summary>
        /// </summary>
        /// <param name="getKey"></param>
        /// <exception cref="ArgumentNullException"><paramref name="getKey" /> is <see langword="null" />.</exception>
        public ComponentContainer(Func<TValue, TKey> getKey)
            : this(null, getKey)
        {

        }

        /// <summary>
        /// </summary>
        /// <param name="getKey"></param>
        /// <param name="select"></param>
        /// <exception cref="ArgumentNullException"><paramref name="getKey"/> is <see langword="null" />.</exception>
        public ComponentContainer(Func<PlugIn, TValue> select, Func<TValue, TKey> getKey)
        {
            if (getKey == null)
                throw new ArgumentNullException(nameof(getKey));
            if (select == null)
            {
                var id = AttributedModelServices.GetTypeIdentity(typeof(TValue));
                _select = p => p.TypeIdentity == id ? p.GetValue<TValue>() : default(TValue);
            }
            else
            {
                _select = select;
            }
            _getKey = getKey;
            _items = new ConcurrentDictionary<TKey, TValue>();
            Reload();
        }

        public void Reload()
        {
            _items.Clear();
            foreach (var plugIn in MEF.PlugIns)
            {
                var value = _select(plugIn);
                if (value != null)
                {
                    _items[_getKey(value)] = value;
                }
            }
        }


        public TValue this[TKey key]
        {
            get
            {
                if (key == null)
                    return default(TValue);
                TValue value;
                _items.TryGetValue(key, out value);
                return value;
            }
        }

        public int Count => _items.Count;
    }
}
