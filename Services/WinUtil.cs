using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Win32;

namespace SupaTweaker.Services;

public static class WinUtil
{
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
}
