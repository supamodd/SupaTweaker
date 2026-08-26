using System.Windows;
using System.Windows.Controls;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public partial class QuickPage : Page
{
    public QuickPage() => InitializeComponent();

    private void Go(object s, RoutedEventArgs e)
    {
        if (A.IsChecked == true)
            WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0, false);
        if (B.IsChecked == true)
            WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 1, false);
        if (C.IsChecked == true)
        {
            WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 0, false);
            WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 0, false);
        }
        if (D.IsChecked == true)
            WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", 0, false);
        if (E.IsChecked == true)
            WinUtil.Run("powercfg.exe", "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
        if (F.IsChecked == true)
            WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", 0, false);
        MainWindow.Instance?.SetStatus("Быстрая настройка выполнена");
    }
}
