using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ImmutableNet
{
    /// <summary>
    /// Encloses a type in an immutable construct.
    /// </summary>
    /// <typeparam name="T">The type to enclose.</typeparam>
    [Serializable]
    [XmlType]
    public class Immutable<T> : ISerializable where T : class
    {
        /// <summary>
        /// An instance of the enclosed immutable data type.
        /// </summary>
        [XmlElement(Order=1)]
        private T self;

        private readonly IDelegateCache<T> _delegateCache;

        /// <summary>
        /// Creates an instance of an Immutable.
        /// </summary>
        public Immutable() : this(DelegateCache<T>.Instance)
        {
        }

        public Immutable(IDelegateCache<T> delegateCache)
        {
            _delegateCache = delegateCache;

            if (_delegateCache.CreationDelegate == null)
            {
                _delegateCache.CreationDelegate = DelegateBuilder.BuildCreationDelegate<T>();
            }

            self = _delegateCache.CreationDelegate.Invoke();
        }

        /// <summary>
        /// A private constructor that allows a new Immutable to be built
        /// from a reference to the enclosed type.
        /// </summary>
        /// <param name="self">The instance of the enclosed type to use.</param>
        private Immutable(T self) : this(self, DelegateCache<T>.Instance)
        {
        }

        /// <summary>
        /// A private constructor that allows a new Immutable to be built
        /// from a reference to the enclosed type.
        /// </summary>
        /// <param name="self">The instance of the enclosed type to use.</param>
        /// <param name="delegateCache"></param>
        private Immutable(T self, IDelegateCache<T> delegateCache)
        {
            _delegateCache = delegateCache;

            this.self = self;
        }

        /// <summary>
        /// A private constructor used by ISerializable to deserialize the Immutable.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The serialization streaming context.</param>
        private Immutable(SerializationInfo info, StreamingContext context)
        {
            _delegateCache = DelegateCache<T>.Instance;

            if(_delegateCache.CreationDelegate == null)
            {
                _delegateCache.CreationDelegate = DelegateBuilder.BuildCreationDelegate<T>();
            }

            self = _delegateCache.CreationDelegate.Invoke();

            if(_delegateCache.DeserializationDelegate == null)
            {
                _delegateCache.DeserializationDelegate = DelegateBuilder.BuildDeserializationDelegate<T>();
            }

            self = _delegateCache.DeserializationDelegate(self, info);
        }

        /// <summary>
        /// Creates a new instance of an Immutable using a stuffed enclosed type.
        /// </summary>
        /// <param name="self">The instance to create the Immutable from.</param>
        /// <returns>A new Immutable with a cloned enclosed instance.</returns>
        public static Immutable<T> Create(T self)
        {
            return Create(self, DelegateCache<T>.Instance);
        }

        public static Immutable<T> Create(T self, IDelegateCache<T> delegateCache)
        {
            if (delegateCache.CloneDelegate == null)
            {
                delegateCache.CloneDelegate = DelegateBuilder.BuildCloner<T>();
            }

            return new Immutable<T>(delegateCache.CloneDelegate(self), delegateCache);
        }

        /// <summary>
        /// Modifies and returns a copy of the modified Immutable.
        /// </summary>
        /// <typeparam name="TValue">The type of the value to set.</typeparam>
        /// <param name="assignment">A member to assign the value to.</param>
        /// <param name="value">The value to assign.</param>
        /// <returns>A new modified Immutable instance.</returns>
        public Immutable<T> Modify<TValue>(Expression<Func<T, TValue>> assignment, TValue value)
        {
            MemberExpression assignTo = assignment.Body as MemberExpression;
            if (assignTo == null)
            {
                var body = assignment.Body as UnaryExpression;
                if (body != null)
                {
                    assignTo = body.Operand as MemberExpression;
                }
            }

            if (assignTo == null)
            {
                throw new ArgumentException("Can only assign to a class member.");
            }

            var accessor = _delegateCache.GetAccessorDelegate<TValue>(assignTo.Member);

            if (accessor == null)
            {
                accessor = DelegateBuilder.BuildAccessorDelegate(assignment);
                _delegateCache.CacheAccessorDelegate(assignTo.Member, accessor);
            }

            return new Immutable<T>(accessor(Clone(), value), _delegateCache);
        }

        /// <summary>
        /// Clones an enclosed immutable type.
        /// </summary>
        /// <returns>A copy of the enclosed type.</returns>
        private T Clone()
        {
            if(_delegateCache.CloneDelegate == null)
            {
                _delegateCache.CloneDelegate = DelegateBuilder.BuildCloner<T>();
            }

            return _delegateCache.CloneDelegate(self);
        }

        /// <summary>
        /// Gets a value from the Immutable.
        /// </summary>
        /// <typeparam name="TReturn">The type of the value to return.</typeparam>
        /// <param name="accessor">A lambda containing the member to return.</param>
        /// <returns>A value from the provided member.</returns>
        public TReturn Get<TReturn>(Func<T, TReturn> accessor)
        {
            return accessor(self);
        }

        /// <summary>
        /// Creates a builder instance of the Immutable.
        /// </summary>
        /// <returns>A builder instance.</returns>
        public ImmutableBuilder<T> ToBuilder()
        {
            return ImmutableBuilder<T>.Create(Clone(), _delegateCache);
        }

        /// <summary>
        /// Provides serialization data for ISerializable.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The serialization streaming context.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if(_delegateCache.SerializationDelegate == null)
            {
                _delegateCache.SerializationDelegate = DelegateBuilder.BuildSerializationDelegate<T>();
            }

            _delegateCache.SerializationDelegate(self, info);
        }
    }
}
