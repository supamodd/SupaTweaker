using System.Windows;
using System.Windows.Controls;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public partial class ComponentsPage : Page
{
    public ComponentsPage() => InitializeComponent();
    private void Opt(object s, RoutedEventArgs e) => WinUtil.Run("optionalfeatures.exe", "");
    private void Appwiz(object s, RoutedEventArgs e) => WinUtil.Run("appwiz.cpl", "");
    private void Fod(object s, RoutedEventArgs e) => WinUtil.Run("ms-settings:optionalfeatures", "");
}
