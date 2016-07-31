using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition;
using System.Collections.Generic;

namespace blqw.IOC
{
    [TestClass]
    public class UnitTest7
    {
        [InheritedExport]
        public interface IInterface
        {

        }

        public class MyClass : IInterface
        {

        }

        public class MyClass2 : IInterface
        {
            public MyClass2(object obj)
            {

            }
        }

        [ImportMany(typeof(IInterface))]
        public List<IInterface> X;

        [ImportMany(typeof(IInterface))]
        public static List<IInterface> Y;

        [TestMethod]
        public void TestMethod1()
        {
            MEF.Import(typeof(UnitTest7));
            Assert.AreEqual(1, Y?.Count);
            Assert.IsInstanceOfType(Y[0], typeof(MyClass));
            MEF.Import(this);
            Assert.AreEqual(1, X?.Count);
            Assert.IsInstanceOfType(X[0], typeof(MyClass));

        }
    }
}
