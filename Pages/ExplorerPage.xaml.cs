using System.Windows;
using System.Windows.Controls;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public partial class ExplorerPage : Page
{
    public ExplorerPage()
    {
        InitializeComponent();
        ShowExt.IsChecked = WinUtil.GetDword(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 1, false) == 0;
        ShowHidden.IsChecked = WinUtil.GetDword(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", 2, false) == 1;
    }

    private void OnChange(object sender, RoutedEventArgs e) { }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        const string adv = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        WinUtil.SetDword(adv, "HideFileExt", ShowExt.IsChecked == true ? 0 : 1, false);
        WinUtil.SetDword(adv, "Hidden", ShowHidden.IsChecked == true ? 1 : 2, false);
        WinUtil.SetDword(adv, "LaunchTo", ThisPc.IsChecked == true ? 1 : 2, false);
        if (ClassicContext.IsChecked == true)
            WinUtil.SetString(@"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "", "", false);
        if (HideSearch.IsChecked == true)
            WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", 0, false);
        MainWindow.Instance?.SetStatus("Параметры проводника записаны");
    }

    private void Restart_Click(object sender, RoutedEventArgs e)
    {
        WinUtil.RestartExplorer();
        MainWindow.Instance?.SetStatus("Explorer перезапущен");
    }
}
