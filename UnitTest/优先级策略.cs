using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition;

namespace blqw.IOC
{
    [TestClass]
    public class UnitTest1
    {
        public class MyClass
        {

            [Export("x")]
            [ExportMetadata("Priority", 1)]
            public static string xxx() { return "1"; }


            [Export("x")]
            [ExportMetadata("Priority", 2)]
            public static string xxx2() { return "2"; }
        }

        [TestMethod]
        public void 测试优先级策略()
        {
            MEF.Initializer();
            var s = MEF.PlugIns["x"].GetValue<Func<string>>()?.Invoke();
            Assert.AreEqual("2", s);
        }
    }
}
