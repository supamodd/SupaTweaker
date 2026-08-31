using System.Windows;
using System.Windows.Controls;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public partial class SettingsPage : Page
{
    private bool _ready;

    public SettingsPage()
    {
        InitializeComponent();
        AutoStart.IsChecked = AppSettings.AutoStart;
        _ready = true;
    }

    private void AutoChanged(object sender, RoutedEventArgs e)
    {
        if (!_ready) return;
        AppSettings.AutoStart = AutoStart.IsChecked == true;
        MainWindow.Instance?.SetStatus(AppSettings.AutoStart
            ? "Автозапуск включён"
            : "Автозапуск выключен");
    }
}
