using System;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace ImmutableNet.Tests
{
    public class DelegateCacheTests
    {
        public class TestClass
        {
            public int Prop;
        }

        [Fact]
        public void Test_That_DelegateCache_Instance_Returns_Same_Instance()
        {
            var delegateCache = DelegateCache<TestClass>.Instance;

            var delegateCache2 = DelegateCache<TestClass>.Instance;

            Assert.Equal(delegateCache, delegateCache2); //checks reference equality
        }

        [Fact]
        public void Test_That_DelegateCache_Accessor_Instance_Returns_Same_Instance()
        {
            throw new NotImplementedException();
        }
    }
}
