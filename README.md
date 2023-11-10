# WakeOnLANServer #

## 介绍 ##

提供可指定向特定VLAN发送WOL封包的网络唤醒服务,使用该功能需要该网络接口为Trunk口.

## 配置WOL发送网口 ##

在appsettings.json文件的WakeOnLanInterface数组中添加指定接口名称,
如["eth0", "eth1"]或使用["*"]向所有网络接口发送WOL封包.
