using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition;

namespace blqw.IOC
{
    [TestClass]
    public class UnitTest5
    {
        [Export("UnitTest5", typeof(ICloneable))]
        class MyClass : ICloneable
        {
            public MyClass()
            {

            }
            public object Clone()
            {
                return "UnitTest5";
            }
        }

        [Import("UnitTest5")]
        ICloneable s0;

        [TestMethod]
        public void 实例字段导入类型实现()
        {
            Assert.IsNull(s0);
            MEF.Import(this);
            Assert.AreEqual("UnitTest5", s0?.Clone());
        }


        [Import("UnitTest5")]
        static ICloneable s1;
        [TestMethod]
        public void 实例静态导入类型实现()
        {
            Assert.IsNull(s1);
            MEF.Import(typeof(UnitTest5));
            Assert.AreEqual("UnitTest5", s1?.Clone());
        }


    }
}
