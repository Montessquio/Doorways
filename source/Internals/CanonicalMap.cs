using Doorways.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Doorways.Internals
{
    /// <summary>
    /// A wrapper around a Dictionary that tries to canonicalize
    /// any <see cref="INamespacedIDEntity"/>s it is asked to store.
    /// </summary>
    internal class CanonicalMap<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> inner;
        private FnCanonicalize canonicalizer;
        public string Prefix { get; private set; }

        public ICollection<TKey> Keys => ((IDictionary<TKey, TValue>)inner).Keys;

        public ICollection<TValue> Values => ((IDictionary<TKey, TValue>)inner).Values;

        public int Count => ((ICollection<KeyValuePair<TKey, TValue>>)inner).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)inner).IsReadOnly;

        public TValue this[TKey key] 
        { 
            get => ((IDictionary<TKey, TValue>)inner)[key]; 
            set
            {
                if (typeof(INamespacedIDEntity).IsAssignableFrom(typeof(TValue)))
                {
                    ((INamespacedIDEntity)value).CanonicalizeIds(canonicalizer, Prefix);
                }
                ((IDictionary<TKey, TValue>)inner)[key] = value;
            }
        }

        public CanonicalMap(FnCanonicalize canonicalizer, string prefix)
        {
            inner = new Dictionary<TKey, TValue>();
            Prefix = prefix;
            this.canonicalizer = canonicalizer;
        }

        public bool ContainsKey(TKey key)
        {
            return ((IDictionary<TKey, TValue>)inner).ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public bool Remove(TKey key)
        {
            return ((IDictionary<TKey, TValue>)inner).Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return ((IDictionary<TKey, TValue>)inner).TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if(typeof(INamespacedIDEntity).IsAssignableFrom(typeof(TValue))) 
            {
                ((INamespacedIDEntity)item.Value).CanonicalizeIds(canonicalizer, Prefix);
            }
            ((ICollection<KeyValuePair<TKey, TValue>>)inner).Add(item);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)inner).Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)inner).Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)inner).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)inner).Remove(item);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<TKey, TValue>>)inner).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)inner).GetEnumerator();
        }
    }
}
