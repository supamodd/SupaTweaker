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
        switch (AppSettings.Theme)
        {
            case ThemeService.Dark: ThemeDark.IsChecked = true; break;
            case ThemeService.Light: ThemeLight.IsChecked = true; break;
            default: ThemeStd.IsChecked = true; break;
        }
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

    private void ThemeClick(object sender, RoutedEventArgs e)
    {
        if (!_ready) return;
        var id = ThemeDark.IsChecked == true ? ThemeService.Dark
            : ThemeLight.IsChecked == true ? ThemeService.Light
            : ThemeService.Standard;
        ThemeService.Apply(id);
        MainWindow.Instance?.SetStatus("Тема: " + (id == ThemeService.Dark ? "тёмная" : id == ThemeService.Light ? "светлая" : "стандартная"));
    }
}
