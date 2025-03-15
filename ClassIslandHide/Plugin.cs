
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
            PInvoke.CreateHardLink(newPath, Path.GetFullPath(path));
            Settings.LastGhostExePath = newPath;

            var processStartInfo = new ProcessStartInfo()
            {
                FileName = newPath,
                WorkingDirectory = Environment.CurrentDirectory,
                ArgumentList = { "-m" }
            };
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
            PInvoke.CreateHardLink(Settings.RawExePath, Path.GetFullPath(path!));
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
}