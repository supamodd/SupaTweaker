using System.Windows;
using System.Windows.Controls;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public partial class SystemPage : Page
{
    private bool _ready;
    public SystemPage()
    {
        InitializeComponent();
        Hibernate.IsChecked = WinUtil.GetDword(@"SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", 1) == 0;
        FastStart.IsChecked = WinUtil.GetDword(@"SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", 1) == 0;
        UacSoft.IsChecked = WinUtil.GetDword(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "ConsentPromptBehaviorAdmin", 5) == 5;
        _ready = true;
    }

    private void OnToggle(object s, RoutedEventArgs e)
    {
        if (!_ready) return;
        WinUtil.Run("powercfg.exe", Hibernate.IsChecked == true ? "/h off" : "/h on", true);
        WinUtil.SetDword(@"SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", FastStart.IsChecked == true ? 0 : 1);
        WinUtil.SetDword(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "ConsentPromptBehaviorAdmin", UacSoft.IsChecked == true ? 5 : 2);
        WinUtil.NotifyWindows("Policy");
        MainWindow.Instance?.SetStatus("Системные параметры применены сразу");
    }

    private void Restore(object s, RoutedEventArgs e)
    {
        WinUtil.Run("powershell.exe", "-NoProfile -Command \"Checkpoint-Computer -Description 'SupaTweaker' -RestorePointType 'MODIFY_SETTINGS'\"", true);
        MainWindow.Instance?.SetStatus("Запрошена точка восстановления");
    }

    private void Sysdm(object s, RoutedEventArgs e) => WinUtil.Run("control.exe", "sysdm.cpl");
}
