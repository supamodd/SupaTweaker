using System.Windows;
using System.Windows.Controls;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public partial class QuickPage : Page
{
    private bool _ready;
    public QuickPage()
    {
        InitializeComponent();
        const string adv = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        A.IsChecked = WinUtil.GetDword(adv, "HideFileExt", 1, false) == 0;
        B.IsChecked = WinUtil.GetDword(adv, "LaunchTo", 2, false) == 1;
        C.IsChecked = WinUtil.GetDword(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1, false) == 0;
        if (WinUtil.GetDword(adv, "TaskbarAl", 1, false) == 0) AlignLeft.IsChecked = true;
        else AlignCenter.IsChecked = true;
        F.IsChecked = WinUtil.GetDword(@"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", 1, false) == 0;
        _ready = true;
    }

    private void OnToggle(object s, RoutedEventArgs e)
    {
        if (!_ready) return;
        WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", A.IsChecked == true ? 0 : 1, false);
        WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", B.IsChecked == true ? 1 : 2, false);
        var dark = C.IsChecked == true ? 0 : 1;
        WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", dark, false);
        WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", dark, false);
        if (E.IsChecked == true)
            WinUtil.Run("powercfg.exe", "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
        else
            WinUtil.Run("powercfg.exe", "/setactive SCHEME_BALANCED");
        WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", F.IsChecked == true ? 0 : 1, false);
        WinUtil.NotifyWindows("ImmersiveColorSet");
        WinUtil.RefreshShellSoon();
        MainWindow.Instance?.SetStatus("Быстрая настройка применена сразу");
    }

    private void Align(object s, RoutedEventArgs e)
    {
        if (!_ready) return;
        WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", AlignLeft.IsChecked == true ? 0 : 1, false);
        WinUtil.RefreshShellSoon();
        MainWindow.Instance?.SetStatus(AlignLeft.IsChecked == true ? "Панель: влево" : "Панель: по центру");
    }
}
