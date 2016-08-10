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



        [TestMethod]
        public void 测试GetSet默认行为()
        {
            var a = new {id = 1, name = "blqw"};
            var id = ComponentServices.GetGeter(a.GetType().GetProperty("id"))(a);
            Assert.AreEqual(a.id, id);
            ComponentServices.GetSeter(a.GetType().GetProperty("id"))(a, 2);
            Assert.AreEqual(2, a.id);
            var name = ComponentServices.GetGeter(a.GetType().GetProperty("name"))(a);
            Assert.AreEqual(a.name, name);
            ComponentServices.GetSeter(a.GetType().GetProperty("name"))(a, "zzj");
            Assert.AreEqual("zzj", a.name);
        }
    }
}
