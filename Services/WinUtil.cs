using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Threading;
using Microsoft.Win32;

namespace SupaTweaker.Services;

public static class WinUtil
{
    private const uint WmSettingChange = 0x001A;
    private const uint SmtoAbortIfHung = 0x0002;
    private static readonly IntPtr HwndBroadcast = new(0xFFFF);
    private static DispatcherTimer? _explorerTimer;

    public static bool IsAdmin()
    {
        using var id = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static void SetDword(string path, string name, int value, bool localMachine = true)
    {
        var hive = localMachine ? Registry.LocalMachine : Registry.CurrentUser;
        using var key = hive.CreateSubKey(path, true);
        key?.SetValue(name, value, RegistryValueKind.DWord);
    }

    public static void SetString(string path, string name, string value, bool localMachine = false)
    {
        var hive = localMachine ? Registry.LocalMachine : Registry.CurrentUser;
        using var key = hive.CreateSubKey(path, true);
        key?.SetValue(name, value, RegistryValueKind.String);
    }

    public static int GetDword(string path, string name, int fallback = 0, bool localMachine = true)
    {
        try
        {
            var hive = localMachine ? Registry.LocalMachine : Registry.CurrentUser;
            using var key = hive.OpenSubKey(path);
            var v = key?.GetValue(name);
            return v is int i ? i : fallback;
        }
        catch { return fallback; }
    }

    public static void DeleteKey(string path, bool localMachine = false)
    {
        try
        {
            var hive = localMachine ? Registry.LocalMachine : Registry.CurrentUser;
            hive.DeleteSubKeyTree(path, false);
        }
        catch { }
    }

    public static void NotifyWindows(string area = "ImmersiveColorSet")
    {
        SendMessageTimeout(HwndBroadcast, WmSettingChange, IntPtr.Zero, area, SmtoAbortIfHung, 100, out _);
        SendMessageTimeout(HwndBroadcast, WmSettingChange, IntPtr.Zero, "Policy", SmtoAbortIfHung, 100, out _);
        SHChangeNotify(0x08000000, 0x1000, IntPtr.Zero, IntPtr.Zero);
    }

    public static void RefreshShellSoon()
    {
        NotifyWindows();
        if (_explorerTimer == null)
        {
            _explorerTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(450) };
            _explorerTimer.Tick += (_, _) =>
            {
                _explorerTimer.Stop();
                RestartExplorer();
            };
        }
        _explorerTimer.Stop();
        _explorerTimer.Start();
    }

    public static void Run(string file, string args, bool wait = false)
    {
        var psi = new ProcessStartInfo(file, args)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        using var p = Process.Start(psi);
        if (wait) p?.WaitForExit(120_000);
    }

    public static string RunOut(string file, string args)
    {
        var psi = new ProcessStartInfo(file, args)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        using var p = Process.Start(psi);
        return p?.StandardOutput.ReadToEnd() ?? "";
    }

    public static void RestartExplorer()
    {
        foreach (var p in Process.GetProcessesByName("explorer"))
        {
            try { p.Kill(); } catch { }
        }
        Process.Start(new ProcessStartInfo("explorer.exe") { UseShellExecute = true });
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, string lParam, uint flags, uint timeout, out IntPtr result);

    [DllImport("shell32.dll")]
    private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
}
