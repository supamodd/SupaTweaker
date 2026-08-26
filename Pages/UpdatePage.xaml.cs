using System.Windows;
using System.Windows.Controls;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public partial class UpdatePage : Page
{
    public UpdatePage() => InitializeComponent();

    private void Apply(object s, RoutedEventArgs e)
    {
        const string p = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU";
        if (Pause.IsChecked == true) WinUtil.SetDword(p, "NoAutoUpdate", 1);
        if (Notify.IsChecked == true) WinUtil.SetDword(p, "AUOptions", 2);
        if (NoDrivers.IsChecked == true)
            WinUtil.SetDword(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "ExcludeWUDriversInQualityUpdate", 1);
        MainWindow.Instance?.SetStatus("Политики обновлений применены");
    }

    private void Reset(object s, RoutedEventArgs e)
    {
        WinUtil.SetDword(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 0);
        WinUtil.SetDword(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "AUOptions", 4);
        MainWindow.Instance?.SetStatus("Политики сброшены");
    }
}
