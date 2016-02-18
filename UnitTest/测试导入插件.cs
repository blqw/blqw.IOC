using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition;

namespace blqw.IOC
{
    [TestClass]
    public class UnitTest3
    {

        public class MyClass
        {
            [Export("test")]
            [ExportMetadata("Priority", 98)]
            public static string test2 = "2";

            [Export("test")]
            [ExportMetadata("Priority", 100)]
            public static string test = Guid.NewGuid().ToString();

            [Export("test")]
            [ExportMetadata("Priority", 99)]
            public static string test1 = "1";
        }



        [Import("test")]
        private string s;


        [TestMethod]
        public void 实例对象导入插件()
        {
            Assert.AreEqual(null, s);
            MEF.Import(this);
            Assert.AreEqual(MyClass.test, s);
        }
        
        [Import("test")]
        private static string ss;

        [TestMethod]
        public void 静态对象导入插件()
        {
            Assert.AreEqual(null, ss);
            MEF.Import(typeof(UnitTest3));
            Assert.AreEqual(MyClass.test, ss);
        }
        

    }
}
