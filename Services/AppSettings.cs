using Microsoft.Win32;

namespace SupaTweaker.Services;

public static class AppSettings
{
    private const string Key = @"Software\SupaTweaker";
    private const string TaskName = "SupaTweaker";

    public static bool AutoStart
    {
        get => WinUtil.GetDword(Key, "AutoStart", 0, false) == 1;
        set
        {
            WinUtil.SetDword(Key, "AutoStart", value ? 1 : 0, false);
            ApplyAutoStart(value);
        }
    }

    public static void ApplyAutoStart(bool on)
    {
        var exe = Environment.ProcessPath ?? System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
        if (string.IsNullOrWhiteSpace(exe)) return;
        if (on)
        {
            WinUtil.Run("schtasks.exe",
                $"/Create /TN \"{TaskName}\" /TR \"\\\"{exe}\\\"\" /SC ONLOGON /RL HIGHEST /F",
                true);
            using var run = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            run?.SetValue("SupaTweaker", $"\"{exe}\"");
        }
        else
        {
            WinUtil.Run("schtasks.exe", $"/Delete /TN \"{TaskName}\" /F", true);
            try
            {
                using var run = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                run?.DeleteValue("SupaTweaker", false);
            }
            catch { }
        }
    }
}
