# 基于微软MEF组件的插件管理组件

## MEF
基于微软MEF,实现代码间依赖**协议**而不是**具体实现**或**指定接口**

## MEF功能导出
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

## MEF功能导入
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
```csharp
static void Main(string[] args)
{
    Console.WriteLine(Components.Encryption(""));      //空字符串
    Console.WriteLine(Components.Encryption("aaaa"));  //YWFhYQ==
}
```

## MEF功能替换 
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
        return Convert3.ToMD5(str);
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
## 插件管理
无需定义,直接获取并使用
```csharp
static void Main(string[] args)
{
    Func<string,string> encode = MEF.PlugIns.GetExport<Func<string, string>>("加密");
    Console.WriteLine(encode("aaaa"));  //74b87337454200d4d33f80c4663dc5e5
}
```
## MEF类型导出

---
## 更新日志
#### [1.3.8.1] 2016.12.06
* 紧急修复bug 

#### [1.3.8] 2016.12.05
* 提供`MEF.ReInitiation`方法，手动重载插件
* 当插件中出现无效插件时，自动重新载入所有插件

#### [1.3.7] 2016.11.30
* 开放`ServiceContainer`类中多个方法的重写

#### [1.3.6] 2016.10.12
* 优化日志拓展方法,当`message`参数为空时,不再抛出异常

#### [1.3.5] 2016.10.07
* 优化插件对优先级的处理

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