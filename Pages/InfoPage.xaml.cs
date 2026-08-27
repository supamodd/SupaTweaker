using System.Globalization;
using System.IO;
using System.Management;
using System.Windows.Controls;
using Microsoft.Win32;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public class InfoRow
{
    public string Label { get; set; } = "";
    public string Value { get; set; } = "";
}

public class InfoGroup
{
    public string Title { get; set; } = "";
    public List<InfoRow> Rows { get; set; } = new();
}

public partial class InfoPage : Page
{
    public InfoPage()
    {
        InitializeComponent();
        try
        {
            var os = First("Win32_OperatingSystem", "Caption", "Version", "OSArchitecture", "LastBootUpTime", "RegisteredUser");
            var cs = First("Win32_ComputerSystem", "Manufacturer", "Model", "TotalPhysicalMemory", "NumberOfLogicalProcessors", "UserName");
            var cpu = First("Win32_Processor", "Name", "MaxClockSpeed", "NumberOfCores");
            var disk = BestDisk();
            var gpu = ReadGpu();

            PcName.Text = W(cs, "Model").Length > 1 ? $"{W(cs, "Manufacturer")} {W(cs, "Model")}" : Environment.MachineName;
            OsLine.Text = $"{W(os, "Caption")}  ·  {W(os, "OSArchitecture")}";

            Groups.ItemsSource = new List<InfoGroup>
            {
                new()
                {
                    Title = "Система",
                    Rows =
                    [
                        new() { Label = "Имя ПК", Value = Environment.MachineName },
                        new() { Label = "Пользователь", Value = W(cs, "UserName") },
                        new() { Label = "Windows", Value = W(os, "Caption") },
                        new() { Label = "Версия", Value = W(os, "Version") },
                        new() { Label = "Архитектура", Value = W(os, "OSArchitecture") },
                        new() { Label = "Последняя загрузка", Value = FormatBoot(W(os, "LastBootUpTime")) }
                    ]
                },
                new()
                {
                    Title = "Процессор и память",
                    Rows =
                    [
                        new() { Label = "Процессор", Value = W(cpu, "Name") },
                        new() { Label = "Ядра", Value = W(cpu, "NumberOfCores") },
                        new() { Label = "Потоки", Value = W(cs, "NumberOfLogicalProcessors") },
                        new() { Label = "Частота", Value = Mhz(W(cpu, "MaxClockSpeed")) },
                        new() { Label = "ОЗУ", Value = Bytes(W(cs, "TotalPhysicalMemory")) }
                    ]
                },
                new()
                {
                    Title = "Графика и диск",
                    Rows =
                    [
                        new() { Label = "Видеокарта", Value = gpu.Name },
                        new() { Label = "Память GPU", Value = gpu.Memory },
                        new() { Label = "Драйвер", Value = gpu.Driver },
                        new() { Label = "Том", Value = W(disk, "DeviceID") },
                        new() { Label = "Ёмкость", Value = Bytes(W(disk, "Size")) },
                        new() { Label = "Свободно", Value = Bytes(W(disk, "FreeSpace")) }
                    ]
                }
            };
        }
        catch (Exception ex)
        {
            PcName.Text = Environment.MachineName;
            OsLine.Text = ex.Message;
            Groups.ItemsSource = new List<InfoGroup>
            {
                new()
                {
                    Title = "Базовые сведения",
                    Rows =
                    [
                        new() { Label = "ОС", Value = Environment.OSVersion.ToString() },
                        new() { Label = "Потоки", Value = Environment.ProcessorCount.ToString() },
                        new() { Label = "Пользователь", Value = Environment.UserName }
                    ]
                }
            };
        }
    }

    private static ManagementObject? First(string cls, params string[] props)
    {
        using var s = new ManagementObjectSearcher($"SELECT {string.Join(",", props)} FROM {cls}");
        return s.Get().Cast<ManagementObject>().FirstOrDefault();
    }

    private static ManagementObject? BestDisk()
    {
        using var s = new ManagementObjectSearcher("SELECT DeviceID, Size, FreeSpace FROM Win32_LogicalDisk WHERE DriveType=3");
        return s.Get().Cast<ManagementObject>()
            .OrderByDescending(o => ulong.TryParse(o["Size"]?.ToString(), out var n) ? n : 0)
            .FirstOrDefault();
    }

    private static (string Name, string Memory, string Driver) ReadGpu()
    {
        var nv = TryNvidiaSmi();
        if (nv != null) return nv.Value;

        ManagementObject? best = null;
        foreach (ManagementObject o in new ManagementObjectSearcher(
                     "SELECT Name, AdapterRAM, DriverVersion FROM Win32_VideoController").Get())
        {
            var name = o["Name"]?.ToString() ?? "";
            if (name.Contains("Basic", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("Remote", StringComparison.OrdinalIgnoreCase))
                continue;
            if (best == null) best = o;
            if (name.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("GeForce", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("Radeon", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("AMD", StringComparison.OrdinalIgnoreCase))
            {
                best = o;
                break;
            }
        }

        var gpuName = W(best, "Name");
        var mem = RegistryVram();
        if (mem == 0 && ulong.TryParse(W(best, "AdapterRAM"), out var wmi)) mem = wmi;
        var driver = NvidiaDriverFromRegistry() ?? FormatWmiDriver(W(best, "DriverVersion"));
        return (gpuName, mem == 0 ? "—" : Bytes(mem.ToString()), driver);
    }

    private static (string Name, string Memory, string Driver)? TryNvidiaSmi()
    {
        string[] paths =
        [
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "NVIDIA Corporation", "NVSMI", "nvidia-smi.exe"),
            Path.Combine(Environment.SystemDirectory, "nvidia-smi.exe"),
            "nvidia-smi.exe"
        ];
        foreach (var p in paths)
        {
            try
            {
                var raw = WinUtil.RunOut(p, "--query-gpu=name,memory.total,driver_version --format=csv,noheader,nounits");
                var line = raw.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(line) || line.Contains("error", StringComparison.OrdinalIgnoreCase)) continue;
                var parts = line.Split(',').Select(x => x.Trim()).ToArray();
                if (parts.Length < 3) continue;
                var mib = double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var m) ? m : 0;
                if (mib <= 0) continue;
                return (parts[0], $"{mib / 1024d:0.##} ГБ", parts[2]);
            }
            catch { }
        }
        return null;
    }

    private static ulong RegistryVram()
    {
        ulong best = 0;
        try
        {
            using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var root = hklm.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}");
            if (root == null) return 0;
            foreach (var sub in root.GetSubKeyNames())
            {
                using var k = root.OpenSubKey(sub);
                if (k == null) continue;
                var desc = k.GetValue("DriverDesc")?.ToString() ?? "";
                if (desc.Contains("Basic", StringComparison.OrdinalIgnoreCase)) continue;
                var qw = k.GetValue("HardwareInformation.qwMemorySize");
                ulong v = 0;
                if (qw is long l) v = unchecked((ulong)l);
                else if (qw is int i) v = (ulong)i;
                else if (qw is byte[] b && b.Length >= 8) v = BitConverter.ToUInt64(b, 0);
                if (v > best) best = v;
            }
        }
        catch { }
        return best;
    }

    private static string? NvidiaDriverFromRegistry()
    {
        try
        {
            using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var display = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            if (display == null) return null;
            string? found = null;
            foreach (var sub in display.GetSubKeyNames())
            {
                using var u = display.OpenSubKey(sub);
                var name = u?.GetValue("DisplayName")?.ToString() ?? "";
                if (!name.Contains("NVIDIA Graphics Driver", StringComparison.OrdinalIgnoreCase)) continue;
                found = u?.GetValue("DisplayVersion")?.ToString();
            }
            return found;
        }
        catch { return null; }
    }

    private static string FormatWmiDriver(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || raw == "—") return "—";
        var p = raw.Split('.');
        if (p.Length == 4 && int.TryParse(p[2], out var a) && int.TryParse(p[3], out var b))
            return $"{a}.{b}";
        return raw;
    }

    private static string W(ManagementObject? o, string p)
    {
        try { return o?[p]?.ToString()?.Trim() ?? "—"; }
        catch { return "—"; }
    }

    private static string Bytes(string raw)
    {
        if (!ulong.TryParse(raw, out var n) || n == 0) return raw is "—" or "" ? "—" : raw;
        var gb = n / 1024d / 1024d / 1024d;
        return gb >= 1 ? $"{gb:0.##} ГБ" : $"{n / 1024d / 1024d:0.##} МБ";
    }

    private static string Mhz(string raw) =>
        int.TryParse(raw, out var n) && n > 0 ? $"{n / 1000.0:0.##} ГГц" : raw;

    private static string FormatBoot(string raw)
    {
        if (raw.Length < 14) return raw;
        if (DateTime.TryParseExact(raw[..14], "yyyyMMddHHmmss", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dt))
            return dt.ToString("dd.MM.yyyy  HH:mm");
        return raw;
    }
}
