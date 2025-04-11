using System.Diagnostics;
using System.IO;
using ClassIsland.Core;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Controls.CommonDialog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using ClassIsland.Shared.Helpers;
using System.Runtime.InteropServices;
using System.Text;
using System.Reflection;
using PeNet;
using PeNet.Header.Resource;

namespace ClassIslandHide;

[PluginEntrance]
public class Plugin : PluginBase
{
    public Settings Settings { get; set; } = new();

    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        Settings = ConfigureFileHelper.LoadConfig<Settings>(Path.Combine(PluginConfigFolder, "Settings.json"));
        Settings.PropertyChanged += (sender, args) =>
            ConfigureFileHelper.SaveConfig(Path.Combine(PluginConfigFolder, "Settings.json"), Settings);
        var path = Environment.ProcessPath?.Replace(".dll", ".exe");
        //Debugger.Break();
        if (path != null && path != Settings.LastGhostExePath)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var newPath = Path.GetFullPath(Path.Combine("./", GetRandomString(chars) + ".exe"));

            if (File.Exists(Settings.LastGhostExePath))
            {
                File.Delete(Settings.LastGhostExePath);
            }

            Settings.RawExePath = Path.GetFullPath(path);
            File.Copy(Path.GetFullPath(path), newPath, true);
            Settings.LastGhostExePath = newPath;

            // 修改进程描述
            ModifyProcessDescription(newPath);

            var processStartInfo = new ProcessStartInfo()
            {
                FileName = newPath,
                WorkingDirectory = Environment.CurrentDirectory,
                ArgumentList = { "-m" },
                // 新增：隐藏控制台窗口
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                // 新增：使用不同的环境变量
                UseShellExecute = false
            };

            // 添加随机环境变量以迷惑分析工具
            processStartInfo.EnvironmentVariables["RANDOM_ID"] = GetRandomString("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");

            foreach (var i in Environment.GetCommandLineArgs().Skip(1).Where(x => !processStartInfo.ArgumentList.Contains(x)))
            {
                processStartInfo.ArgumentList.Add(i);
            }
            Process.Start(processStartInfo);

            Environment.Exit(0);
            return;
        }
        if (Path.GetFullPath(path!) != Settings.RawExePath)
        {
            File.Delete(Settings.RawExePath);
            File.Copy(Path.GetFullPath(path!), Settings.RawExePath, true);
        }

        EventManager.RegisterClassHandler(
            typeof(Window),                 // 目标类型
            FrameworkElement.LoadedEvent,             // 路由事件
            new RoutedEventHandler(OnWindowLoaded) // 事件处理程序
        );
        //CommonDialog.ShowHint("123");
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Window window)
        {
            return;
        }


        window.Title = GetRandomString("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+=-[]\\';\":/.,?><");

        var result = PInvoke.SetWindowDisplayAffinity((HWND)new WindowInteropHelper(window).Handle, WINDOW_DISPLAY_AFFINITY.WDA_EXCLUDEFROMCAPTURE);
    }

    public static string GetRandomString(string chars)
    {
        var randomString = new string(
            Enumerable.Range(0, GetInt(RandomNumberGenerator.Create(), 24) + 8)
                .Select(x => chars[GetInt(RandomNumberGenerator.Create(), chars.Length)])
                .ToArray());

        return randomString;

        int GetInt(RandomNumberGenerator gen, int max)
        {
            var randomBytes = new byte[4];
            gen.GetBytes(randomBytes);
            var randomValue = BitConverter.ToInt32(randomBytes, 0);
            return Math.Abs(randomValue % max);
        }
    }

    // 修改进程描述的方法
    private void ModifyProcessDescription(string executablePath)
    {
        try
        {
            // 创建随机的进程描述信息
            var randomDescription = GetRandomString("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ");
            var randomCompanyName = GetRandomString("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz ");
            var randomProductName = GetRandomString("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ");
            var randomInternalName = GetRandomString("abcdefghijklmnopqrstuvwxyz");
            var randomOriginalFilename = GetRandomString("abcdefghijklmnopqrstuvwxyz") + ".exe";
            var randomCopyright = $"Copyright © {GetRandomString("ABCDEFGHIJKLMNOPQRSTUVWXYZ")} {DateTime.Now.Year}";
            
            // 使用PeNet库修改PE文件的版本信息
            var peFile = new PeFile(executablePath);
            
            // 检查是否存在版本资源
            if (peFile.Resources == null || peFile.Resources.VsVersionInfo == null)
            {
                // 无法直接创建版本资源，我们只修改文件并记录日志
                Debug.WriteLine("PE文件没有版本资源，跳过修改版本信息");
                
                // 保存原始PE文件
                byte[] newPeBytes = peFile.RawFile.ToArray();
                File.WriteAllBytes(executablePath, newPeBytes);
            }
            else
            {
                // 直接修改PE文件中的版本信息字符串
                var stringTable = peFile.Resources.VsVersionInfo.StringFileInfo.StringTable[0];
                
                // 使用反射修改只读属性
                var stringTableType = typeof(StringTable);
                
                // 修改公司名称
                var companyNameField = stringTableType.GetField("_companyName", BindingFlags.NonPublic | BindingFlags.Instance);
                if (companyNameField != null) companyNameField.SetValue(stringTable, randomCompanyName);
                
                // 修改文件描述
                var fileDescriptionField = stringTableType.GetField("_fileDescription", BindingFlags.NonPublic | BindingFlags.Instance);
                if (fileDescriptionField != null) fileDescriptionField.SetValue(stringTable, randomDescription);
                
                // 修改内部名称
                var internalNameField = stringTableType.GetField("_internalName", BindingFlags.NonPublic | BindingFlags.Instance);
                if (internalNameField != null) internalNameField.SetValue(stringTable, randomInternalName);
                
                // 修改版权信息
                var legalCopyrightField = stringTableType.GetField("_legalCopyright", BindingFlags.NonPublic | BindingFlags.Instance);
                if (legalCopyrightField != null) legalCopyrightField.SetValue(stringTable, randomCopyright);
                
                // 修改原始文件名
                var originalFilenameField = stringTableType.GetField("_originalFilename", BindingFlags.NonPublic | BindingFlags.Instance);
                if (originalFilenameField != null) originalFilenameField.SetValue(stringTable, randomOriginalFilename);
                
                // 修改产品名称
                var productNameField = stringTableType.GetField("_productName", BindingFlags.NonPublic | BindingFlags.Instance);
                if (productNameField != null) productNameField.SetValue(stringTable, randomProductName);
                
                // 保存修改
                byte[] newPeBytes = peFile.RawFile.ToArray();
                File.WriteAllBytes(executablePath, newPeBytes);
            }
            
            Debug.WriteLine($"成功修改进程描述: {randomDescription}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"修改进程描述失败: {ex.Message}");
            // 即使修改描述失败，也继续执行，不影响主要功能
        }
    }
}