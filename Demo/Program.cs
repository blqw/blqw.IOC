using blqw.IOC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blqw.IOC
{
    [Export("Component")]
    public class Program
    {
        [Import("test")]
        [ExportMetadata("Priority", 99)]
        private string s;

        [Import("a")]
        private Func<string> a;
        static void Main(string[] args)
        {
            var program = new Program();
            MEF.Import(program);
            Console.WriteLine(MEF.Container);
            Console.WriteLine(Impl.MEFPart.Container);
            Console.WriteLine(program.s);
            Console.WriteLine(program.s == MyClass.test);



            // Console.WriteLine(s == MyClass.test);
        }
    }


    public class MyClass
    {
        [Export("test")]
        [ExportMetadata("Priority", 99)]
        public static string test2 = "2";

        [Export("test")]
        [ExportMetadata("Priority", 100)]
        public static string test = Guid.NewGuid().ToString();

        [Export("test")]
        [ExportMetadata("Priority", 99)]
        public static string test1 = "1";

        [Export("a")]
        public static string a()
        {
            return "s";
        }
    }
}
