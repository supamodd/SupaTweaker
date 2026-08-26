using System.Management;
using System.Windows.Controls;

namespace SupaTweaker.Pages;

public partial class InfoPage : Page
{
    public InfoPage()
    {
        InitializeComponent();
        try
        {
            var os = Query("Win32_OperatingSystem", "Caption", "Version", "OSArchitecture", "LastBootUpTime");
            var cs = Query("Win32_ComputerSystem", "Manufacturer", "Model", "TotalPhysicalMemory", "NumberOfLogicalProcessors");
            var cpu = Query("Win32_Processor", "Name");
            Info.Text = os + "\n" + cs + "\n" + cpu;
        }
        catch (Exception ex)
        {
            Info.Text = "WMI недоступен: " + ex.Message + "\n\n" +
                        Environment.OSVersion + "\n" +
                        Environment.MachineName + "\n" +
                        Environment.ProcessorCount + " логических процессоров\n" +
                        Environment.UserName;
        }
    }

    private static string Query(string cls, params string[] props)
    {
        var lines = new List<string> { $"[{cls}]" };
        using var s = new ManagementObjectSearcher($"SELECT {string.Join(",", props)} FROM {cls}");
        foreach (var o in s.Get())
        {
            foreach (var p in props)
                lines.Add($"  {p}: {o[p]}");
        }
        return string.Join('\n', lines);
    }
}
