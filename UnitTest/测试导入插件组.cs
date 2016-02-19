using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.Composition;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace blqw.IOC
{
    [TestClass]
    public class UnitTest4
    {
        public class MyClass
        {
            [Export("测试导入插件组")]
            [ExportMetadata("Priority", 98)]
            public static string test2 = "2";

            [Export("测试导入插件组")]
            [ExportMetadata("Priority", 100)]
            public static string test = Guid.NewGuid().ToString();

            [Export("测试导入插件组")]
            [ExportMetadata("Priority", 99)]
            public static string test1 = "1";
        }

        
        [ImportMany("测试导入插件组")]
        private IEnumerable<string> s;

        [ImportMany("测试导入插件组")]
        private string[] s1;

        [ImportMany("测试导入插件组")]
        private List<string> s2;

        //[ImportMany("测试导入插件组")]
        //private IList<string> s3;

        [ImportMany("测试导入插件组")]
        private HashSet<string> s4;

        [TestMethod]
        public void 实例对象导入插件组()
        {
            Assert.IsNull(s);
            Assert.IsNull(s1);
            Assert.IsNull(s2);
            //Assert.AreEqual(null, s3);
            Assert.IsNull(s4);
            MEF.Import(this);

            Action<IEnumerable<string>> assert = p =>
            {

                Assert.AreEqual(3, p.Count());
                Assert.IsTrue(p.Contains("2"));
                Assert.IsTrue(p.Contains("1"));
                Assert.IsTrue(p.Contains(MyClass.test));
            };

            assert(s);
            assert(s1);
            assert(s2);
            //assert(s3);
            assert(s4);
        }


        [ImportMany("测试导入插件组")]
        private static IEnumerable<string> ss;

        [ImportMany("测试导入插件组")]
        private static string[] ss1;

        [ImportMany("测试导入插件组")]
        private static List<string> ss2;

        [ImportMany("测试导入插件组")]
        private static IList<string> ss3;

        [ImportMany("测试导入插件组")]
        private static HashSet<string> ss4;


        [TestMethod]
        public void 静态对象导入插件组()
        {
            Assert.IsNull(ss);
            Assert.IsNull(ss1);
            Assert.IsNull(ss2);
            Assert.IsNull(ss3);
            Assert.IsNull(ss4);
            MEF.Import(typeof(UnitTest4));

            Action<IEnumerable<string>> assert = p =>
            {
                Assert.IsNotNull(p);
                Assert.AreEqual(3, p.Count());
                Assert.IsTrue(p.Contains("2"));
                Assert.IsTrue(p.Contains("1"));
                Assert.IsTrue(p.Contains(MyClass.test));
            };

            assert(ss);
            assert(ss1);
            assert(ss2);
            assert(ss3);
            assert(ss4);
        }

    }
}
