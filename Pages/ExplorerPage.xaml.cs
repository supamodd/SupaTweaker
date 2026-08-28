using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public partial class ExplorerPage : Page
{
    private bool _ready;

    public ExplorerPage()
    {
        InitializeComponent();
        const string adv = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        ShowExt.IsChecked = WinUtil.GetDword(adv, "HideFileExt", 1, false) == 0;
        ShowHidden.IsChecked = WinUtil.GetDword(adv, "Hidden", 2, false) == 1;
        ThisPc.IsChecked = WinUtil.GetDword(adv, "LaunchTo", 2, false) == 1;
        HideSearch.IsChecked = WinUtil.GetDword(@"Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", 1, false) == 0;
        ClassicContext.IsChecked = WinUtil.GetDword(@"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "", 0, false) == 0
            && Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32") != null;
        _ready = true;
    }

    private void OnToggle(object sender, RoutedEventArgs e)
    {
        if (!_ready) return;
        const string adv = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        WinUtil.SetDword(adv, "HideFileExt", ShowExt.IsChecked == true ? 0 : 1, false);
        WinUtil.SetDword(adv, "Hidden", ShowHidden.IsChecked == true ? 1 : 2, false);
        WinUtil.SetDword(adv, "LaunchTo", ThisPc.IsChecked == true ? 1 : 2, false);
        WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode", HideSearch.IsChecked == true ? 0 : 1, false);
        const string cls = @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}";
        if (ClassicContext.IsChecked == true)
            WinUtil.SetString(cls + @"\InprocServer32", "", "", false);
        else
            WinUtil.DeleteKey(cls, false);
        WinUtil.RefreshShellSoon(restartExplorer: true);
        MainWindow.Instance?.SetStatus("Проводник обновлён без выхода из системы");
    }
}
