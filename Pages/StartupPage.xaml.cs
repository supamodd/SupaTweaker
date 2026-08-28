using Microsoft.Win32;
using SupaTweaker.Services;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SupaTweaker.Pages;

public class StartupItem
{
    public string Name { get; set; } = "";
    public string State { get; set; } = "";
    public string Source { get; set; } = "";
    public string Command { get; set; } = "";
    public string RunPath { get; set; } = "";
    public string ApprovedPath { get; set; } = "";
    public bool HiveLm { get; set; }
    public bool Enabled { get; set; }
}

public partial class StartupPage : Page
{
    private static readonly byte[] EnabledBlob = [0x02, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
    private static readonly byte[] DisabledBlob = [0x03, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

    public StartupPage()
    {
        InitializeComponent();
        Load();
    }

    private void Refresh(object s, RoutedEventArgs e) => Load();

    private void Load()
    {
        var items = new List<StartupItem>();
        AddRun(items, @"Software\Microsoft\Windows\CurrentVersion\Run", false, "HKCU Run");
        AddRun(items, @"Software\Microsoft\Windows\CurrentVersion\Run", true, "HKLM Run");
        AddRun(items, @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Run", true, "HKLM Run32");
        AddFolder(items, Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Папка пользователя");
        AddFolder(items, Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), "Папка общая");
        List.ItemsSource = items.OrderBy(x => x.Name).ToList();
        MainWindow.Instance?.SetStatus($"Записей автозагрузки: {items.Count}");
    }

    private static void AddRun(List<StartupItem> list, string path, bool lm, string source)
    {
        try
        {
            var hive = lm ? Registry.LocalMachine : Registry.CurrentUser;
            using var key = hive.OpenSubKey(path);
            if (key == null) return;
            var approvedRel = path.Contains("WOW6432Node", StringComparison.OrdinalIgnoreCase)
                ? @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run32"
                : @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
            foreach (var name in key.GetValueNames())
            {
                var cmd = key.GetValue(name)?.ToString() ?? "";
                var on = IsEnabled(lm, approvedRel, name);
                list.Add(new StartupItem
                {
                    Name = name,
                    Command = cmd,
                    Source = source,
                    State = on ? "Вкл" : "Выкл",
                    Enabled = on,
                    RunPath = path,
                    ApprovedPath = approvedRel,
                    HiveLm = lm
                });
            }
        }
        catch { }
    }

    private static void AddFolder(List<StartupItem> list, string dir, string source)
    {
        if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir)) return;
        const string approved = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\StartupFolder";
        foreach (var file in Directory.GetFiles(dir, "*.*"))
        {
            var name = Path.GetFileName(file);
            if (name.Equals("desktop.ini", StringComparison.OrdinalIgnoreCase)) continue;
            var on = IsEnabled(false, approved, name);
            list.Add(new StartupItem
            {
                Name = Path.GetFileNameWithoutExtension(file),
                Command = file,
                Source = source,
                State = on ? "Вкл" : "Выкл",
                Enabled = on,
                RunPath = dir,
                ApprovedPath = approved,
                HiveLm = false
            });
        }
    }

    private static bool IsEnabled(bool lm, string approved, string name)
    {
        try
        {
            var hive = lm ? Registry.LocalMachine : Registry.CurrentUser;
            using var k = hive.OpenSubKey(approved);
            var v = k?.GetValue(name) as byte[];
            if (v == null || v.Length == 0) return true;
            return v[0] == 2 || v[0] == 6;
        }
        catch { return true; }
    }

    private void SetEnabled(bool on)
    {
        if (List.SelectedItem is not StartupItem it) return;
        try
        {
            var hive = it.HiveLm ? Registry.LocalMachine : Registry.CurrentUser;
            using var k = hive.CreateSubKey(it.ApprovedPath, true);
            k?.SetValue(it.Name.Contains('.') && it.Source.Contains("Папка")
                    ? Path.GetFileName(it.Command)
                    : it.Name,
                on ? EnabledBlob : DisabledBlob, RegistryValueKind.Binary);
            Load();
            MainWindow.Instance?.SetStatus(on ? "Включено" : "Отключено");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void EnableSel(object s, RoutedEventArgs e) => SetEnabled(true);
    private void DisableSel(object s, RoutedEventArgs e) => SetEnabled(false);

    private void RestoreList(object s, RoutedEventArgs e)
    {
        WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", 1, false);
        TryDeleteValue(false, @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoRun");
        TryDeleteValue(false, @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoStartup");
        TryDeleteValue(false, @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoCommonStartup");
        TryDeleteValue(true, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoRun");
        TryDeleteValue(true, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoStartup");
        TryDeleteValue(true, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoCommonStartup");
        TryDeleteValue(true, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "DisableStartupApps");
        WinUtil.NotifyWindows("Policy");
        Load();
        MainWindow.Instance?.SetStatus("Отслеживание автозагрузки включено. Откройте диспетчер задач заново.");
    }

    private static void TryDeleteValue(bool lm, string path, string name)
    {
        try
        {
            var hive = lm ? Registry.LocalMachine : Registry.CurrentUser;
            using var k = hive.OpenSubKey(path, true);
            k?.DeleteValue(name, false);
        }
        catch { }
    }

    private void OpenFolder(object s, RoutedEventArgs e)
    {
        var dir = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        Process.Start(new ProcessStartInfo("explorer.exe", dir) { UseShellExecute = true });
    }
}
