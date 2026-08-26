using System.Windows;
using System.Windows.Controls;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public partial class SystemPage : Page
{
    public SystemPage() => InitializeComponent();

    private void Apply(object s, RoutedEventArgs e)
    {
        if (Hibernate.IsChecked == true) WinUtil.Run("powercfg.exe", "/h off", true);
        if (FastStart.IsChecked == true)
            WinUtil.SetDword(@"SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", 0);
        if (UacSoft.IsChecked == true)
            WinUtil.SetDword(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "ConsentPromptBehaviorAdmin", 5);
        MainWindow.Instance?.SetStatus("Системные параметры применены");
    }

    private void Restore(object s, RoutedEventArgs e)
    {
        WinUtil.Run("powershell.exe", "-NoProfile -Command \"Checkpoint-Computer -Description 'SupaTweaker' -RestorePointType 'MODIFY_SETTINGS'\"", true);
        MainWindow.Instance?.SetStatus("Запрошена точка восстановления");
    }

    private void Sysdm(object s, RoutedEventArgs e) => WinUtil.Run("control.exe", "sysdm.cpl");
}
