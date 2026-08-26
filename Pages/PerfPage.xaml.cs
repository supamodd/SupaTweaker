using System.Windows;
using System.Windows.Controls;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public partial class PerfPage : Page
{
    public PerfPage() => InitializeComponent();

    private void Apply(object s, RoutedEventArgs e)
    {
        if (BestPerf.IsChecked == true)
            WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", "VisualFXSetting", 2, false);
        if (GameMode.IsChecked == true)
            WinUtil.SetDword(@"Software\Microsoft\GameBar", "AllowAutoGameMode", 1, false);
        if (SysMain.IsChecked == true)
            WinUtil.Run("sc.exe", "stop SysMain");
        if (HighPow.IsChecked == true)
            WinUtil.Run("powercfg.exe", "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
        MainWindow.Instance?.SetStatus("Параметры производительности применены");
    }
}
