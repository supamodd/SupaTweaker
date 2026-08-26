using System.Windows;
using System.Windows.Controls;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public partial class AdvancedPage : Page
{
    public AdvancedPage() => InitializeComponent();

    private void Apply(object s, RoutedEventArgs e)
    {
        if (Verbose.IsChecked == true)
            WinUtil.SetDword(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "VerboseStatus", 1);
        if (Seconds.IsChecked == true)
            WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSecondsInSystemClock", 1, false);
        if (EndTask.IsChecked == true)
            WinUtil.SetDword(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings", "TaskbarEndTask", 1, false);
        if (NumLock.IsChecked == true)
            WinUtil.SetDword(@"Control Panel\Keyboard", "InitialKeyboardIndicators", 2, false);
        MainWindow.Instance?.SetStatus("Дополнительные параметры применены");
    }
}
