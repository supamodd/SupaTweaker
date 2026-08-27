using System.Globalization;
using System.Management;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Xml.Linq;

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
            var gpu = First("Win32_VideoController", "Name", "AdapterRAM", "DriverVersion");
            var disk = First("Win32_LogicalDisk", "DeviceID", "Size", "FreeSpace");

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
                        new() { Label = "Видеокарта", Value = W(gpu, "Name") },
                        new() { Label = "Память GPU", Value = Bytes(W(gpu, "AdapterRAM")) },
                        new() { Label = "Драйвер", Value = W(gpu, "DriverVersion") },
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
