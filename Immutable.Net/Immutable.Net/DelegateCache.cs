using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Serialization;

namespace ImmutableNet
{
    public class DelegateCache<T> : IDelegateCache<T>
    {
        private class AccessorCache<TValue>
        {
            public readonly ConcurrentDictionary<MemberInfo, Func<T, TValue, T>> AccessorDelegates =
                new ConcurrentDictionary<MemberInfo, Func<T, TValue, T>>();

            private AccessorCache()
            {
            }

            public static AccessorCache<TValue> Instance { get; } = new AccessorCache<TValue>();
        }

        public Func<T, TValue, T> GetAccessorDelegate<TValue>(MemberInfo memberInfo)
        {
            Func<T, TValue, T> accessorDelegate;

            AccessorCache<TValue>.Instance.AccessorDelegates.TryGetValue(memberInfo, out accessorDelegate);

            return accessorDelegate;
        }

        public void CacheAccessorDelegate<TValue>(MemberInfo memberInfo, Func<T, TValue, T> accessorDelegate)
        {
            AccessorCache<TValue>.Instance.AccessorDelegates.AddOrUpdate(memberInfo, accessorDelegate, (key, item) => accessorDelegate);
        }

        public Func<T> CreationDelegate { get; set; }

        public Func<T, T> CloneDelegate { get; set; }
        public Func<T, SerializationInfo, T> SerializationDelegate { get; set; }
        public Func<T, SerializationInfo, T> DeserializationDelegate { get; set; }

        private DelegateCache()
        {
        }

        public static DelegateCache<T> Instance { get; } = new DelegateCache<T>();
    }
}
