using System.Windows;
using System.Windows.Controls;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public partial class PerfPage : Page
{
    private bool _ready;
    public PerfPage()
    {
        InitializeComponent();
        BestPerf.IsChecked = WinUtil.GetDword(@"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", "VisualFXSetting", 0, false) == 2;
        GameMode.IsChecked = WinUtil.GetDword(@"Software\Microsoft\GameBar", "AllowAutoGameMode", 0, false) == 1;
        _ready = true;
    }

    private void OnToggle(object s, RoutedEventArgs e)
    {
        if (!_ready) return;
        WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", "VisualFXSetting", BestPerf.IsChecked == true ? 2 : 0, false);
        WinUtil.SetDword(@"Software\Microsoft\GameBar", "AllowAutoGameMode", GameMode.IsChecked == true ? 1 : 0, false);
        WinUtil.Run("sc.exe", SysMain.IsChecked == true ? "stop SysMain" : "start SysMain");
        WinUtil.Run("powercfg.exe", HighPow.IsChecked == true
            ? "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c"
            : "/setactive SCHEME_BALANCED");
        WinUtil.NotifyWindows("Policy");
        MainWindow.Instance?.SetStatus("Производительность применена сразу");
    }
}
