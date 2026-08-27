using System.Windows;
using System.Windows.Controls;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public partial class UpdatePage : Page
{
    private bool _ready;
    public UpdatePage()
    {
        InitializeComponent();
        const string p = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU";
        Pause.IsChecked = WinUtil.GetDword(p, "NoAutoUpdate") == 1;
        Notify.IsChecked = WinUtil.GetDword(p, "AUOptions") == 2;
        NoDrivers.IsChecked = WinUtil.GetDword(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "ExcludeWUDriversInQualityUpdate") == 1;
        _ready = true;
    }

    private void OnToggle(object s, RoutedEventArgs e)
    {
        if (!_ready) return;
        const string p = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU";
        WinUtil.SetDword(p, "NoAutoUpdate", Pause.IsChecked == true ? 1 : 0);
        WinUtil.SetDword(p, "AUOptions", Notify.IsChecked == true ? 2 : 4);
        WinUtil.SetDword(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "ExcludeWUDriversInQualityUpdate", NoDrivers.IsChecked == true ? 1 : 0);
        WinUtil.NotifyWindows("Policy");
        MainWindow.Instance?.SetStatus("Политики обновлений применены сразу");
    }
}
