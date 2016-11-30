using blqw.IOC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace blqw.IOC
{
    [Export("Component")]
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Components.Encryption(""));
            Console.WriteLine(Components.Encryption("aaaa"));


            Func<string, string> encode = MEF.PlugIns.GetExport<Func<string, string>>("加密");
            Console.WriteLine(encode("aaaa"));

        }
    }


    [Export("加密器")]
    class MD5 : ICustomFormatter
    {
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            throw new NotImplementedException();
        }
    }


    public class MyClass2
    {
        [Export("加密")]
        [ExportMetadata("Priority", 1)]
        public static string Encryption(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return Guid.Empty.ToString("n");
            }
            return ToMD5_Fast(str).ToString("n");
        }

        /// <summary> 使用MD5加密
        /// </summary>
        /// <param name="input">加密字符串</param>
        /// <remarks>周子鉴 2015.08.26</remarks>
        public static Guid ToMD5_Fast(string input)
        {
            using (var md5Provider = new MD5CryptoServiceProvider())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = md5Provider.ComputeHash(bytes);
                Swap(hash, 0, 3);   //交换0,3的值
                Swap(hash, 1, 2);   //交换1,2的值
                Swap(hash, 4, 5);   //交换4,5的值
                Swap(hash, 6, 7);   //交换6,7的值
                return new Guid(hash);
            }
        }

        private static void Swap(byte[] arr, int a, int b)
        {
            var temp = arr[a];
            arr[a] = arr[b];
            arr[b] = temp;
        }
    }

    static class Components
    {
        static Components()
        {
            MEF.Import(typeof(Components));
        }

        [Import("加密")]
        public static Func<string, string> Encryption;

    }


    public class MyClass
    {
        [Export("加密")]
        public static string Encryption(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }
    }
}
