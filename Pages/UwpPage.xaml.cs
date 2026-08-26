using System.Windows;
using System.Windows.Controls;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public partial class UwpPage : Page
{
    public UwpPage()
    {
        InitializeComponent();
        Refresh(null!, null!);
    }

    private void Refresh(object s, RoutedEventArgs e)
    {
        var raw = WinUtil.RunOut("powershell.exe",
            "-NoProfile -Command \"Get-AppxPackage | Where-Object { -not $_.IsFramework } | Select-Object -ExpandProperty Name\"");
        Apps.Items.Clear();
        foreach (var line in raw.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).OrderBy(x => x))
            Apps.Items.Add(line);
        MainWindow.Instance?.SetStatus($"Найдено пакетов: {Apps.Items.Count}");
    }

    private void Remove(object s, RoutedEventArgs e)
    {
        foreach (var item in Apps.SelectedItems.Cast<string>().ToList())
        {
            WinUtil.Run("powershell.exe",
                $"-NoProfile -Command \"Get-AppxPackage -Name '{item.Replace("'", "''")}' | Remove-AppxPackage\"", true);
        }
        Refresh(s, e);
        MainWindow.Instance?.SetStatus("Удаление запрошено");
    }
}
