# ClassIslandHide

本插件可以通过伪装隐藏 ClassIsland ，以尝试规避部分弹窗拦截工具对 ClassIsland 的拦截。

## 功能

- 设置随机窗口标题
- 设置随机进程名
- 使 ClassIsland 窗口对屏幕监控程序不可见

上述功能在插件安装后即生效，无需手动操作。

## 使用方法

1. 在插件市场下载并安装本插件
2. 重启应用。恭喜，插件已生效！

## 注意事项

1. 由于本插件会设置随机进程名，一定要在本插件**禁用**的情况下设置开机自启动和注册 Url 协议，否则会降低隐藏效果或导致应用无法启动。

## 许可

本项目基于 GNU General Public License v3.0 许可。

# ResourceHacker工具嵌入说明

为了使进程描述修改功能正常工作，你需要将ResourceHacker.exe工具嵌入到项目中。

## 步骤

1. 下载ResourceHacker工具(http://www.angusj.com/resourcehacker/)
2. 在项目中创建一个Tools文件夹
3. 将ResourceHacker.exe复制到Tools文件夹中
4. 在项目文件(.csproj)中添加以下配置，将ResourceHacker.exe嵌入为资源:

```xml
<ItemGroup>
  <EmbeddedResource Include="Tools\ResourceHacker.exe" />
</ItemGroup>
```

5. 确保嵌入资源的命名空间正确，应为"ClassIslandHide.Tools.ResourceHacker.exe"

你也可以改用其他资源编辑工具，如rcedit或者自定义的PE文件编辑库，只需更新代码中的相关部分即可。