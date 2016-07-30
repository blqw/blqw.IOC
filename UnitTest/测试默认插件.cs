using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace blqw.IOC
{
    [TestClass]
    public class UnitTest6
    {
        [TestMethod]
        public void 测试默认插件()
        {
            Assert.IsNotNull(ComponentServices.Converter);
        }
    }
}
