using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition;
using System.Linq;

namespace blqw.IOC
{
    [TestClass]
    public class UnitTest2
    {
        public class MyClass
        {

            [Export("a")]
            [ExportMetadata("Priority", 100)]
            public static string a() { return "a"; }


            [Export("a")]
            [ExportMetadata("Priority", 1)]
            public static string a1 { get; set; } = "a1";


            [Export("b")]
            [ExportMetadata("Priority", 100)]
            public static string b = "b";


            [Export("b")]
            [ExportMetadata("Priority", 1)]
            public static string b1 { get; set; } = "b";

            [Export("c")]
            [ExportMetadata("Priority", 100)]
            public static string c { get; set; } = "c";

            [Export("c")]
            [ExportMetadata("Priority", 1)]
            [ExportMetadata("xyz", "?")]
            public static string c1 { get; set; } = "c1";
        }


        [TestMethod]
        public void 测试获取方法_属性_字段()
        {
            MEF.Initializer();
            Assert.AreEqual("a", MEF.PlugIns.GetExport<Func<string>>()?.Invoke());
            Assert.AreEqual("a1", MEF.PlugIns.GetExport<string>("a"));
            Assert.AreEqual("b", MEF.PlugIns.GetExport<string>("b"));
            Assert.AreEqual("c", MEF.PlugIns.GetExport<string>("c"));

            var arr = MEF.PlugIns.GetExports("b").ToArray();
            Assert.IsNotNull(arr);
            Assert.AreEqual(arr.Length, 2);
            Assert.AreEqual("b", arr[0]);
            Assert.AreEqual("b", arr[1]);

            var s = MEF.PlugIns.Where(p => p.Name == "c" && p.GetMetadata<string>("xyz") == "?").Select(p => p.GetValue<string>()).FirstOrDefault();
            Assert.AreEqual("c1", s);

        }
    }
}
