using Microsoft.VisualStudio.TestTools.UnitTesting;
using blqw.IOC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace blqw.IOC.Tests
{
    [TestClass()]
    public class CallContextExTests
    {
        [TestMethod()]
        public void GetDataNamesTest()
        {
            CallContext.SetData("aaa", 1);
            CallContext.SetData("bbb", 1);
            var names = CallContextEx.GetDataNames()?.OrderBy(a => a).ToArray();
            Assert.IsNotNull(names);
            Assert.AreEqual(2, names.Length);
            Assert.AreEqual("aaa", names[0]);
            Assert.AreEqual("bbb", names[1]);
        }

        [TestMethod()]
        public void GetLogicalDataNamesTest()
        {
            CallContext.LogicalSetData("aaa", 1);
            CallContext.LogicalSetData("bbb", 1);
            var names = CallContextEx.GetLogicalDataNames()?.OrderBy(a => a).ToArray();
            Assert.IsNotNull(names);
            Assert.AreEqual(2, names.Length);
            Assert.AreEqual("aaa", names[0]);
            Assert.AreEqual("bbb", names[1]);
        }

        [TestMethod()]
        public void ClearLogicalDataTest()
        {
            CallContext.LogicalSetData("aaa", 1);
            CallContext.LogicalSetData("bbb", 1);
            CallContextEx.ClearLogicalData();
            var names = CallContextEx.GetLogicalDataNames()?.OrderBy(a => a).ToArray();
            Assert.IsNotNull(names);
            Assert.AreEqual(0, names.Length);
        }

        [TestMethod()]
        public void RestoreLogicalDataTest()
        {
            CallContext.LogicalSetData("aaa", 1);
            CallContext.LogicalSetData("bbb", 1);
            var arc = CallContextEx.ArchiveAndClearLogicalData();
            var names = CallContextEx.GetLogicalDataNames()?.OrderBy(a => a).ToArray();
            Assert.IsNotNull(names);
            Assert.AreEqual(0, names.Length);
            arc.Restore();

            names = CallContextEx.GetLogicalDataNames()?.OrderBy(a => a).ToArray();
            Assert.IsNotNull(names);
            Assert.AreEqual(2, names.Length);
            Assert.AreEqual("aaa", names[0]);
            Assert.AreEqual("bbb", names[1]);
        }
    }
}