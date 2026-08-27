using System.Windows;
using System.Windows.Controls;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public partial class PersonalPage : Page
{
    private bool _ready;

    public PersonalPage()
    {
        InitializeComponent();
        const string per = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        const string adv = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        Dark.IsChecked = WinUtil.GetDword(per, "AppsUseLightTheme", 1, false) == 0;
        DarkSys.IsChecked = WinUtil.GetDword(per, "SystemUsesLightTheme", 1, false) == 0;
        Trans.IsChecked = WinUtil.GetDword(per, "EnableTransparency", 1, false) == 1;
        HideWidgets.IsChecked = WinUtil.GetDword(adv, "TaskbarDa", 1, false) == 0;
        HideChat.IsChecked = WinUtil.GetDword(adv, "TaskbarMn", 1, false) == 0;
        if (WinUtil.GetDword(adv, "TaskbarAl", 1, false) == 0) AlignLeft.IsChecked = true;
        else AlignCenter.IsChecked = true;
        _ready = true;
    }

    private void OnToggle(object s, RoutedEventArgs e)
    {
        if (!_ready) return;
        const string per = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        const string adv = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        WinUtil.SetDword(per, "AppsUseLightTheme", Dark.IsChecked == true ? 0 : 1, false);
        WinUtil.SetDword(per, "SystemUsesLightTheme", DarkSys.IsChecked == true ? 0 : 1, false);
        WinUtil.SetDword(per, "EnableTransparency", Trans.IsChecked == true ? 1 : 0, false);
        WinUtil.SetDword(adv, "TaskbarDa", HideWidgets.IsChecked == true ? 0 : 1, false);
        WinUtil.SetDword(adv, "TaskbarMn", HideChat.IsChecked == true ? 0 : 1, false);
        WinUtil.NotifyWindows("ImmersiveColorSet");
        WinUtil.RefreshShellSoon();
        MainWindow.Instance?.SetStatus("Тема и панель применены сразу");
    }

    private void AlignChanged(object s, RoutedEventArgs e)
    {
        if (!_ready) return;
        var left = AlignLeft.IsChecked == true;
        WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", left ? 0 : 1, false);
        WinUtil.RefreshShellSoon();
        MainWindow.Instance?.SetStatus(left ? "Панель задач: влево" : "Панель задач: по центру");
    }
}
