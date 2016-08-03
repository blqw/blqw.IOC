using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace blqw.IOC
{
    [TestClass]
    public class UnitTest8
    {
        [TestMethod]
        public void 测试json的默认行为()
        {
            var obj = new { id = 1, name = "blqw", sex = true };
            var str = ComponentServices.ToJsonString(obj);
            Assert.IsNotNull(str);
            var obj2 = ComponentServices.ToJsonObject(obj.GetType(), str);
            Assert.IsNotNull(obj2);
        }
    }
}
