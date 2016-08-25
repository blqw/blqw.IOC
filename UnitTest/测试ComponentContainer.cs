using Microsoft.VisualStudio.TestTools.UnitTesting;
using blqw.IOC;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace blqw.IOC.Tests
{



    [TestClass()]
    public class Test_ComponentContainer
    {
        [InheritedExport(typeof(IInterface))]
        public interface IInterface
        {
            string Name { get; }

        }

        public class MyClass : IInterface
        {
            public string Name { get; } = "a";
        }

        public class MyClass2 : IInterface
        {
            public string Name { get; } = "b";
        }

        [ImportMany(typeof(IInterface))]
        public List<IInterface> X;

        [TestMethod()]
        public void 测试ComponentContainer导入插件()
        {
            var container = new ComponentContainer<string, IInterface>(v => v.Name);
            Assert.AreEqual(2, container.Count);
            Assert.IsInstanceOfType(container["a"], typeof(MyClass));
            Assert.IsInstanceOfType(container["b"], typeof(MyClass2));
        }
    }
}