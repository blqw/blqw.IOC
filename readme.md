﻿# 基于微软MEF组件的插件管理组件
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