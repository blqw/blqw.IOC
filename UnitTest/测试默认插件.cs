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
            Assert.IsNotNull(ComponentServices.WrapMamber);
            Assert.IsNotNull(ComponentServices.GetDefaultValue);
            Assert.IsNotNull(ComponentServices.GetDynamic);
            Assert.IsNotNull(ComponentServices.GetGeter);
            Assert.IsNotNull(ComponentServices.GetSeter);
            Assert.IsNotNull(ComponentServices.ToJsonObject);
            Assert.IsNotNull(ComponentServices.ToJsonString);
        }
    }
}
