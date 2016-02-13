using blqw.IOC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blqw.IOC
{
    public class Program
    {
        static void Main(string[] args)
        {
            var func = MEF.PlugIns.GetExports("x");
            foreach (var item in func)
            {
                Console.WriteLine(item);
            }
            //Console.WriteLine(func?.Invoke("test"));
        }
    }


    public class MyClass
    {

        [Export("x")]
        [ExportMetadata("Priority", 100)]
        public static string xxx(string name) { return "xxx:" + name; }
    }
}
