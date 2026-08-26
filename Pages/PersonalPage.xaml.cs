using System.Windows;
using System.Windows.Controls;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public partial class PersonalPage : Page
{
    public PersonalPage() => InitializeComponent();

    private void Apply(object s, RoutedEventArgs e)
    {
        const string per = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        if (Dark.IsChecked == true) WinUtil.SetDword(per, "AppsUseLightTheme", 0, false);
        if (DarkSys.IsChecked == true) WinUtil.SetDword(per, "SystemUsesLightTheme", 0, false);
        WinUtil.SetDword(per, "EnableTransparency", Trans.IsChecked == true ? 1 : 0, false);
        const string adv = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        if (TaskLeft.IsChecked == true) WinUtil.SetDword(adv, "TaskbarAl", 0, false);
        if (HideWidgets.IsChecked == true) WinUtil.SetDword(adv, "TaskbarDa", 0, false);
        if (HideChat.IsChecked == true) WinUtil.SetDword(adv, "TaskbarMn", 0, false);
        MainWindow.Instance?.SetStatus("Персонализация применена. Может понадобиться выход из сеанса.");
    }
}
