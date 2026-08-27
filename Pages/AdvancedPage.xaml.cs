using System.Windows;
using System.Windows.Controls;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public partial class AdvancedPage : Page
{
    private bool _ready;
    public AdvancedPage()
    {
        InitializeComponent();
        Verbose.IsChecked = WinUtil.GetDword(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "VerboseStatus") == 1;
        Seconds.IsChecked = WinUtil.GetDword(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSecondsInSystemClock", 0, false) == 1;
        EndTask.IsChecked = WinUtil.GetDword(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings", "TaskbarEndTask", 0, false) == 1;
        NumLock.IsChecked = WinUtil.GetDword(@"Control Panel\Keyboard", "InitialKeyboardIndicators", 0, false) == 2;
        _ready = true;
    }

    private void OnToggle(object s, RoutedEventArgs e)
    {
        if (!_ready) return;
        WinUtil.SetDword(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "VerboseStatus", Verbose.IsChecked == true ? 1 : 0);
        WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSecondsInSystemClock", Seconds.IsChecked == true ? 1 : 0, false);
        WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings", "TaskbarEndTask", EndTask.IsChecked == true ? 1 : 0, false);
        WinUtil.SetDword(@"Control Panel\Keyboard", "InitialKeyboardIndicators", NumLock.IsChecked == true ? 2 : 0, false);
        WinUtil.RefreshShellSoon();
        MainWindow.Instance?.SetStatus("Дополнительно применено сразу");
    }
}
