using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace ImmutableNet
{
    public interface IDelegateCache<T>
    {
        Func<T, TValue, T> GetAccessorDelegate<TValue>(MemberInfo memberInfo);
        void CacheAccessorDelegate<TValue>(MemberInfo memberInfo, Func<T, TValue, T> accessorDelegate);
        Func<T> CreationDelegate { get; set; }
        Func<T, T> CloneDelegate { get; set; }
        Func<T, SerializationInfo, T> SerializationDelegate { get; set; }
        Func<T, SerializationInfo, T> DeserializationDelegate { get; set; }
    }
}
