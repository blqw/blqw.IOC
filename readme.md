# 基于微软MEF组件的插件管理组件

## 插件式解耦
组件与组件之间依赖插件协议而不是具体实现或具体接口

* #### 定义导出功能
```csharp
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
```

* #### 功能导入
```csharp
static class Components
{
    static Components()
    {
        MEF.Import(typeof(Components));
    }

    [Import("加密")]
    public static Func<string, string> Encryption;

}
```

* #### 使用插件
```csharp
static void Main(string[] args)
{
    Console.WriteLine(Components.Encryption(""));      //空字符串
    Console.WriteLine(Components.Encryption("aaaa"));  //YWFhYQ==
}
```

* #### 替换原有功能
```csharp
public class MyClass2
{
    [Export("加密")]
    [ExportMetadata("Priority", 1)] //增加优先级
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
```
```csharp
static void Main(string[] args)
{
    Console.WriteLine(Components.Encryption(""));      //00000000000000000000000000000000
    Console.WriteLine(Components.Encryption("aaaa"));  //74b87337454200d4d33f80c4663dc5e5
}
```

--------

## 更新日志
#### [1.3.4] 2016.10.07
* 修复bug

#### [1.3.3] 2016.10.07
* 新增一组方法用于获取匹配的插件 `PlugInContainer.GetPlugIns`
* 新增`SimpleTraceListener`侦听器基类
* 新增`ServiceContainer`的日志记录

#### [1.3.2] 2016.10.05
* 优化`ServiceContainer`可以根据优先级过滤插件

#### [1.3.0] 2016.09.23
* 修复部分bug
* 优化日志

#### 2016.08.10
* 优化默认插件的行为

#### 2016.08.03
* `ComponentServices.ToJsonObject`和`ComponentServices.ToJsonString`2个方法在没有插件时,默认会寻找`Newtonsoft.Json`

#### 2016.07.31
* 优化插件加载,当1个插件加载失败时,不会影响其他插件

#### 2016.07.30
* 修复默认插件为空的bug
* 修复一个很诡异的bug

#### 2016.07.27
* 重构部分功能

#### 2016.06.27
* 优化加载逻辑,防止相同的组件多次加载  

#### 2016.04.15
* 优化多线程并发初始化时加载逻辑  

#### 2016.04.09
* 优化Type加载失败时的处理逻辑  

#### 2016.04.07
* 优化代码  