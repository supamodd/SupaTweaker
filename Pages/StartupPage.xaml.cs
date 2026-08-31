using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public class StartupItem
{
    public string Name { get; set; } = "";
    public string State { get; set; } = "";
    public string Source { get; set; } = "";
    public string Command { get; set; } = "";
    public string RunPath { get; set; } = "";
    public string ApprovedPath { get; set; } = "";
    public string Kind { get; set; } = "run";
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
        EnsureTaskManagerList();
        Load();
    }

    private void Refresh(object s, RoutedEventArgs e) => Load();

    private void Load()
    {
        var items = new List<StartupItem>();
        AddRun(items, @"Software\Microsoft\Windows\CurrentVersion\Run", false, "HKCU Run");
        AddRun(items, @"Software\Microsoft\Windows\CurrentVersion\RunOnce", false, "HKCU RunOnce");
        AddRun(items, @"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Run", false, "HKCU Run32");
        AddRun(items, @"Software\Microsoft\Windows\CurrentVersion\Run", true, "HKLM Run");
        AddRun(items, @"Software\Microsoft\Windows\CurrentVersion\RunOnce", true, "HKLM RunOnce");
        AddRun(items, @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Run", true, "HKLM Run32");
        AddRun(items, @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\RunOnce", true, "HKLM RunOnce32");
        AddApprovedOrphans(items, @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", false);
        AddApprovedOrphans(items, @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", true);
        AddFolder(items, Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Папка пользователя");
        AddFolder(items, Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), "Папка общая");
        AddScheduled(items);
        List.ItemsSource = items
            .GroupBy(x => x.Kind + "|" + x.Name + "|" + x.Source, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(x => x.Name)
            .ToList();
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
                if (string.IsNullOrWhiteSpace(name)) continue;
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
                    HiveLm = lm,
                    Kind = "run"
                });
            }
        }
        catch { }
    }

    private static void AddApprovedOrphans(List<StartupItem> list, string approved, bool lm)
    {
        try
        {
            var hive = lm ? Registry.LocalMachine : Registry.CurrentUser;
            using var k = hive.OpenSubKey(approved);
            if (k == null) return;
            foreach (var name in k.GetValueNames())
            {
                if (list.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && x.Kind == "run"))
                    continue;
                var on = IsEnabled(lm, approved, name);
                list.Add(new StartupItem
                {
                    Name = name,
                    Command = "(реестр StartupApproved)",
                    Source = lm ? "HKLM Approved" : "HKCU Approved",
                    State = on ? "Вкл" : "Выкл",
                    Enabled = on,
                    RunPath = @"Software\Microsoft\Windows\CurrentVersion\Run",
                    ApprovedPath = approved,
                    HiveLm = lm,
                    Kind = "run"
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
                HiveLm = false,
                Kind = "folder"
            });
        }
    }

    private static void AddScheduled(List<StartupItem> list)
    {
        try
        {
            var ty = Type.GetTypeFromProgID("Schedule.Service");
            if (ty == null) return;
            dynamic svc = Activator.CreateInstance(ty)!;
            svc.Connect();
            WalkTasks(list, svc.GetFolder(@"\"));
        }
        catch { }
    }

    private static void WalkTasks(List<StartupItem> list, dynamic folder)
    {
        try
        {
            foreach (var t in folder.GetTasks(1))
            {
                try
                {
                    string path = t.Path;
                    if (SkipTask(path)) continue;
                    if (!HasLogonOrBoot(t)) continue;
                    string cmd = "";
                    try
                    {
                        var actions = t.Definition.Actions;
                        if (actions.Count >= 1)
                            cmd = (actions[1].Path ?? "") + " " + (actions[1].Arguments ?? "");
                    }
                    catch { }
                    bool on = false;
                    try { on = t.Enabled; } catch { }
                    list.Add(new StartupItem
                    {
                        Name = t.Name,
                        Command = cmd.Trim(),
                        Source = "Планировщик",
                        State = on ? "Вкл" : "Выкл",
                        Enabled = on,
                        RunPath = path,
                        Kind = "task"
                    });
                }
                catch { }
            }
        }
        catch { }

        try
        {
            foreach (var f in folder.GetFolders(0))
                WalkTasks(list, f);
        }
        catch { }
    }

    private static bool SkipTask(string path)
    {
        if (string.IsNullOrEmpty(path)) return true;
        var p = path.Replace('/', '\\');
        if (p.StartsWith(@"\Microsoft\Windows\", StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    private static bool HasLogonOrBoot(dynamic task)
    {
        try
        {
            foreach (var tr in task.Definition.Triggers)
            {
                int type = (int)tr.Type;
                if (type is 8 or 9 or 11) return true;
            }
        }
        catch { }
        return false;
    }

    private static bool IsEnabled(bool lm, string approved, string name)
    {
        try
        {
            var hive = lm ? Registry.LocalMachine : Registry.CurrentUser;
            using var k = hive.OpenSubKey(approved);
            var v = k?.GetValue(name) as byte[];
            if (v == null || v.Length == 0) return true;
            return v[0] is 2 or 6;
        }
        catch { return true; }
    }

    private void SetEnabled(bool on)
    {
        if (List.SelectedItem is not StartupItem it) return;
        try
        {
            if (it.Kind == "task")
            {
                WinUtil.Run("schtasks.exe", $"/Change /TN \"{it.RunPath.TrimStart('\\')}\" {(on ? "/ENABLE" : "/DISABLE")}", true);
            }
            else
            {
                var hive = it.HiveLm ? Registry.LocalMachine : Registry.CurrentUser;
                using var k = hive.CreateSubKey(it.ApprovedPath, true);
                var valueName = it.Kind == "folder" ? Path.GetFileName(it.Command) : it.Name;
                k?.SetValue(valueName, on ? EnabledBlob : DisabledBlob, RegistryValueKind.Binary);
            }

            DisableRelated(it, on);
            Load();
            MainWindow.Instance?.SetStatus(on ? "Включено" : "Отключено");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private static void DisableRelated(StartupItem it, bool on)
    {
        var n = it.Name + " " + it.Command;
        if (n.Contains("Teams", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var task in new[]
                     {
                         @"Microsoft\Office\Office Automatic Updates 2.0",
                         "Teams",
                         "TeamsUpdateTaskUser"
                     })
            {
                WinUtil.Run("schtasks.exe", $"/Change /TN \"{task}\" {(on ? "/ENABLE" : "/DISABLE")}", true);
            }
        }
    }

    private void EnableSel(object s, RoutedEventArgs e) => SetEnabled(true);
    private void DisableSel(object s, RoutedEventArgs e) => SetEnabled(false);

    private static void EnsureTaskManagerList()
    {
        WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackProgs", 1, false);
        WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackDocs", 1, false);
        TryDeleteValue(false, @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoInstrumentation");
        TryDeleteValue(false, @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoStartMenuMFUprogramsList");
        TryDeleteValue(false, @"Software\Policies\Microsoft\Windows\Explorer", "NoStartMenuMFUprogramsList");
        TryDeleteValue(true, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoInstrumentation");
        TryDeleteValue(true, @"SOFTWARE\Policies\Microsoft\Windows\Explorer", "NoStartMenuMFUprogramsList");
    }

    private void RestoreList(object s, RoutedEventArgs e)
    {
        EnsureTaskManagerList();
        TryDeleteValue(false, @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoRun");
        TryDeleteValue(false, @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoStartup");
        TryDeleteValue(false, @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoCommonStartup");
        TryDeleteValue(true, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoRun");
        TryDeleteValue(true, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoStartup");
        TryDeleteValue(true, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoCommonStartup");
        TryDeleteValue(true, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "DisableStartupApps");
        WinUtil.NotifyWindows("Policy");
        Load();
        MainWindow.Instance?.SetStatus("Отслеживание автозагрузки включено. Закройте и откройте диспетчер задач.");
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
